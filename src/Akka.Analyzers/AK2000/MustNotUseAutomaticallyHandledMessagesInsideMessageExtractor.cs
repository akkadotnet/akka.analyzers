// -----------------------------------------------------------------------
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

            var methodDeclaration = (MethodDeclarationSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);

            INamedTypeSymbol? messageExtractorSymbol = akkaContext.AkkaClusterSharding.IMessageExtractorType;
            
            if (methodSymbol == null || messageExtractorSymbol == null)
                return;
            
            var messageExtractorMethods = messageExtractorSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.Name is "EntityMessage" or "EntityId");
            
            var forbiddenTypes = new[]
                { akkaContext.AkkaClusterSharding.StartEntityType, akkaContext.AkkaClusterSharding.ShardEnvelopeType };
            
            foreach (var extractorMethod in messageExtractorMethods)
            {
                var containingTypeIsMessageExtractor = methodSymbol.ContainingType.AllInterfaces.Any(i =>
                    SymbolEqualityComparer.Default.Equals(i, messageExtractorSymbol));
                
                var methodOverridesMessageExtractorMethod = methodSymbol.OverriddenMethod != null &&
                                                            SymbolEqualityComparer.Default.Equals(methodSymbol.OverriddenMethod, extractorMethod);
                
                if (containingTypeIsMessageExtractor && methodOverridesMessageExtractorMethod)
                {
                    // Retrieve all the descendant nodes of the method that are expressions
                    var descendantNodes = methodDeclaration.DescendantNodes().OfType<ExpressionSyntax>();

                    foreach (var expression in descendantNodes)
                    {
                        var typeInfo = ctx.SemanticModel.GetTypeInfo(expression);

                        // Check if the type of the expression matches any of the forbidden type symbols
                        if (forbiddenTypes.Any(forbiddenType => SymbolEqualityComparer.Default.Equals(typeInfo.Type, forbiddenType)))
                        {
                            var diagnostic = Diagnostic.Create(RuleDescriptors.Ak2001DoNotUseAutomaticallyHandledMessagesInShardMessageExtractor,
                                expression.GetLocation());
                            ctx.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }, SyntaxKind.MethodDeclaration);
    }
}