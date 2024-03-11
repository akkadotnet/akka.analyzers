// -----------------------------------------------------------------------
//  <copyright file="ShouldUseIWithTimersInsteadOfScheduleTellFixer.cs" company="Akka.NET Project">
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
using Xunit.Abstractions;

namespace Akka.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public class ShouldUseIWithTimersInsteadOfScheduleTellFixer(): BatchedCodeFixProvider(RuleDescriptors.Ak1004ShouldUseIWithTimersInsteadOfScheduleTell.Id)
{
#pragma warning disable CA2211
    public static ITestOutputHelper? Output;
#pragma warning restore CA2211
    
    public const string Key_ScheduleTell = "AK1004_ScheduleTell";
    private const string TimerKeyPrefix = "TimerKey_";

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the PipeTo invocation expression
        var invocationExpr = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>().First();
        if (invocationExpr is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use StartSingleTimer() or StartPeriodicTimer() instead of ScheduleTellOnce() or ScheduleTellRepeatedly()",
                c => ReplaceScheduleTellWithIWithTimersAsync(context.Document, invocationExpr, diagnostic, c),
                Key_ScheduleTell),
            context.Diagnostics);
    }

    private static async Task<Document> ReplaceScheduleTellWithIWithTimersAsync(
        Document document,
        InvocationExpressionSyntax invocationExpr, 
        Diagnostic diagnostic,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var generator = SyntaxGenerator.GetGenerator(document);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        // Find the class declaration
        var classDeclaration = invocationExpr.Ancestors().OfType<ClassDeclarationSyntax>().First();

        var firstMember = classDeclaration.Members.First();
        var leadingTrivia = firstMember.GetLeadingTrivia();
        var trailingTrivia = firstMember.GetTrailingTrivia();
        
        // find the next valid timer key name
        var hash = diagnostic.Properties[ShouldUseIWithTimersInsteadOfScheduleTellAnalyzer.HashKey] ?? string.Empty;
        var timerName = $"{TimerKeyPrefix}{hash}";
        
        // Create the new field declaration for TimerKey
        var timerKeyDeclaration = GenerateTimerKeyField(hash, leadingTrivia, trailingTrivia);
        
        // Get the invocation expression symbol
        var arguments = invocationExpr.ArgumentList.Arguments;
        var symbol = semanticModel.GetSymbolInfo(invocationExpr.Expression, cancellationToken).Symbol!;
        var newInvocation = symbol.Name switch
        {
            "ScheduleTellOnce" => generator.InvocationExpression(
                    expression: generator.MemberAccessExpression(
                        generator.IdentifierName("Timers"),
                        "StartSingleTimer"
                    ),
                    arguments:
                    [
                        generator.Argument(generator.IdentifierName(timerName)),
                        arguments[2], // message
                        arguments[0]  // timeout
                    ]
                ).NormalizeWhitespace().WithTriviaFrom(invocationExpr),
            
            _ => generator.InvocationExpression(
                    expression: generator.MemberAccessExpression(
                        generator.IdentifierName("Timers"),
                        "StartPeriodicTimer"
                    ),
                    arguments:
                    [
                        generator.Argument(generator.IdentifierName(timerName)),
                        arguments[3], // message
                        arguments[0], // delay
                        arguments[1]  // interval
                    ]
                ).NormalizeWhitespace().WithTriviaFrom(invocationExpr)
        };
        
        // replace original invocation with new one
        var newClassDeclaration = classDeclaration.ReplaceNode(invocationExpr, newInvocation);

        // Add the timer key field declaration to the top of the class
        newClassDeclaration = newClassDeclaration.WithMembers(newClassDeclaration.Members.Insert(0, timerKeyDeclaration));
        
        // Inject IWithTimers interface, if needed
        // Create the new base list syntax with the interface if not already implemented
        var baseList = newClassDeclaration.BaseList ?? SyntaxFactory.BaseList();
        if (baseList.Types.All(t => t.ToString() != "IWithTimers"))
        {
            // Specify the interface to add
            var interfaceTypeName = generator.IdentifierName("IWithTimers");
            baseList = baseList
                .AddTypes(SyntaxFactory.SimpleBaseType((TypeSyntax)interfaceTypeName))
                .NormalizeWhitespace()
                .WithTrailingTrivia(baseList.GetTrailingTrivia());
            newClassDeclaration =
                newClassDeclaration.WithBaseList(baseList);

            var timersProperty = SyntaxFactory
                .PropertyDeclaration(SyntaxFactory.ParseTypeName("ITimerScheduler"), "Timers")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                )
                .NormalizeWhitespace()
                .WithLeadingTrivia(leadingTrivia)
                .WithTrailingTrivia(trailingTrivia);

            newClassDeclaration = newClassDeclaration.AddMembers(timersProperty);
        }
        
        // Replace old class declaration with new one
        editor.ReplaceNode(classDeclaration, newClassDeclaration);
        
        var newDocument = editor.GetChangedDocument();
        if (Output is not null)
        {
            var content = (await newDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!.ToString();
            Output.WriteLine($"========== {diagnostic.Properties["hash"]}\n{content}\n==========");
        }
        return newDocument;
    }

    private static FieldDeclarationSyntax GenerateTimerKeyField(string hash, SyntaxTriviaList leadingTrivia, SyntaxTriviaList trailingTrivia)
    {
        return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier($"{TimerKeyPrefix}{hash}"))
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal($"{hash} - PLEASE REFACTOR THIS")))))))
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.ConstKeyword)))
            .NormalizeWhitespace()
            .WithLeadingTrivia(leadingTrivia)
            .WithTrailingTrivia(trailingTrivia);
    }
}
