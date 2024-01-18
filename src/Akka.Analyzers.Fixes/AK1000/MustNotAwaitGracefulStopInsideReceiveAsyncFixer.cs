// -----------------------------------------------------------------------
//  <copyright file="MustNotAwaitGracefulStopInsideReceiveAsyncFixer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using static Akka.Analyzers.Fixes.CodeGeneratorUtilities;

namespace Akka.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public class MustNotAwaitGracefulStopInsideReceiveAsyncFixer()
    : BatchedCodeFixProvider(RuleDescriptors.Ak1002DoNotAwaitOnGracefulStop.Id)
{
    public const string Key_FixAwaitGracefulStop = "AK1002_FixAwaitGracefulStop";
    
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic is null)
            return;
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the await invocation expression
        if (root.FindToken(diagnosticSpan.Start).Parent is not AwaitExpressionSyntax awaitExpression)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Remove await keyword",
                c => RemoveAwaitKeywordFromGracefulStopAsync(context.Document, awaitExpression, c),
                Key_FixAwaitGracefulStop),
            context.Diagnostics);
    }
    
    private static async Task<Document> RemoveAwaitKeywordFromGracefulStopAsync(Document document,
        AwaitExpressionSyntax awaitExpr, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        // Create an assignment expression
        var assignment = SyntaxFactory.EqualsValueClause(awaitExpr.Expression);
        
        // Create a variable declaration "_"
        var variableDeclarator = SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("_"))
            .WithInitializer(assignment);
        var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("var"))
            .AddVariables(variableDeclarator);
        var localDeclaration = SyntaxFactory.LocalDeclarationStatement(variableDeclaration)
            .WithAdditionalAnnotations(Formatter.Annotation); // need this line to get indentation right
        
        var lambdaExpr = awaitExpr.FirstAncestorOrSelf<LambdaExpressionSyntax>();
        if (lambdaExpr?.Body is ExpressionSyntax)
        {
            // Insert the local variable declaration at the start of the lambda block
            var bodyBlock = SyntaxFactory.Block(localDeclaration);
            
            // Replace the original with the new one
            var newLambdaExpr = lambdaExpr.WithBody(null).WithBlock(bodyBlock);
            editor.ReplaceNode(lambdaExpr, newLambdaExpr);
        }
        else
        {
            // Replace the await expression with the new local declaration
            if(awaitExpr.Parent != null)
                editor.ReplaceNode(awaitExpr.Parent, localDeclaration);
        }
        
        var newDocument = editor.GetChangedDocument();

        return newDocument;
    }

}