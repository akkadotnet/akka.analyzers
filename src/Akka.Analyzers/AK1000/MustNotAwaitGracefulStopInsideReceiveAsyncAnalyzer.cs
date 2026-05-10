// -----------------------------------------------------------------------
//  <copyright file="MustNotAwaitGracefulShutdownInsideReceive.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
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

        var handlerCache = ActorHandlerMethodCache.Create(context.Compilation, akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;
            var akkaCore = akkaContext.AkkaCore;

            if (semanticModel.GetSymbolInfo(invocationExpr.Expression).Symbol is not IMethodSymbol methodSymbol)
                return;

            // Unfold reduced extension method to its original method
            methodSymbol = methodSymbol.ReducedFrom ?? methodSymbol;

            // Method must be one of the GracefulStop() extension methods
            var refSymbols = akkaCore.Actor.GracefulStopSupportSupport.GracefulStop;
            if (!refSymbols.Any(s => ReferenceEquals(methodSymbol, s)))
                return;

            // Check 1: GracefulStop() should not be awaited
            if (invocationExpr.Parent is not AwaitExpressionSyntax awaitExpression)
                return;

            // Check 2: Ensure method is accessing ActorBase.Self or ActorContext.Self
            if (!invocationExpr.IsAccessingActorSelf(semanticModel, akkaCore))
                return;

            // Check 3: Either the await sits inside a Receive*Async/Command*Async lambda...
            if (invocationExpr.IsInsideReceiveAsyncLambda(semanticModel, akkaCore))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    RuleDescriptors.Ak1002DoNotAwaitOnGracefulStop, awaitExpression.GetLocation()));
                return;
            }

            // ...or the await sits inside a method body whose method symbol is bound as a
            // method-group handler somewhere in the compilation (ReceiveAsync(SomeMethod), etc.).
            if (IsInsideHandlerMethodBody(invocationExpr, semanticModel, handlerCache))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    RuleDescriptors.Ak1002DoNotAwaitOnGracefulStop, awaitExpression.GetLocation()));
            }
        }, SyntaxKind.InvocationExpression);
    }

    private static bool IsInsideHandlerMethodBody(
        SyntaxNode node,
        SemanticModel semanticModel,
        ActorHandlerMethodCache handlerCache)
    {
        var enclosingMethodSyntax = FindEnclosingMethodLikeNode(node);
        if (enclosingMethodSyntax is null)
            return false;

        if (semanticModel.GetDeclaredSymbol(enclosingMethodSyntax) is not IMethodSymbol enclosingMethod)
            return false;

        return handlerCache.Contains(enclosingMethod);
    }

    /// <summary>
    /// Walks up to the enclosing named method declaration. Bails when an
    /// <see cref="AnonymousFunctionExpressionSyntax"/> (lambda or anonymous method) is encountered
    /// first — that code is generally dispatched separately (e.g. <c>Task.Run(...)</c>) and
    /// shouldn't inherit the handler-method classification of the surrounding named method.
    /// Walks past <see cref="LocalFunctionStatementSyntax"/> because local functions execute
    /// inline in their containing method's actor-scheduler context.
    /// </summary>
    private static SyntaxNode? FindEnclosingMethodLikeNode(SyntaxNode node)
    {
        for (var current = node.Parent; current != null; current = current.Parent)
        {
            if (current is AnonymousFunctionExpressionSyntax)
                return null;
            if (current is MethodDeclarationSyntax)
                return current;
        }
        return null;
    }
}