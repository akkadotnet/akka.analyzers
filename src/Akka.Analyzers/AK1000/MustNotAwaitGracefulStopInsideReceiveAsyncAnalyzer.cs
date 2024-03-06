// -----------------------------------------------------------------------
//  <copyright file="MustNotAwaitGracefulShutdownInsideReceive.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;
using Akka.Analyzers.Context.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotAwaitGracefulStopInsideReceiveAsyncAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1002DoNotAwaitOnGracefulStop)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;
            var akkaCore = akkaContext.AkkaCore;
            
            if(!IsGracefulStopInvocation(invocationExpr, semanticModel, akkaCore))
                return;
            
            // Check 1: GracefulStop() should not be awaited
            if (invocationExpr.Parent is not AwaitExpressionSyntax awaitExpression)
                return;

            // Check 2: Ensure called within ReceiveAsync<T> or ReceiveAnyAsync lambda expression
            if (!invocationExpr.IsInsideReceiveAsyncLambda(semanticModel, akkaCore))
                return;

            // Check 3: Ensure method is accessing ActorBase.Self or ActorContext.Self
            if(!invocationExpr.IsAccessingActorSelf(semanticModel, akkaCore))
               return;
            
            var diagnostic = Diagnostic.Create(RuleDescriptors.Ak1002DoNotAwaitOnGracefulStop, awaitExpression.GetLocation());
            ctx.ReportDiagnostic(diagnostic);
        }, SyntaxKind.InvocationExpression);
    }
    

    /// <summary>
    /// Check if the invocation expression is a valid `GracefulStop` method invocation
    /// </summary>
    /// <param name="invocationExpression">The invocation expression being analyzed</param>
    /// <param name="semanticModel">The semantic model</param>
    /// <param name="akkaContext">The Akka context</param>
    /// <returns>true if the invocation expression is a valid `GracefulStop` method</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsGracefulStopInvocation(
        InvocationExpressionSyntax invocationExpression,
        SemanticModel semanticModel,
        IAkkaCoreContext akkaContext)
    {
        // Expression need to be a member access
        if (invocationExpression.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;
        
        // Expression name should be "GracefulStop"
        if (memberAccess.Name.Identifier.Text is not "GracefulStop")
            return false;
        
        // Expression need to be a method invocation
        if (semanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
            return false;

        // Check if the method is an extension method
        if(methodSymbol is not { IsExtensionMethod: true })
            return false;

        // Check that method is defined inside the GracefulStopSupport static class
        if (!SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, akkaContext.Actor.GracefulStopSupportType))
            return false;

        return true;
    }
}