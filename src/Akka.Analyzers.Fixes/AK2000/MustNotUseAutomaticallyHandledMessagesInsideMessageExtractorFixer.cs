// -----------------------------------------------------------------------
//  <copyright file="MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorFixer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using IfStatementSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax;

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
            if (node is IfStatementSyntax || node is SwitchSectionSyntax || node is SwitchExpressionArmSyntax)
            {
                // special case - have to check for else if here
                if(node.Parent is ElseClauseSyntax)
                    return node.Parent;
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
        
        // check if this is an if statement we're removing
        if (nodeToRemove is IfStatementSyntax ifStatement)
        {
           // first edge case - are we the topmost if statement WITHOUT any children? If so we can just remove the entire thing
           // check if we have any descendants that are if or else statements
           var hasParentIfStatement = ifStatement.Parent is IfStatementSyntax;
           
            var otherElsesAndIfs = ifStatement.DescendantNodes().Where(c => c is IfStatementSyntax or ElseClauseSyntax).ToArray();
            if (hasParentIfStatement)
            {
                // ok, so we are not the top node - that makes this easy. Here is what we need to do:
                // 1. Start with the parent node
                // 2. Remove the nodeToRemove
                // 3. Add all of nodeToRemove's children as children of the parent node
                
                var parent = ifStatement.Parent;
                if(parent == null)
                    return document; // should never happen
                
                var newParent = parent.RemoveNode(nodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);
                if(newParent == null)
                    return document; // should never happen
                
                var newParentWithChildren = newParent.InsertNodesAfter(newParent.ChildNodes().Last(), otherElsesAndIfs);
                var updatedRoot = root.ReplaceNode(parent, newParentWithChildren);
                return document.WithSyntaxRoot(updatedRoot);
            }
        }
        
        var newRoot = root.RemoveNode(nodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);
        return newRoot == null ? document : document.WithSyntaxRoot(newRoot);
    }
}