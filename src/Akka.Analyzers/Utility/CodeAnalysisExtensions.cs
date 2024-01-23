// -----------------------------------------------------------------------
//  <copyright file="CodeAnalysisExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Akka.Analyzers;

internal static class CodeAnalysisExtensions
{
    // public static bool IsInsidePropsCreateMethod(this IOperation operation,
    //     AkkaContext akkaContext)
    // {
    //     Guard.AssertIsNotNull(operation);
    //     Guard.AssertIsNotNull(akkaContext);
    //
    //     if (akkaContext.AkkaCore.PropsType is null)
    //         return false;
    //
    //     var semanticModel = operation.SemanticModel;
    //     if (semanticModel is null)
    //         return false;
    // }

    public static bool IsActorBaseSubclass(this INamedTypeSymbol typeSymbol, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(typeSymbol);
        Guard.AssertIsNotNull(akkaContext);

        if (akkaContext.AkkaCore.ActorBaseType is null)
            return false;

        var currentBaseType = typeSymbol;
        while (currentBaseType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentBaseType, akkaContext.AkkaCore.ActorBaseType)) return true;
            currentBaseType = currentBaseType.BaseType;
        }

        return false;
    }
    
    /// <summary>
    /// Check if a syntax node is within a lambda expression that is an argument for either 
    /// `ReceiveAsync` or `ReceiveAnyAsync` method invocation 
    /// </summary>
    /// <param name="node">The syntax node being analyzed</param>
    /// <param name="semanticModel">The semantic model</param>
    /// <param name="akkaContext">The Akka context</param>
    /// <returns>true if the syntax node is inside a valid `ReceiveAsync` or `ReceiveAnyAsync` method</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInsideReceiveAsyncLambda(
        this SyntaxNode node,
        SemanticModel semanticModel,
        AkkaContext akkaContext)
    {
        // Traverse up the syntax tree to find the first lambda expression ancestor
        var lambdaExpression = node.FirstAncestorOrSelf<LambdaExpressionSyntax>();

        // Check if this lambda expression is an argument to an invocation expression
        if (lambdaExpression?.Parent is not ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocationExpression } }) 
            return false;

        return invocationExpression.IsReceiveAsyncInvocation(semanticModel, akkaContext);
    }

    /// <summary>
    /// Check if the invocation expression is a valid `ReceiveAsync` or `ReceiveAnyAsync` method invocation
    /// </summary>
    /// <param name="invocationExpression">The invocation expression being analyzed</param>
    /// <param name="semanticModel">The semantic model</param>
    /// <param name="akkaContext">The Akka context</param>
    /// <returns>true if the invocation expression is a valid `ReceiveAsync` or `ReceiveAnyAsync` method</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReceiveAsyncInvocation(this InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel, AkkaContext akkaContext)
    {
        // Get the method symbol from the invocation expression
        if (semanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
            return false;

        // Check if the method name is `ReceiveAsync` or `ReceiveAnyAsync` and it is defined inside the ReceiveActor class
        return methodSymbol.Name is "ReceiveAsync" or "ReceiveAnyAsync" 
               && SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, akkaContext.AkkaCore.ReceiveActorType);
    }
}