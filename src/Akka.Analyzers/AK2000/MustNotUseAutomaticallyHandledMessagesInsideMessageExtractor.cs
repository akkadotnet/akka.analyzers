﻿// -----------------------------------------------------------------------
//  <copyright file="MustNotUseAutomaticallyHandledMessagesInsideMessageExtractor.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotUseAutomaticallyHandledMessagesInsideMessageExtractor()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak2001DoNotUseAutomaticallyHandledMessagesInShardMessageExtractor)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            if (akkaContext.HasAkkaClusterShardingInstalled == false)
                return; // exit early if we don't have Akka.Cluster.Sharding installed

            AnalyzeMethod(ctx, akkaContext);
        }, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext ctx, AkkaContext akkaContext)
    {
        var methodDeclaration = (MethodDeclarationSyntax)ctx.Node;
        var semanticModel = ctx.SemanticModel;
        var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);

        INamedTypeSymbol? messageExtractorSymbol = akkaContext.AkkaClusterSharding.IMessageExtractorType;

        if (methodSymbol == null || messageExtractorSymbol == null)
            return;

        var containingTypeIsMessageExtractor = methodSymbol.ContainingType.AllInterfaces.Any(i =>
            SymbolEqualityComparer.Default.Equals(i, messageExtractorSymbol));

        if (!containingTypeIsMessageExtractor)
            return;

        var messageExtractorMethods = messageExtractorSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.Name is "EntityMessage" or "EntityId")
            .ToArray();

        var forbiddenTypes = new[]
            { akkaContext.AkkaClusterSharding.StartEntityType, akkaContext.AkkaClusterSharding.ShardEnvelopeType };
            
        var reportedLocations = new HashSet<Location>();
            
        // we know for sure that we are inside a message extractor now
        foreach (var interfaceMember in methodSymbol.ContainingType.AllInterfaces.SelectMany(i =>
                     i.GetMembers().OfType<IMethodSymbol>()))
        {
            foreach (var extractorMethod in messageExtractorMethods)
            {
                if (SymbolEqualityComparer.Default.Equals(interfaceMember, extractorMethod))
                {
                    // Retrieve all the descendant nodes of the method that are expressions
                    var descendantNodes = methodDeclaration.DescendantNodes();

                    foreach (var node in descendantNodes)
                    {
                        AnalyzeDeclaredVariableNodes(ctx, node, forbiddenTypes, reportedLocations);
                    }
                }
            }
        }
    }

    private static void AnalyzeDeclaredVariableNodes(SyntaxNodeAnalysisContext ctx, SyntaxNode node,
        INamedTypeSymbol?[] forbiddenTypes, HashSet<Location> reportedLocations)
    {
        var semanticModel = ctx.SemanticModel;
        switch (node)
        {
            case DeclarationPatternSyntax declarationPatternSyntax:
            {
                // get the symbol for the declarationPatternSyntax.Type
                var variableType = semanticModel.GetTypeInfo(declarationPatternSyntax.Type).Type;

                if (forbiddenTypes.Any(t => SymbolEqualityComparer.Default.Equals(t, variableType)))
                {
                    var location = declarationPatternSyntax.GetLocation();
                                        
                    // duplicate
                    if(reportedLocations.Contains(location))
                        break;
                    var diagnostic = Diagnostic.Create(
                        RuleDescriptors
                            .Ak2001DoNotUseAutomaticallyHandledMessagesInShardMessageExtractor,
                        location);
                    ctx.ReportDiagnostic(diagnostic);
                    reportedLocations.Add(location);
                }

                break;
            }
        }
    }
}