// -----------------------------------------------------------------------
//  <copyright file="MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Akka.Analyzers.Context.System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1009MustNotUseConfigureAwaitFalseInsideActorReceiveHandler)
{
    private const string ConfigureAwaitMethodName = "ConfigureAwait";

    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;

            if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;

            if (memberAccess.Name.Identifier.Text != ConfigureAwaitMethodName)
                return;

            if (invocationExpr.ArgumentList.Arguments.Count != 1)
                return;

            if (invocationExpr.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax literal
                || !literal.IsKind(SyntaxKind.FalseLiteralExpression))
                return;

            // Cheap syntactic prune before any semantic lookups: bail out if not inside a lambda
            // that's an argument to some invocation. Eliminates the overwhelming common case
            // (top-level methods, synchronous helpers) without resolving any symbols.
            if (!CodeAnalysisExtensions.TryGetEnclosingLambdaInvocation(invocationExpr, out _))
                return;

            if (ctx.SemanticModel.GetSymbolInfo(invocationExpr.Expression).Symbol is not IMethodSymbol methodSymbol)
                return;

            if (!IsTaskConfigureAwait(methodSymbol, akkaContext.SystemThreadingTasks))
                return;

            if (!invocationExpr.IsInsideAsyncActorHandlerLambda(ctx.SemanticModel, akkaContext))
                return;

            var location = Location.Create(
                invocationExpr.SyntaxTree,
                TextSpan.FromBounds(memberAccess.Name.SpanStart, invocationExpr.Span.End));
            ctx.ReportDiagnostic(Diagnostic.Create(
                RuleDescriptors.Ak1009MustNotUseConfigureAwaitFalseInsideActorReceiveHandler,
                location));
        }, SyntaxKind.InvocationExpression);
    }

    private static bool IsTaskConfigureAwait(IMethodSymbol methodSymbol, ISystemThreadingTasksContext taskContext)
    {
        if (methodSymbol.Name != ConfigureAwaitMethodName)
            return false;

        var containingDefinition = methodSymbol.ContainingType.OriginalDefinition;
        return ReferenceEquals(containingDefinition, taskContext.TaskType)
               || ReferenceEquals(containingDefinition, taskContext.TaskOfTType)
               || ReferenceEquals(containingDefinition, taskContext.ValueTaskType)
               || ReferenceEquals(containingDefinition, taskContext.ValueTaskOfTType);
    }
}
