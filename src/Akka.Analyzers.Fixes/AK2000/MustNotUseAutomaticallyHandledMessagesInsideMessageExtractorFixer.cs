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
using Microsoft.CodeAnalysis.Formatting;
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

        SyntaxNode? newRoot = null;
        
        // check if this is an if statement we're removing
        if (nodeToRemove is IfStatementSyntax ifStatement)
        {
            newRoot = RemoveForbiddenIfStatement(root, ifStatement);
        }
        else if (nodeToRemove is ElseClauseSyntax elseClauseSyntax)
        {
            // check if this is an else-if we're removing
            if (elseClauseSyntax.Statement is IfStatementSyntax elseIfStatement)
            {
                newRoot = RemoveForbiddenIfStatement(root, elseIfStatement);
            }
            else
            {
                // remove the else clause entirely
                newRoot = root.RemoveNode(nodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);
            }
        }
        else
        {
            newRoot = root.RemoveNode(nodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);
        }
        
        return newRoot == null ? document : document.WithSyntaxRoot(newRoot);
    }
    
    private static SyntaxNode? RemoveForbiddenIfStatement(SyntaxNode root, IfStatementSyntax forbiddenIfStatement)
    {
        // Check if the if statement is part of an else-if chain
        if (forbiddenIfStatement.Parent is ElseClauseSyntax elseClause &&
            elseClause.Statement is IfStatementSyntax)
        {
            // Handle removing an else-if branch
            if (elseClause.Parent is IfStatementSyntax parentIfStatement)
            {
                // Check if there's a subsequent else/else-if after the current else-if
                if (forbiddenIfStatement.Else != null)
                {
                    // Replace the current else-if with its own else
                    var newElseClause = elseClause.WithStatement(forbiddenIfStatement.Else.Statement);
                    var newParentIfStatement = parentIfStatement.WithElse(newElseClause);
                    return root.ReplaceNode(parentIfStatement, newParentIfStatement).WithLeadingTrivia();
                }
                else
                {
                    // Remove the else-if branch entirely
                    var newParentIfStatement = parentIfStatement.WithElse(null);
                    return root.ReplaceNode(parentIfStatement, newParentIfStatement).WithLeadingTrivia();
                }
            }
        }
    
        // Handle removing a standalone if statement
        if (forbiddenIfStatement.Else == null)
        {
            // If there's no else part, remove the entire if statement
            return root.RemoveNode(forbiddenIfStatement, SyntaxRemoveOptions.KeepNoTrivia);
        }
        else
        {
            var leadingTrivia = forbiddenIfStatement.GetLeadingTrivia();
            
            // If there's an else part, replace the if statement with the else part
            return root.ReplaceNode(forbiddenIfStatement, forbiddenIfStatement.Else.Statement.WithLeadingTrivia(leadingTrivia));
        }
    }

}