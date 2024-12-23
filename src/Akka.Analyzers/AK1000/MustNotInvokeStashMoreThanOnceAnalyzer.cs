// -----------------------------------------------------------------------
//  <copyright file="MustNotInvokeStashMoreThanOnce.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotInvokeStashMoreThanOnceAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1008MustNotInvokeStashMoreThanOnce)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        // For lambdas, namely Receive<T> and ReceiveAny
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var node = ctx.Node;
            var semanticModel = ctx.SemanticModel;
            var akkaCore = akkaContext.AkkaCore;
            var stashMethod = akkaCore.Actor.IStash.Stash!;

            // First: need to check if this method / lambda is declared in an ActorBase subclass
            var symbol = semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol is null || !symbol.ContainingType.IsActorBaseSubclass(akkaContext.AkkaCore))
                return;

            // Find all stash calls within the method or lambda
            var stashCalls = node.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(c => IsStashInvocation(semanticModel, c, stashMethod))
                .ToList();

            if (stashCalls.Count < 2)
                return;

            // Flag all stash calls if they are in the same block without branching
            var groupedByBlock = stashCalls
                .GroupBy(GetContainingBlock)
                .Where(group => group.Key != null && group.Count() > 1)
                .SelectMany(group => group)
                .ToList();

            // Report all calls in the same block
            foreach (var call in groupedByBlock)
            {
                ReportDiagnostic(call, ctx);
            }
        }, SyntaxKind.MethodDeclaration, SyntaxKind.InvocationExpression);
        return;

        static void ReportDiagnostic(InvocationExpressionSyntax call, SyntaxNodeAnalysisContext ctx)
        {
            var diagnostic = Diagnostic.Create(
                descriptor: RuleDescriptors.Ak1008MustNotInvokeStashMoreThanOnce,
                location: call.GetLocation());
            ctx.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsStashInvocation(SemanticModel model, InvocationExpressionSyntax invocation,
        IMethodSymbol stashMethod)
    {
        var symbol = model.GetSymbolInfo(invocation).Symbol;
        return SymbolEqualityComparer.Default.Equals(symbol, stashMethod);
    }

    private static SyntaxNode? GetContainingBlock(SyntaxNode node)
    {
        return node.AncestorsAndSelf()
            .FirstOrDefault(n => n is BlockSyntax || n is SwitchSectionSyntax || n is MethodDeclarationSyntax);
    }
}
