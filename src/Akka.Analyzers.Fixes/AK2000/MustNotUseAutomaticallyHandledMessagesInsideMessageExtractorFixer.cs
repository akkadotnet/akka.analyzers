// -----------------------------------------------------------------------
//  <copyright file="MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorFixer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Akka.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public class MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorFixer() 
    : BatchedCodeFixProvider(RuleDescriptors.Ak2001DoNotUseAutomaticallyHandledMessagesInShardMessageExtractor.Id)
{
    public const string Key_FixAutomaticallyHandledShardedMessage = "AK2001_FixAutoShardMessage";
    
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var reportedNodes = new HashSet<SyntaxNode>();

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the token at the location of the diagnostic.
            var token = root.FindToken(diagnosticSpan.Start);

            // Find the correct parent node to remove.
            var nodeToRemove = FindParentNodeToRemove(token.Parent);

            // Check if the node has already been processed to avoid duplicates
            if (nodeToRemove != null && reportedNodes.Add(nodeToRemove))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Remove unnecessary message handling",
                        createChangedDocument: c => RemoveOffendingNode(context.Document, nodeToRemove, c),
                        equivalenceKey: Key_FixAutomaticallyHandledShardedMessage),
                    diagnostic);
            }
        }
    }
    
    private static SyntaxNode? FindParentNodeToRemove(SyntaxNode? node)
    {
        while (node != null)
        {
            if (node is IfStatementSyntax || node is CaseSwitchLabelSyntax || node is SwitchExpressionArmSyntax)
            {
                return node;
            }
            node = node.Parent;
        }
        return null;
    }

    private static async Task<Document> RemoveOffendingNode(Document document, SyntaxNode nodeToRemove, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if(root == null)
            return document;
        
        var newRoot = root.RemoveNode(nodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);
        return newRoot == null ? document : document.WithSyntaxRoot(newRoot);
    }
}