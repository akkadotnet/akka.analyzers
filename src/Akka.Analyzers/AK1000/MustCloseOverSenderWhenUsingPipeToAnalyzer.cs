using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustCloseOverSenderWhenUsingPipeToAnalyzer() : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1001CloseOverSenderUsingPipeTo)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);
        
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol is null || !classSymbol.IsActorBaseSubclass(akkaContext))
                return; // not an actor, skip

            // Analyze both methods and lambda expressions
            var members = classDeclaration.DescendantNodes().OfType<MemberDeclarationSyntax>();
            foreach (var member in members)
            {
                switch (member)
                {
                    case MethodDeclarationSyntax method:
                        AnalyzeMethodOrLambda(ctx, method.Body);
                        break;
                    case PropertyDeclarationSyntax property:
                        AnalyzeMethodOrLambda(ctx, property.AccessorList);
                        break;
                }
            }

        },SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeMethodOrLambda(SyntaxNodeAnalysisContext context, SyntaxNode? node)
    {
        if (node == null) return;
        
        var invocations = node.DescendantNodes().OfType<InvocationExpressionSyntax>();
        foreach (var invocation in invocations)
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax
                {
                    Name.Identifier.ValueText: "PipeTo"
                } memberAccessExpr) continue;
            
            var dataFlow = context.SemanticModel.AnalyzeDataFlow(node);
            if (!dataFlow.Captured.Any(symbol => symbol.Name == "Sender")) continue;
            
            var diagnostic = Diagnostic.Create(RuleDescriptors.Ak1001CloseOverSenderUsingPipeTo, memberAccessExpr.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}