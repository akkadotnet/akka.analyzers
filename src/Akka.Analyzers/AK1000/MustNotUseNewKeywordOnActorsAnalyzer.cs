// -----------------------------------------------------------------------
//  <copyright file="MustNotUseNewKeywordOnActorsAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
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

        var methodSymbol = semanticModel.GetDeclaredSymbol(enclosingMethod);
        if(methodSymbol is null)
            return false;

        var refSymbols = akkaContext.Actor.IIndirectActorProducer.Produce;
        // Check for explicit interface implementation
        if (methodSymbol.ExplicitInterfaceImplementations.Any(s => refSymbols.Any(r => ReferenceEquals(s, r))))
            return true;
        
        // Check for implicit interface implementation
        foreach (var interfaceMember in methodSymbol.ContainingType.AllInterfaces.SelectMany(i => i.GetMembers().OfType<IMethodSymbol>()))
        {
            if (refSymbols.Any(s => ReferenceEquals(interfaceMember, s)))
                return true;
        }
        
        return false;
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
                if (semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedMethodSymbol)
                {
                    var refSymbols = akkaContext.Actor.Props.Create;
                    if(refSymbols.Any(s => SymbolEqualityComparer.Default.Equals(invokedMethodSymbol, s)))
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