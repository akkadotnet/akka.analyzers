// -----------------------------------------------------------------------
//  <copyright file="MustCloseOverSenderWhenUsingPipeToAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustCloseOverSenderWhenUsingPipeToAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1001CloseOverSenderUsingPipeTo)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;

            // Check if it's a PipeTo method call
            if (invocationExpr.Expression is MemberAccessExpressionSyntax
                {
                    Name.Identifier.ValueText: "PipeTo"
                } memberAccessExpr)
            {
                // Check if the containing type is an Akka.NET actor
                var containingType = ctx.SemanticModel.GetEnclosingSymbol(invocationExpr.SpanStart)?.ContainingType;
                if (containingType != null && containingType.IsActorBaseSubclass(akkaContext))
                    // Check if 'this.Sender' is used in the arguments
                    foreach (var arg in invocationExpr.ArgumentList.Arguments)
                    {
                        var symbol = ctx.SemanticModel.GetSymbolInfo(arg.Expression).Symbol;
                        if (IsThisSenderSymbol(symbol, akkaContext))
                        {
                            var diagnostic = Diagnostic.Create(RuleDescriptors.Ak1001CloseOverSenderUsingPipeTo,
                                memberAccessExpr.Name.GetLocation());
                            ctx.ReportDiagnostic(diagnostic);
                            break; // Report only once per invocation
                        }
                    }
            }
        }, SyntaxKind.InvocationExpression);
    }

    private static bool IsThisSenderSymbol(ISymbol? symbol, AkkaContext akkaContext)
    {
        // Check if the symbol is 'this.Sender'
        return symbol is { Name: "Sender", ContainingType.BaseType: not null } &&
               symbol.ContainingType.IsActorBaseSubclass(akkaContext);
    }
}