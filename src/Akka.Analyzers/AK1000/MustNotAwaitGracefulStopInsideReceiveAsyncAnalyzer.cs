// -----------------------------------------------------------------------
//  <copyright file="MustNotAwaitGracefulShutdownInsideReceive.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;
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

            if(!IsGracefulStopInvocation(invocationExpr, ctx.SemanticModel, akkaContext))
                return;
            
            // Check 1: GracefulStop() should not be awaited
            if (invocationExpr.Parent is not AwaitExpressionSyntax awaitExpression)
                return;

            // Check 2: Ensure called within ReceiveAsync<T> lambda expression
            if (!invocationExpr.IsInsideReceiveAsyncLambda(ctx.SemanticModel, akkaContext))
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
        AkkaContext akkaContext)
    {
        // Get the method symbol from the invocation expression
        if (semanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
            return false;

        // Check if the method name is 'GracefulStop', that it is an extension method,
        // and it is defined inside the GracefulStopSupport static class
        return methodSymbol is { Name: "GracefulStop", IsExtensionMethod: true }
               && SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, akkaContext.AkkaCore.GracefulStopSupportType);
    }
}