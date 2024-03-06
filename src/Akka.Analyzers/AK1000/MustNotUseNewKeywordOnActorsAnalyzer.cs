// -----------------------------------------------------------------------
//  <copyright file="MustNotUseNewKeywordOnActorsAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context.Core;
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

            var akkaCore = akkaContext.AkkaCore;
            // Check if it's a subclass of ActorBase
            if (!typeSymbol.IsActorBaseSubclass(akkaCore))
                return;

            // Check if it's within the context of Props.Create
            if (
                IsInsidePropsCreate(objectCreation, ctx.SemanticModel, akkaCore) || 
                IsWithinIndirectActorProducerProduce(objectCreation, ctx.SemanticModel, akkaCore))
                return;

            var diagnostic = Diagnostic.Create(RuleDescriptors.Ak1000DoNotNewActors, objectCreation.GetLocation(),
                typeSymbol.Name);
            ctx.ReportDiagnostic(diagnostic);
        }, SyntaxKind.ObjectCreationExpression);
    }
    
    private static bool IsWithinIndirectActorProducerProduce(
        ObjectCreationExpressionSyntax objectCreation,
        SemanticModel semanticModel,
        IAkkaCoreContext akkaContext)
    {
        var enclosingMethod = objectCreation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (enclosingMethod == null)
            return false;

        if (semanticModel.GetDeclaredSymbol(enclosingMethod) is not { Name: "Produce" } methodSymbol)
            return false;

        var containingType = methodSymbol.ContainingType;
        if (containingType == null)
            return false;

        // Check if the containing type implements IIndirectActorProducer
        return containingType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, akkaContext.Actor.IIndirectActorProducerType));
    }


    private static bool IsInsidePropsCreate(
        ObjectCreationExpressionSyntax objectCreation,
        SemanticModel semanticModel,
        IAkkaCoreContext akkaContext)
    {
        // Traverse upwards in the syntax tree from the object creation expression
        var currentNode = objectCreation.Parent;

        bool insideLambda = false;

        while (currentNode != null)
        {
            // Check if we are inside a lambda expression
            if (currentNode is LambdaExpressionSyntax)
            {
                insideLambda = true;
            }

            // Check if the current node is an InvocationExpressionSyntax
            if (currentNode is InvocationExpressionSyntax invocation)
            {
                // Get the symbol for the method being invoked

                // Check if the method symbol is for Akka.Actor.Props.Create
                if (semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedMethodSymbol &&
                    SymbolEqualityComparer.Default.Equals(invokedMethodSymbol.ContainingType, akkaContext.Actor.PropsType) &&
                    invokedMethodSymbol.Name == "Create")
                {
                    return true;
                }
            }

            // Move to the next parent node
            currentNode = currentNode.Parent;
        }

        // If we are inside a lambda, but not inside Props.Create, we assume it might be a legitimate use case
        return insideLambda;
    }
}