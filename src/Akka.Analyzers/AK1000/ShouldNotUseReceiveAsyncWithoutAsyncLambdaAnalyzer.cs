// -----------------------------------------------------------------------
//  <copyright file="ShouldNotUseReceiveAsyncWithoutAwait.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShouldNotUseReceiveAsyncWithoutAsyncLambdaAnalyzer(): AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1003ShouldNotUseReceiveAsyncSynchronously)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;
            
            // check that the invocation is a valid ReceiveAsync or ReceiveAnyAsync method
            if(!invocationExpr.IsReceiveAsyncInvocation(semanticModel, akkaContext))
                return;
            
            // Get the method argument that matches a lambda expression of Func<T, Task>
            var lambdaArg = invocationExpr.ArgumentList.Arguments
                .FirstOrDefault(arg => 
                    arg.Expression is LambdaExpressionSyntax expr 
                    && IsFuncOfTTask(semanticModel.GetTypeInfo(expr).ConvertedType));

            if (lambdaArg is null)
                return;

            var lambdaExpr = (LambdaExpressionSyntax)lambdaArg.Expression;
            
            // first case, lambda expression is not prefixed with the `async` keyword.
            // We will assume that the user is doing a proper thing and not just returning a `Task.CompletedTask`
            if (!lambdaExpr.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
                return;
            
            // second case, lambda expression contains a block and one of its child is awaited, OK
            if(lambdaExpr.Body.DescendantNodes().OfType<AwaitExpressionSyntax>().Any())
                return;
            
            // third case, lambda expression is a one-liner await expression, OK
            if (lambdaExpr.Body is AwaitExpressionSyntax) 
                return;
            
            // fourth case, there are no await expression inside the lambda expression body
            ReportDiagnostic();
            return;
            
            void ReportDiagnostic()
            {
                var diagnostic = Diagnostic.Create(
                    descriptor: RuleDescriptors.Ak1003ShouldNotUseReceiveAsyncSynchronously, 
                    location: invocationExpr.GetLocation(), 
                    "lambda expression");
                ctx.ReportDiagnostic(diagnostic);
            }
        }, SyntaxKind.InvocationExpression);

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFuncOfTTask(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol { DelegateInvokeMethod: not null } namedTypeSymbol) 
            return false;
        
        var delegateReturnType = namedTypeSymbol.DelegateInvokeMethod.ReturnType;
        var delegateParameters = namedTypeSymbol.DelegateInvokeMethod.Parameters;

        return delegateReturnType is INamedTypeSymbol { Name: nameof(Task) }
               && delegateParameters.Length == 1;
    }    
}