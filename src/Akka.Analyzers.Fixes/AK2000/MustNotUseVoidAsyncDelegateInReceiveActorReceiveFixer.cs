// -----------------------------------------------------------------------
//  <copyright file="MustNotUseVoidAsyncDelegateInReceiveFixer.cs" company="Akka.NET Project">
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
public class MustNotUseVoidAsyncDelegateInReceiveActorReceiveFixer()
    : BatchedCodeFixProvider(
        RuleDescriptors.Ak2003MustNotUseVoidAsyncDelegateInReceiveActorReceive.Id,
        RuleDescriptors.Ak2004MustNotUseVoidAsyncDelegateInDslActorReceive.Id)
{
    public const string Key_FixReceiveWithVoidAsyncDelegate = "AK2003_FixReceiveWithVoidAsyncDelegate";
    
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // 1) Get the syntax root
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        // 2) Find the node at the diagnostic's span
        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic is null)
            return;
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan);
        
        // 3) Find the enclosing InvocationExpressionSyntax
        var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation == null)
            return;
        
        // 4) Check if any argument is an async lambda. If not, skip registering a fix.
        var hasAsyncLambda = invocation.ArgumentList.Arguments
            .Any(a => a.Expression is LambdaExpressionSyntax lambda && lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword));
        if (!hasAsyncLambda)
            return;
        
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace Receive with ReceiveAsync",
                createChangedDocument: c => ReplaceReceiveWithReceiveAsync(context.Document, invocation, c),
                equivalenceKey: Key_FixReceiveWithVoidAsyncDelegate),
            diagnostic);
    }

    private static async Task<Document> ReplaceReceiveWithReceiveAsync(Document document,
        InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        // We want to keep the same type arguments (e.g. <int>) but rename "Receive" → "ReceiveAsync".
        // invocation.Expression can be:
        //   • GenericNameSyntax  (e.g. Receive<int>)
        //   • MemberAccessExpressionSyntax whose .Name is a GenericNameSyntax (e.g. this.Receive<int>)
        //   • (rarely) a bare IdentifierNameSyntax

        var expression = invocation.Expression;

        var oldNameNode = expression switch
        {
            GenericNameSyntax genericName => (SimpleNameSyntax)genericName,
            MemberAccessExpressionSyntax { Name: GenericNameSyntax memberGeneric } => memberGeneric,
            IdentifierNameSyntax identifierName => identifierName,
            _ => null
        };

        if(oldNameNode is null)
        {
            // If we don’t find a recognizable simple name, bail out.
            return document;
        }

        // Build a new SimpleNameSyntax with identifier "ReceiveAsync", preserving any type arguments.
        SimpleNameSyntax newNameNode;
        if (oldNameNode is GenericNameSyntax oldGeneric)
        {
            newNameNode = SyntaxFactory.GenericName(SyntaxFactory.Identifier("ReceiveAsync"), oldGeneric.TypeArgumentList);
        }
        else
        {
            newNameNode = SyntaxFactory.IdentifierName("ReceiveAsync");
        }

        // If the old expression was a member‐access (e.g. this.Receive<int>), replace only the Name:
        SyntaxNode replacedExpression;
        if (expression is MemberAccessExpressionSyntax oldMemberAccess)
        {
            replacedExpression = oldMemberAccess.WithName(newNameNode);
        }
        else
        {
            // Otherwise, replace the entire expression (Receive<int> → ReceiveAsync<int>)
            replacedExpression = newNameNode;
        }

        // Create a new InvocationExpressionSyntax with the replaced expression
        var newInvocation = invocation.WithExpression((ExpressionSyntax)replacedExpression);

        // Replace invocation in the syntax tree
        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = oldRoot!.ReplaceNode(invocation, newInvocation);

        return document.WithSyntaxRoot(newRoot);
    }
}