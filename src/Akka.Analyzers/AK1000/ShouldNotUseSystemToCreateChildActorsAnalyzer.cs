// -----------------------------------------------------------------------
//  <copyright file="MustCloseOverSenderWhenUsingReceiveAsyncAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShouldNotUseSystemToCreateChildActorsAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1008ShouldNotUseSystemToCreateChildActor)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        // Both methods this rule matches are named ActorOf.
        var actorOfNames = new[]
            {
                akkaContext.AkkaCore.Actor.ActorSystem.ActorOf,
                akkaContext.AkkaCore.Actor.ActorRefFactoryExtensions.ActorOf
            }
            .Where(m => m is not null)
            .Select(m => m!.Name)
            .ToImmutableHashSet();
        if (actorOfNames.IsEmpty)
            return;

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpression = (InvocationExpressionSyntax)ctx.Node;

            // Reject by name before binding.
            if (!invocationExpression.CouldInvokeAnyOf(actorOfNames))
                return;

            var semanticModel = ctx.SemanticModel;
            
            // Get the member symbol from the invocation expression
            if(semanticModel.GetSymbolInfo(invocationExpression.Expression).Symbol is not IMethodSymbol methodInvocationSymbol)
                return;
            
            // Make sure we get the actual method if it's an extension method
            if (methodInvocationSymbol.IsExtensionMethod)
            {
                // Method must be accessing a member of an instance
                if(invocationExpression.Expression is not MemberAccessExpressionSyntax memberAccess)
                    return;
                
                // Accessed member must be of type `ActorSystem`
                var receiverType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (receiverType is null)
                    return;

                if (!receiverType.IsDerivedOrImplements(akkaContext.AkkaCore.Actor.ActorSystemType!))
                    return;
                
                // Make sure that we're accessing the actual method
                while (methodInvocationSymbol.ReducedFrom is not null)
                {
                    methodInvocationSymbol = methodInvocationSymbol.ReducedFrom;
                }

                // Method must be the `ActorOf<T>()` extension method
                if (!ReferenceEquals(methodInvocationSymbol, akkaContext.AkkaCore.Actor.ActorRefFactoryExtensions.ActorOf))
                    return;
            }
            else
            {
                // Check if the method matches any `ActorOf` methods
                if (!ReferenceEquals(methodInvocationSymbol, akkaContext.AkkaCore.Actor.ActorSystem.ActorOf))
                    return;
            }
            
            // Traverse up the parent nodes to see if any of them are ActorBase classes
            AssertInvocationIsInClassType(invocationExpression, semanticModel, akkaContext.AkkaCore.Actor.ActorBaseType, ctx);
            
        }, SyntaxKind.InvocationExpression);
    }

    /// <summary>
    /// Traverse up the parent nodes to see if any of them derived from a certain class
    /// </summary>
    private static void AssertInvocationIsInClassType(
        InvocationExpressionSyntax invocationExpression, 
        SemanticModel semanticModel,
        INamedTypeSymbol? classType,
        SyntaxNodeAnalysisContext context)
    {
        if(classType is null)
            return;
        
        var parent = invocationExpression.Parent;
        while (parent != null)
        {
            if (parent is ClassDeclarationSyntax classDeclaration)
            {
                var isActorType = classDeclaration.IsDerivedOrImplements(semanticModel, classType);
                if (isActorType)
                {
                    // If found, report a diagnostic
                    var diagnostic = Diagnostic.Create(
                        descriptor: RuleDescriptors.Ak1008ShouldNotUseSystemToCreateChildActor, 
                        location: invocationExpression.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }
                
            parent = parent.Parent;
        }
    }
}