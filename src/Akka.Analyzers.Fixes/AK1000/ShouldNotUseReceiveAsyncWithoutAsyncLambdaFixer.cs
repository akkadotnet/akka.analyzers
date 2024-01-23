// -----------------------------------------------------------------------
//  <copyright file="ShouldNotUseReceiveAsyncWithoutAsyncLambdaFixer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Composition;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Akka.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public class ShouldNotUseReceiveAsyncWithoutAsyncLambdaFixer()
    : BatchedCodeFixProvider(RuleDescriptors.Ak1003ShouldNotUseReceiveAsyncSynchronously.Id)
{
    public const string Key_FixReceiveAsyncWithoutAsync = "AK1003_FixReceiveAsyncWithoutAsync";
    
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic is null)
            return;
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the ReceiveAsync<T>() invocation identified by the diagnostic
        var invocationExpr = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();
        if (invocationExpr is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace ReceiveAsync or ReceiveAnyAsync with Receive or ReceiveAny",
                createChangedDocument: c => ReplaceReceiveAsyncWithReceiveAsync(context.Document, invocationExpr, c),
                equivalenceKey: Key_FixReceiveAsyncWithoutAsync),
            diagnostic);
    }

    private static async Task<Document> ReplaceReceiveAsyncWithReceiveAsync(Document document,
        InvocationExpressionSyntax invocationExpr, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return document;
        
        // Get the method argument that matches a lambda expression of Func<T, Task>
        var lambdaArg = invocationExpr.ArgumentList.Arguments
            .FirstOrDefault(arg => 
                arg.Expression is LambdaExpressionSyntax expr 
                && IsFuncOfTTask(semanticModel.GetTypeInfo(expr).ConvertedType));
        if(lambdaArg is null)
            return document;
            
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        
        // Remove "Async" from the method name
        var newMethodName = invocationExpr.Expression.ToString().Replace("Async", "");
        var newInvocationExpr = invocationExpr
            .WithExpression(SyntaxFactory.ParseExpression(newMethodName))
            .WithTriviaFrom(invocationExpr);

        var lambdaExpr = (LambdaExpressionSyntax)lambdaArg.Expression;
        
        // Remove 'async' keyword
        var newLambdaExpr = lambdaExpr.WithAsyncKeyword(default);
        
        // Remove 'return Task.CompletedTask;'
        if (lambdaExpr.Body is BlockSyntax blockSyntax)
        {
            var newBlockStatements = blockSyntax.Statements
                .Where(stmt => 
                    stmt is not ReturnStatementSyntax returnStmt 
                    || returnStmt.Expression?.ToString() is not "Task.CompletedTask");
            var newBlock = SyntaxFactory.Block(newBlockStatements).WithTriviaFrom(blockSyntax);
            newLambdaExpr = newLambdaExpr.WithBlock(newBlock);
        }

        // replace old lambda argument expression content with the new one
        var newLambdaArg = lambdaArg.WithExpression(newLambdaExpr);
        var newArgumentList = SyntaxFactory.ArgumentList(invocationExpr.ArgumentList.Arguments.Replace(lambdaArg, newLambdaArg));
        
        // replace original method invocation lambda argument with the modified one 
        newInvocationExpr = newInvocationExpr.WithArgumentList(newArgumentList);
        
        // replace original method invocation with the modified one
        editor.ReplaceNode(invocationExpr, newInvocationExpr);

        return editor.GetChangedDocument();
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