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
public class MustNotUseTimeSpanZeroWithAskAnalyzer() : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak2000DoNotUseZeroTimeoutWithAsk)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocation = (InvocationExpressionSyntax)ctx.Node;

            if (ModelExtensions.GetSymbolInfo(ctx.SemanticModel, invocation).Symbol is not IMethodSymbol methodSymbol)
                return;

            // Check if it's a call to Ask
            if (!IsMethodAsk(methodSymbol, akkaContext))
                return;
            
            // Iterate over the arguments - check only the timespan ones
            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                if (IsTimeSpanType(ctx.SemanticModel.GetTypeInfo(arg.Expression).ConvertedType))
                {
                    // Check if the TimeSpan argument is default or TimeSpan.Zero
                    // Check if the argument is a problematic TimeSpan
                    if (IsProblematicTimeSpan(arg.Expression, ctx))
                    {
                        var diagnostic = Diagnostic.Create(RuleDescriptors.Ak2000DoNotUseZeroTimeoutWithAsk, arg.GetLocation());
                        ctx.ReportDiagnostic(diagnostic);
                        break; // Report only once per invocation
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
    
    private static bool IsTimeSpanType(ITypeSymbol? typeSymbol)
    {
        return typeSymbol != null && typeSymbol.ToString() == "System.TimeSpan";
    }
    
    private static bool IsProblematicTimeSpan(ExpressionSyntax? expression, SyntaxNodeAnalysisContext context)
    {
        if(expression == null)
            return false;
        
        // Handle cases where the argument is a literal or default value
        var expressionStr = expression.ToString();
        if (expressionStr is "default" or "System.TimeSpan.Zero")
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