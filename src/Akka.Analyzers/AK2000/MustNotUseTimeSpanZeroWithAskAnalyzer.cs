// -----------------------------------------------------------------------
//  <copyright file="MustNotUseTimeSpanZeroWithAskAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers.AK2000;

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
            {
                foreach (var argument in invocationExpr.ArgumentList.Arguments)
                {
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
                }
            }
        }, SyntaxKind.InvocationExpression);
    }

    private static bool IsMethodAsk(IMethodSymbol methodSymbol, AkkaContext context)
    {
        if (!context.HasAkkaInstalled)
            return false;

        // Check if the method is Ask or Ask<T>
        return methodSymbol.Name == "Ask" && methodSymbol.ContainingType.ToString().Contains("Akka.Actor");
    }

    private static bool IsProblematicTimeSpan(ExpressionSyntax? expression, SyntaxNodeAnalysisContext context)
    {
        if (expression == null)
            return false;

        // Handle cases where the argument is a literal or default value
        var expressionStr = expression.ToString();
        if (expressionStr is "default(TimeSpan)" or "System.TimeSpan.Zero")
        {
            return true;
        }

        // Handle cases where the argument is a variable
        if (expression is IdentifierNameSyntax identifierName)
        {
            var symbol = context.SemanticModel.GetSymbolInfo(identifierName).Symbol;
            if (symbol is ILocalSymbol localSymbol)
            {
                return localSymbol.DeclaringSyntaxReferences
                    .Select(r => r.GetSyntax())
                    .OfType<VariableDeclaratorSyntax>()
                    .Any(v => IsProblematicTimeSpan(v.Initializer?.Value, context));
            }
        }

        return false;
    }
}