// -----------------------------------------------------------------------
//  <copyright file="MustNotUseTimeSpanZeroWithAskAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotUseTimeSpanZeroWithAskAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak2000DoNotUseZeroTimeoutWithAsk)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;

            if (ctx.SemanticModel.GetSymbolInfo(invocationExpr).Symbol is IMethodSymbol { Name: "Ask" } methodSymbol &&
                methodSymbol.Parameters.Any(p => p.Type.ToString() == "System.TimeSpan?"))
                foreach (var argument in invocationExpr.ArgumentList.Arguments)
                    if (ctx.SemanticModel.GetTypeInfo(argument.Expression).Type is INamedTypeSymbol argType &&
                        argType.ConstructedFrom.ToString() == "System.TimeSpan")
                    {
                        if (argument.Expression is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Zero" })
                        {
                            var diagnostic = Diagnostic.Create(RuleDescriptors.Ak2000DoNotUseZeroTimeoutWithAsk,
                                argument.GetLocation());
                            ctx.ReportDiagnostic(diagnostic);
                        }
                        else if (argument.Expression is IdentifierNameSyntax identifierName)
                        {
                            var symbol = ctx.SemanticModel.GetSymbolInfo(identifierName).Symbol;
                            if (symbol is ILocalSymbol
                                {
                                    HasConstantValue: true, ConstantValue: not null
                                } localSymbol && localSymbol.ConstantValue.Equals(TimeSpan.Zero))
                            {
                                var diagnostic = Diagnostic.Create(RuleDescriptors.Ak2000DoNotUseZeroTimeoutWithAsk,
                                    argument.GetLocation());
                                ctx.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
        }, SyntaxKind.InvocationExpression);
    }
}