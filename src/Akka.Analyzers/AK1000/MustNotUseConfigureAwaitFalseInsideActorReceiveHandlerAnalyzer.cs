// -----------------------------------------------------------------------
//  <copyright file="MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1009MustNotUseConfigureAwaitFalseInsideActorReceiveHandler)
{
    private const string ConfigureAwaitMethodName = "ConfigureAwait";
    private const string SystemThreadingTasksNamespace = "System.Threading.Tasks";

    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;

            // Method call must be `<expr>.ConfigureAwait(<arg>)`
            if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;

            if (memberAccess.Name.Identifier.Text != ConfigureAwaitMethodName)
                return;

            // Argument must be a literal `false`
            if (invocationExpr.ArgumentList.Arguments.Count != 1)
                return;

            if (invocationExpr.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax literal
                || !literal.IsKind(SyntaxKind.FalseLiteralExpression))
                return;

            // Resolve the symbol and confirm it's Task/Task<T>/ValueTask/ValueTask<T>.ConfigureAwait
            if (ctx.SemanticModel.GetSymbolInfo(invocationExpr.Expression).Symbol is not IMethodSymbol methodSymbol)
                return;

            if (!IsTaskConfigureAwait(methodSymbol))
                return;

            // The call must be inside a ReceiveAsync/ReceiveAnyAsync/CommandAsync/CommandAnyAsync handler lambda
            if (!invocationExpr.IsInsideAsyncActorHandlerLambda(ctx.SemanticModel, akkaContext))
                return;

            var diagnostic = Diagnostic.Create(
                RuleDescriptors.Ak1009MustNotUseConfigureAwaitFalseInsideActorReceiveHandler,
                invocationExpr.GetLocation());
            ctx.ReportDiagnostic(diagnostic);
        }, SyntaxKind.InvocationExpression);
    }

    private static bool IsTaskConfigureAwait(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.Name != ConfigureAwaitMethodName)
            return false;

        var containingType = methodSymbol.ContainingType;
        if (containingType is null)
            return false;

        if (containingType.ContainingNamespace?.ToDisplayString() != SystemThreadingTasksNamespace)
            return false;

        var typeName = containingType.Name;
        return typeName is "Task" or "ValueTask";
    }
}
