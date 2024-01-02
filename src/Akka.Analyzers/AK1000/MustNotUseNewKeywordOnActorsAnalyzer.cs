// -----------------------------------------------------------------------
//  <copyright file="MustNotUseNewKeywordOnActorsAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotUseNewKeywordOnActorsAnalyzer() : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1000DoNotNewActors)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var objectCreation = (ObjectCreationExpressionSyntax)ctx.Node;

            if (ModelExtensions.GetTypeInfo(ctx.SemanticModel, objectCreation).Type is not INamedTypeSymbol typeSymbol)
                return;

            // Check if it's a subclass of ActorBase
            if (!typeSymbol.IsActorBaseSubclass(akkaContext))
                return;

            // Check if it's within the context of Props.Create
            if (IsInsidePropsCreate(objectCreation))
                return;

            var diagnostic = Diagnostic.Create(RuleDescriptors.Ak1000DoNotNewActors, objectCreation.GetLocation(),
                typeSymbol.Name);
            ctx.ReportDiagnostic(diagnostic);
        }, SyntaxKind.ObjectCreationExpression);
    }

    private static bool IsInsidePropsCreate(ObjectCreationExpressionSyntax objectCreation)
    {
        // Traverse upwards in the syntax tree from the object creation expression
        var currentNode = objectCreation.Parent;

        while (currentNode != null)
        {
            // Check if the current node is an ArgumentSyntax, which could be a part of a method call
            if (currentNode is ArgumentSyntax
                {
                    Parent.Parent: InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax
                        {
                            Name.Identifier.ValueText: "Create", Expression: IdentifierNameSyntax
                            {
                                Identifier.ValueText: "Props"
                            }
                        }
                    }
                })
                // Get the parent InvocationExpressionSyntax, if any
                // Check if the method being called is 'Props.Create'
                return true;

            // Move to the next parent node
            currentNode = currentNode.Parent;
        }

        return false;
    }
}