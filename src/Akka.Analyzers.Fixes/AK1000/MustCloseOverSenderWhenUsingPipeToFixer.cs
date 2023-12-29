using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Akka.Analyzers.Fixes.AK1000;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
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
        var invocationExpr = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();
        if (invocationExpr is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use local variable for Sender",
                createChangedDocument: c => UseLocalVariableForSenderAsync(context.Document, invocationExpr, c),
                equivalenceKey: Key_FixPipeToSender),
            diagnostic);
    }
    
    private static async Task<Document> UseLocalVariableForSenderAsync(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        // assert that Root is not null
        if (root is null)
            return document;

        // Generate a local variable to hold 'Sender'
        var senderVariable = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier("sender"))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName("Sender")))))));

        // Replace 'this.Sender' with the local variable
        var newInvocation = invocationExpr.ReplaceNode(
            invocationExpr.DescendantNodes().OfType<MemberAccessExpressionSyntax>().First(),
            SyntaxFactory.IdentifierName("sender"));

        // Insert the local variable declaration before the invocation expression
        var newRoot = root.InsertNodesBefore(invocationExpr!, new[] { senderVariable });
        newRoot = newRoot.ReplaceNode(invocationExpr, newInvocation);

        return document.WithSyntaxRoot(newRoot);
    }
}