// -----------------------------------------------------------------------
//  <copyright file="ShouldNotUseSystemToCreateChildActorsFixer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Akka.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public class ShouldNotUseSystemToCreateChildActorsFixer()
    : BatchedCodeFixProvider(RuleDescriptors.Ak1008ShouldNotUseSystemToCreateChildActor.Id)
{
    public const string Key_FixActorSystemActorOf = "AK1008_FixActorSystemActorOf";
    
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic is null)
            return;
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the invocation expression
        if(root.FindNode(diagnosticSpan) is not InvocationExpressionSyntax invocationExpr)
            return;
        
        context.RegisterCodeFix(
            CodeAction.Create(
                "Remove await keyword",
                c => ReplaceSystemWithContextAsync(context.Document, invocationExpr, c),
                Key_FixActorSystemActorOf),
            context.Diagnostics);
    }

    private static async Task<Document> ReplaceSystemWithContextAsync(
        Document document,
        InvocationExpressionSyntax invocationExpr,
        CancellationToken cancellationToken)
    {
        // We expect the offending call to look like: Context.System.ActorOf(...)
        // That means invocationNode.Expression is a MemberAccessExpressionSyntax.
        if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccess)
            return document;

        // Create a new receiver identifier: "Context"
        var newReceiver = SyntaxFactory.IdentifierName("Context")
            .WithLeadingTrivia(memberAccess.Expression.GetLeadingTrivia())
            .WithTrailingTrivia(memberAccess.Expression.GetTrailingTrivia());
            
        // Build a new member access expression using the new receiver and preserving the operator and the invoked member name.
        var newMemberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                newReceiver,
                memberAccess.OperatorToken,
                memberAccess.Name)
            .WithLeadingTrivia(memberAccess.GetLeadingTrivia())
            .WithTrailingTrivia(memberAccess.GetTrailingTrivia());
            
        // Replace the original invocation's expression with the new member access.
        var newInvocation = invocationExpr.WithExpression(newMemberAccess);
            
        // Use DocumentEditor to perform the replacement.
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        editor.ReplaceNode(invocationExpr, newInvocation);
            
        return editor.GetChangedDocument();
    }
}