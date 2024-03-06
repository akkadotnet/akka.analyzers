// -----------------------------------------------------------------------
//  <copyright file="MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak2001DoNotUseAutomaticallyHandledMessagesInShardMessageExtractor)
{
    private static readonly Version RelevantVersion = new(1, 5, 15, 0);
    
    protected override bool ShouldAnalyze(AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(akkaContext);
        
        // Akka.Cluster.Sharding has to be installed and the version of it used has to be greater than or equal to v1.5.15
        return akkaContext.HasAkkaClusterShardingInstalled && akkaContext.AkkaClusterSharding.Version >= RelevantVersion;
    }

    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            AnalyzeMethodDeclaration(ctx, akkaContext);
        }, SyntaxKind.MethodDeclaration);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;
            if (semanticModel.GetSymbolInfo(invocationExpr).Symbol is not IMethodSymbol methodSymbol)
                return; // couldn't find the symbol, bail out quickly

            var hashCodeMessageExtractorSymbol =
                context.Compilation.GetTypeByMetadataName("Akka.Cluster.Sharding.HashCodeMessageExtractor");
            if (hashCodeMessageExtractorSymbol == null)
                return; // couldn't find the type

            if (SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, hashCodeMessageExtractorSymbol) &&
                methodSymbol is { IsStatic: true, Name: "Create" })
            {
                
                // we are invoking the HashCodeMessageExtractor.Create method if we've made it this far
                AnalyzeLambdaExpressions(invocationExpr.ArgumentList.Arguments, ctx, akkaContext);
            }
        }, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeLambdaExpressions(SeparatedSyntaxList<ArgumentSyntax> argumentListArguments, SyntaxNodeAnalysisContext ctx, AkkaContext akkaContext)
    {
        var forbiddenTypes = GetForbiddenTypes(akkaContext);
        var reportedLocations = new HashSet<Location>();
        
        foreach (var argument in argumentListArguments)
        {
            // if the argument is a lambda expression, we need to analyze it
            if (argument.Expression is LambdaExpressionSyntax lambdaExpression)
            {
                var descendantNodes = lambdaExpression.DescendantNodes();
                
                foreach (var node in descendantNodes)
                {
                    AnalyzeDeclaredVariableNodes(ctx, node, forbiddenTypes, reportedLocations);
                }
            }
        }
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext ctx, AkkaContext akkaContext)
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

        INamedTypeSymbol?[] forbiddenTypes = GetForbiddenTypes(akkaContext);

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

    private static INamedTypeSymbol?[] GetForbiddenTypes(AkkaContext akkaContext)
    {
        var forbiddenTypes = new[]
            { akkaContext.AkkaClusterSharding.StartEntityType, akkaContext.AkkaClusterSharding.ShardEnvelopeType };
        return forbiddenTypes;
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
                    if (reportedLocations.Contains(location))
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