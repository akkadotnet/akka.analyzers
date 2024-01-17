// -----------------------------------------------------------------------
//  <copyright file="MustNotAwaitGracefulShutdownInsideReceive.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotAwaitGracefulStopInsideReceiveAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1002DoNotAwaitOnGracefulStop)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var awaitExpression = (AwaitExpressionSyntax)ctx.Node;
            
            var containingType = ctx.SemanticModel.GetEnclosingSymbol(awaitExpression.SpanStart)?.ContainingType;
            if (containingType == null || !containingType.IsActorBaseSubclass(akkaContext)) 
                return;

            if (awaitExpression.Expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "GracefulStop" } } invocationExpr &&
                IsInsideReceiveAsync(ctx, invocationExpr))
            {
                var diagnostic = Diagnostic.Create(RuleDescriptors.Ak1002DoNotAwaitOnGracefulStop, awaitExpression.GetLocation());
                ctx.ReportDiagnostic(diagnostic);
            }
            
        }, SyntaxKind.AwaitExpression);
    }
    
    private static bool IsInsideReceiveAsync(SyntaxNodeAnalysisContext context, SyntaxNode node)
    {
        var ancestor = node.FirstAncestorOrSelf<LambdaExpressionSyntax>();
        if (ancestor is not { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } }) 
            return false;

        return context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: "ReceiveAsync" };
    }
}