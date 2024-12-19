// -----------------------------------------------------------------------
//  <copyright file="CodeAnalysisExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Akka.Analyzers.Context.Core;
using Akka.Analyzers.Context.Core.Actor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;

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

    public static bool IsActorBaseSubclass(this INamedTypeSymbol typeSymbol, IAkkaCoreContext akkaContext)
    {
        Guard.AssertIsNotNull(typeSymbol);
        Guard.AssertIsNotNull(akkaContext);

        if (akkaContext.Actor.ActorBaseType is null)
            return false;

        var currentBaseType = typeSymbol;
        while (currentBaseType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentBaseType, akkaContext.Actor.ActorBaseType)) return true;
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
        IAkkaCoreContext akkaContext)
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
    public static bool IsReceiveAsyncInvocation(
        this InvocationExpressionSyntax invocationExpression,
        SemanticModel semanticModel,
        IAkkaCoreContext akkaContext)
    {
        // Get the method symbol from the invocation expression
        if (semanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
            return false;
        return methodSymbol.IsReceiveAsyncInvocation(akkaContext);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReceiveAsyncInvocation(
        this IMethodSymbol methodSymbol,
        IAkkaCoreContext akkaContext)
    {
        // Go up the chain to make sure that we have the base generic method symbol declaration it originated from 
        var from = methodSymbol.ConstructedFrom;
        while (!ReferenceEquals(from, from.ConstructedFrom))
            from = from.ConstructedFrom;
        
        // Check if the method name is `ReceiveAsync` or `ReceiveAnyAsync` and it is defined inside the ReceiveActor class
        var refSymbols = akkaContext.Actor.ReceiveActor.ReceiveAsync.AddRange(akkaContext.Actor.ReceiveActor.ReceiveAnyAsync);
        return refSymbols.Any(s => ReferenceEquals(from, s));
    }
    
    public static bool IsAccessingActorSelf(
        this InvocationExpressionSyntax invocationExpression,
        SemanticModel semanticModel,
        IAkkaCoreContext akkaContext)
    {
        // Expression need to be a member access
        if (invocationExpression.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;
        
        return IsAccessingActorBaseSelf(memberAccess, semanticModel, akkaContext) ||
               IsAccessingActorContextSelf(memberAccess, semanticModel, akkaContext);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActorSenderIdentifier(
        this IdentifierNameSyntax identifier,
        SemanticModel semanticModel,
        IAkkaCoreContext akkaContext)
    {
        // Make sure that identifier is a property
        if (semanticModel.GetSymbolInfo(identifier).Symbol is not IPropertySymbol propertySymbol)
            return false;
        
        // Property is equal to `ActorBase.Sender` or `IActorContext.Sender`
        return ReferenceEquals(akkaContext.Actor.ActorBase.Sender, propertySymbol) ||
               ReferenceEquals(akkaContext.Actor.IActorContext.Sender, propertySymbol);
    }
    
    private static bool IsAccessingActorBaseSelf(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        IAkkaCoreContext akkaContext)
    {
        // Method accesses an identifier
        if (memberAccess.Expression is not IdentifierNameSyntax identifier)
            return false;
        
        // Make sure that identifier is a property
        if (semanticModel.GetSymbolInfo(identifier).Symbol is not IPropertySymbol propertySymbol)
            return false;

        // Property is equal to `ActorBase.Self`
        return ReferenceEquals(propertySymbol, akkaContext.Actor.ActorBase.Self!);
    }

    private static bool IsAccessingActorContextSelf(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        IAkkaCoreContext akkaContext)
    {
        // The object being accessed by the invocation needs to be a member access itself
        if (memberAccess.Expression is not MemberAccessExpressionSyntax selfMemberAccess)
            return false;

        if (semanticModel.GetSymbolInfo(selfMemberAccess).Symbol is not IPropertySymbol propertySymbol)
            return false;
        
        return ReferenceEquals(propertySymbol, akkaContext.Actor.IActorContext.Self!);
    }

    public static bool IsDerivedOrImplements(this ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, ITypeSymbol baseSymbol)
    {
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        return classSymbol is not null && ((ITypeSymbol)classSymbol).IsDerivedOrImplements(baseSymbol);
    }
    
    public static bool IsDerivedOrImplements(this ITypeSymbol typeSymbol, ITypeSymbol baseSymbol)
    {
        if (SymbolEqualityComparer.Default.Equals(typeSymbol, baseSymbol))
            return true;
        
        // Check interfaces directly implemented by the type
        foreach (var interfaceType in typeSymbol.AllInterfaces)
        {
            if (IsDerivedOrImplements(interfaceType, baseSymbol))
                return true;
        }

        // Recursively check base types
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if(IsDerivedOrImplements(baseType, baseSymbol))
                return true;
            baseType = baseType.BaseType;
        }

        return false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool OverridesAny(this IMethodSymbol methodSymbol, IReadOnlyCollection<IMethodSymbol> refMethods)
    {
        if (!methodSymbol.IsOverride)
            return false;
        
        while (methodSymbol.OverriddenMethod != null)
            methodSymbol = methodSymbol.OverriddenMethod;

        return methodSymbol.MatchesAny(refMethods);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool MatchesAny(this IMethodSymbol methodSymbol, IReadOnlyCollection<IMethodSymbol> refMethods)
    {
        while (!ReferenceEquals(methodSymbol, methodSymbol.ConstructedFrom))
            methodSymbol = methodSymbol.ConstructedFrom;

        return refMethods.Any(m => ReferenceEquals(m, methodSymbol));
    }

    public static List<IOperation> Descendants(this BasicBlock block)
    {
        var descendants = new List<IOperation>();
        foreach (var operation in block.Operations)
        {
            RecurseOperation(operation, descendants);
        }
        return descendants;

        static void RecurseOperation(IOperation operation, List<IOperation> descendants)
        {
            descendants.Add(operation);
            foreach (var childOperation in operation.ChildOperations)
            {
                RecurseOperation(childOperation, descendants);
            }
        }
    }
    
}