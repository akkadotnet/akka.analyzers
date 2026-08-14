// -----------------------------------------------------------------------
//  <copyright file="ShouldUseImmutableEnumerableForStreamAggregateAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Akka.Analyzers.Context;
using Akka.Analyzers.Context.Streams;
using Akka.Analyzers.Context.System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShouldUseImmutableEnumerableForStreamAggregateAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak2007MustUseImmutableEnumerableForStreamAggregate)
{
    private static readonly Version RelevantVersion = new(1, 5, 0, 0);
    
    protected override bool ShouldAnalyze(AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(akkaContext);
        
        // Akka.Streams has to be installed and the version of it used has to be greater than or equal to v1.5.0
        return akkaContext.HasAkkaStreamsInstalled && akkaContext.AkkaStreams.Version >= RelevantVersion;
    }

    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        // Hoisted out of the per-node action, which built a ten-element List and copied it into an
        // ImmutableArray per call site.
        var aggregateMethods = akkaContext.AkkaStreams.GetAllAggregateMethods();
        var aggregateMethodNames = aggregateMethods.MethodNames();
        if (aggregateMethodNames.IsEmpty)
            return;

        context.RegisterSyntaxNodeAction(ctx =>
        {
            AnalyzeInvocationExpression(ctx, akkaContext, aggregateMethods, aggregateMethodNames);
        }, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocationExpression(
        SyntaxNodeAnalysisContext ctx,
        AkkaContext akkaContext,
        ImmutableArray<IMethodSymbol> aggregateMethods,
        ImmutableHashSet<string> aggregateMethodNames)
    {
        var invocationExpr = (InvocationExpressionSyntax)ctx.Node;

        // Reject by name before binding.
        if (!invocationExpr.CouldInvokeAnyOf(aggregateMethodNames))
            return;

        var semanticModel = ctx.SemanticModel;
        
        if (semanticModel.GetSymbolInfo(invocationExpr).Symbol is not IMethodSymbol methodSymbol)
            return;

        // Check if this is an aggregate method from any of the Streams contexts
        if (!IsAggregateMethod(methodSymbol, aggregateMethods))
            return;

        // Get the zero parameter type by checking the method signature
        var zeroParameterType = GetZeroParameterType(methodSymbol);
        if (zeroParameterType == null)
            return;
        
        // Check if the zero parameter type is an enumerable
        if (!IsEnumerable(zeroParameterType, akkaContext))
            return;

        // Check if the enumerable is immutable
        if (IsImmutableEnumerable(zeroParameterType, akkaContext))
            return;

        // Report diagnostic - the enumerable is mutable
        ctx.ReportDiagnostic(Diagnostic.Create(
            RuleDescriptors.Ak2007MustUseImmutableEnumerableForStreamAggregate,
            invocationExpr.GetLocation()));
    }

    private static ITypeSymbol? GetZeroParameterType(IMethodSymbol methodSymbol)
    {
        // Get the original definition to work with the unconstructed method
        var originalMethod = methodSymbol.ConstructedFrom;
        while (!ReferenceEquals(originalMethod, originalMethod.ConstructedFrom))
            originalMethod = originalMethod.ConstructedFrom;

        // For static methods, we need to check ReducedFrom to get the original method
        if (originalMethod is { IsStatic: true, ReducedFrom: not null })
        {
            originalMethod = originalMethod.ReducedFrom;
        }

        // Find the parameter that represents the zero value
        // For aggregate methods, this is typically the first parameter after any source/flow parameters
        for (int i = 0; i < originalMethod.Parameters.Length; i++)
        {
            var parameter = originalMethod.Parameters[i];
            
            // Look for parameters with names like "zero", "seed", "initial", etc.
            if (parameter.Name.Equals("zero", StringComparison.OrdinalIgnoreCase) ||
                parameter.Name.Equals("seed", StringComparison.OrdinalIgnoreCase) ||
                parameter.Name.Equals("initial", StringComparison.OrdinalIgnoreCase) ||
                parameter.Name.Equals("initialValue", StringComparison.OrdinalIgnoreCase))
            {
                // Map the original parameter type to the constructed type
                var originalParameterType = parameter.Type;
                var constructedParameterType = methodSymbol.Parameters[i].Type;
                
                // If the original type is a type parameter, find the corresponding type argument
                if (originalParameterType is ITypeParameterSymbol typeParam)
                {
                    var typeParamIndex = originalMethod.TypeParameters.IndexOf(typeParam);
                    if (typeParamIndex >= 0 && typeParamIndex < methodSymbol.TypeArguments.Length)
                    {
                        return methodSymbol.TypeArguments[typeParamIndex];
                    }
                }
                
                // If it's not a type parameter, return the constructed type
                return constructedParameterType;
            }
        }

        // Fallback: if we can't find a named parameter, assume that:
        // * the second type parameter is the init type if it is a static method.
        // * the first type parameter is the init type if it is NOT a static method.
        // This is a reasonable assumption for most aggregate methods
        if(methodSymbol is { IsStatic: true, TypeArguments.Length: > 1 })
            return methodSymbol.TypeArguments[1];
        
        if (methodSymbol.TypeArguments.Length > 0)
            return methodSymbol.TypeArguments[0];

        return null;
    }

    private static bool IsAggregateMethod(IMethodSymbol methodSymbol, ImmutableArray<IMethodSymbol> aggregateMethods)
    {
        var originalMethod = methodSymbol;
        // For static methods, we need to check ReducedFrom to get the original method
        while (originalMethod.ReducedFrom is not null)
            originalMethod = originalMethod.ReducedFrom;

        // Get the original definition to compare against
        while (!ReferenceEquals(originalMethod, originalMethod.ConstructedFrom))
            originalMethod = originalMethod.ConstructedFrom;

        // Convert to the generic method definition
        var genericMethod = originalMethod.OriginalDefinition;

        // Check if this method is any of the aggregate methods from our contexts (compare generic definitions)
        return aggregateMethods.Any(m => SymbolEqualityComparer.Default.Equals(genericMethod, m.OriginalDefinition));
    }

    // Update IsEnumerable to use the extension methods
    private static bool IsEnumerable(ITypeSymbol typeSymbol, AkkaContext akkaContext)
    {
        var iEnumerableType = akkaContext.SystemCollections.IEnumerableType;
        var iEnumerableGenericType = akkaContext.SystemCollections.IEnumerableGenericType;
        if (iEnumerableType == null || iEnumerableGenericType == null)
            return false;
        return typeSymbol.IsDerivedOrImplements(iEnumerableType) ||
               typeSymbol.IsDerivedOrImplements(iEnumerableGenericType);
    }

    private static bool IsImmutableEnumerable(ITypeSymbol typeSymbol, AkkaContext akkaContext)
    {
        // Allow string as an immutable enumerable
        if (typeSymbol.SpecialType == SpecialType.System_String)
            return true;

        // Allow IImmutable* interfaces from System.Collections.Immutable
        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.ContainingNamespace.ToDisplayString() == "System.Collections.Immutable" &&
            namedType.Name.StartsWith("IImmutable", StringComparison.Ordinal))
            return true;

        // Get the immutable collection types from the context
        var immutableCollectionTypes = akkaContext.SystemCollectionsImmutable.GetAllImmutableCollectionTypes();
        
        // Check if the type is any of the immutable collection types
        return immutableCollectionTypes.Any(immutableType => 
            SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, immutableType));
    }
} 