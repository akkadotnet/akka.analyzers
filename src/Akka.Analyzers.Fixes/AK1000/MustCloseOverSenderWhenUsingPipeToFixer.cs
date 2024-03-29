// -----------------------------------------------------------------------
//  <copyright file="MustCloseOverSenderWhenUsingPipeToFixer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
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
public sealed class MustCloseOverSenderWhenUsingPipeToFixer()
    : BatchedCodeFixProvider(RuleDescriptors.Ak1001CloseOverSenderUsingPipeTo.Id)
{
    public const string Key_FixPipeToSender = "AK1000_FixPipeToSender";

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic is null)
            return;
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the PipeTo invocation expression
        var invocationExpr = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>().First();
        if (invocationExpr is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use local variable for Sender",
                c => UseLocalVariableForSenderAsync(context.Document, invocationExpr, c),
                Key_FixPipeToSender),
            context.Diagnostics);
    }

    private static async Task<Document> UseLocalVariableForSenderAsync(Document document,
        InvocationExpressionSyntax invocationExpr, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        // Generate a local variable to hold 'Sender'
        var senderVariable = IntroduceLocalVariableStatement("sender", SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ThisExpression(),
                SyntaxFactory.IdentifierName("Sender")))
            .WithAdditionalAnnotations(Formatter.Annotation); // need this line to get indentation right

        // Identify the 'recipient' argument - assuming it's the first argument
        var arguments = invocationExpr.ArgumentList.Arguments;

        // Create a new argument to replace the 'recipient' argument
        var newRecipientArgument = SyntaxFactory.Argument(SyntaxFactory.IdentifierName("sender"));

        // Building a new list of arguments
        var newArguments = SyntaxFactory.SeparatedList(arguments.Select(arg =>
        {
            // Check if the argument is 'this.Sender'
            if (arg.Expression is IdentifierNameSyntax { Identifier.ValueText: "Sender" } or
                MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Sender" }) // need the right side of the "or" for `this.Sender"
            {
                return newRecipientArgument;
            }

            // For all other arguments, keep them as-is
            return arg;
        }));

        var newArgumentList = SyntaxFactory.ArgumentList(newArguments);
        var newInvocationExpr = invocationExpr.WithArgumentList(newArgumentList);

        // Check to see if we're inside a lambda expression and it does not have a body block
        var lambdaExpr = invocationExpr.FirstAncestorOrSelf<LambdaExpressionSyntax>();
        if (lambdaExpr?.Body is ExpressionSyntax exprSyntax)
        {
            // Make sure to replace the old invocation with the new one
            var newExprSyntax = exprSyntax.ReplaceNode(invocationExpr, newInvocationExpr);
            
            // Create a new block with the original lambda body as a statement
            var originalBody = SyntaxFactory.ExpressionStatement(newExprSyntax);
            
            // Insert the local variable declaration at the start of the lambda block
            var bodyBlock = SyntaxFactory.Block(
                senderVariable.WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed), 
                originalBody.WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));
            
            // Replace the original with the new one
            var newLambdaExpr = lambdaExpr.WithBody(null).WithBlock(bodyBlock);
            editor.ReplaceNode(lambdaExpr, newLambdaExpr);
        }
        else
        {
            // Find an appropriate insertion point for the local variable
            var insertionPoint = invocationExpr.FirstAncestorOrSelf<StatementSyntax>();

            if (insertionPoint == null)
                // Unable to find a valid insertion point
                return document;

            // Insert the local variable declaration at the found insertion point
            editor.InsertBefore(insertionPoint, senderVariable);
            
            // Make sure to replace the old invocation with the new one
            editor.ReplaceNode(invocationExpr, newInvocationExpr);
        }

        var newDocument = editor.GetChangedDocument(); // error happens here

        return newDocument;
    }
}