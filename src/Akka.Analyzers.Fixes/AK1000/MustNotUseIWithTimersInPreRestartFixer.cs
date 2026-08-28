// -----------------------------------------------------------------------
//  <copyright file="MustNotUseIWithTimersInPreRestartFixer.cs" company="Akka.NET Project">
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

namespace Akka.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public class MustNotUseIWithTimersInPreRestartFixer() 
    : BatchedCodeFixProvider(RuleDescriptors.Ak1007MustNotUseIWithTimersInPreRestart.Id)
{
    public const string Key_FixITimerScheduler = "AK1007_FixITimerScheduler";
    
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic is null)
            return;
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the ITimerScheduler method invocation expression
        var invocationExpr = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>().First();
        if (invocationExpr is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use local variable for Sender",
                c => MoveITimerSchedulerMethodToPostRestartAsync(context.Document, invocationExpr, c),
                Key_FixITimerScheduler),
            context.Diagnostics);
    }

    private static async Task<Document> MoveITimerSchedulerMethodToPostRestartAsync(
        Document document,
        InvocationExpressionSyntax invocationExprSyntax, 
        CancellationToken cancellationToken)
    {
        #region Remove ITimerScheduler method invocation from AroundPreRestart or PreRestart

        // Find the containing method declaration (AroundPreRestart/PreRestart)
        var preRestartMethod = invocationExprSyntax.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (preRestartMethod is null)
            return document;
        
        // Find the containing class declaration
        var classDeclaration = preRestartMethod.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDeclaration is null)
            return document;

        // Remove the invocation expression from the containing method
        MethodDeclarationSyntax? newPreRestartMethod = null;
        if (preRestartMethod.Body is not null)
        {
            var statements = preRestartMethod.Body.Statements.Where(st =>
                st is ExpressionStatementSyntax { Expression: InvocationExpressionSyntax i } &&
                !ReferenceEquals(i, invocationExprSyntax)
            );
            newPreRestartMethod = preRestartMethod.WithBody(SyntaxFactory.Block(statements));
        }

        #endregion

        #region Create or append to PostRestart

        var existingPostRestartMethod = classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "PostRestart");

        if (existingPostRestartMethod is not null)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        
            // if the PreRestart method is a one-liner, just remove it
            if (newPreRestartMethod is null)
            {
                editor.RemoveNode(preRestartMethod);
            }
            // else, replace it with the new one
            else
            {
                // Replace the original method with the new one in the syntax tree
                editor.ReplaceNode(preRestartMethod, newPreRestartMethod);
            }
            
            // Append to existing PostRestart
            var newPostRestartMethod = existingPostRestartMethod
                .AddBodyStatements(SyntaxFactory.ExpressionStatement(invocationExprSyntax));
            editor.ReplaceNode(existingPostRestartMethod, newPostRestartMethod);
            
            var newDocument = editor.GetChangedDocument();
            return newDocument;
        }
        else
        {
            // Create the invocation expression for 'base.PostRestart(reason)'
            var baseInvocationExpression = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.BaseExpression(),
                    SyntaxFactory.IdentifierName("PostRestart")
                ),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("reason"))))
            );
            
            // Create PostRestart Method
            var newPostRestartMethod = SyntaxFactory
                .MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), 
                    "PostRestart")
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), 
                    SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("reason"))
                        .WithType(SyntaxFactory.ParseTypeName("Exception")))))
                .WithBody(SyntaxFactory.Block(SyntaxFactory.List<StatementSyntax>(
                    new[]
                    {
                        SyntaxFactory.ExpressionStatement(baseInvocationExpression),
                        SyntaxFactory.ExpressionStatement(invocationExprSyntax)
                    }
                )));

            ClassDeclarationSyntax newClassDeclaration;
            if (newPreRestartMethod is not null)
            {
                newClassDeclaration = classDeclaration.ReplaceNode(preRestartMethod, newPreRestartMethod);
                newClassDeclaration = newClassDeclaration.AddMembers(newPostRestartMethod);
            }
            else
            {
                newClassDeclaration = classDeclaration.AddMembers(newPostRestartMethod);
            }

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.ReplaceNode(classDeclaration, newClassDeclaration);
            
            var newDocument = editor.GetChangedDocument();
            return newDocument;
        }
        #endregion
    }
}