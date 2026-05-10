// -----------------------------------------------------------------------
//  <copyright file="MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerFixer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
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
public class MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerFixer()
    : BatchedCodeFixProvider(RuleDescriptors.Ak1009MustNotUseConfigureAwaitFalseInsideActorReceiveHandler.Id)
{
    public const string Key_RemoveConfigureAwaitFalse = "AK1009_RemoveConfigureAwaitFalse";

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic is null)
            return;
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        if (root.FindNode(diagnosticSpan) is not InvocationExpressionSyntax invocationExpr)
            return;

        if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Remove ConfigureAwait(false)",
                c => RemoveConfigureAwaitFalseAsync(context.Document, invocationExpr, memberAccess, c),
                Key_RemoveConfigureAwaitFalse),
            context.Diagnostics);
    }

    private static async Task<Document> RemoveConfigureAwaitFalseAsync(
        Document document,
        InvocationExpressionSyntax invocationExpr,
        MemberAccessExpressionSyntax memberAccess,
        CancellationToken cancellationToken)
    {
        var replacement = memberAccess.Expression
            .WithLeadingTrivia(invocationExpr.GetLeadingTrivia())
            .WithTrailingTrivia(invocationExpr.GetTrailingTrivia());

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        editor.ReplaceNode(invocationExpr, replacement);

        return editor.GetChangedDocument();
    }
}
