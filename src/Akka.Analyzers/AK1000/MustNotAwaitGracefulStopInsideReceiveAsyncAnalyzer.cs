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

            if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccess || memberAccess.Name.Identifier.Text != "GracefulStop") 
                return;
            
            // Check 1: GracefulStop() should not be awaited
            if (invocationExpr.Parent is not AwaitExpressionSyntax awaitExpression)
                return;

            // Check 2: Ensure called within ReceiveAsync<T> lambda expression
            if (!IsInsideReceiveAsyncLambda(invocationExpr, ctx.SemanticModel, akkaContext))
                return;

            var diagnostic = Diagnostic.Create(RuleDescriptors.Ak1002DoNotAwaitOnGracefulStop, awaitExpression.GetLocation());
            ctx.ReportDiagnostic(diagnostic);
        }, SyntaxKind.InvocationExpression);
    }
    
    private static bool IsInsideReceiveAsyncLambda(SyntaxNode node, SemanticModel semanticModel, AkkaContext akkaContext)
    {
        // Traverse up the syntax tree to find the first lambda expression ancestor
        var lambdaExpression = node.FirstAncestorOrSelf<LambdaExpressionSyntax>();

        // Check if this lambda expression is an argument to an invocation expression
        if (lambdaExpression?.Parent is not ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocationExpression } }) 
            return false;
        
        // Get the method symbol from the invocation expression
        if (semanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
            return false;

        // Check if the method name is 'ReceiveAsync' and it is defined inside the ReceiveActor class
        return methodSymbol.Name == "ReceiveAsync" && SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, akkaContext.AkkaCore.ReceiveActorType);
    }
}