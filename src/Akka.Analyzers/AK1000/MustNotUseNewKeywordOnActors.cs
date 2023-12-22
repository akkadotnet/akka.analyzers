// -----------------------------------------------------------------------
//  <copyright file="MustNotUseNewKeywordOnActors.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotUseNewKeywordOnActors() : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1000DoNotNewActors)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);
        
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var objectCreation = (ObjectCreationExpressionSyntax) ctx.Node;
            
            var typeSymbol = ModelExtensions.GetTypeInfo(ctx.SemanticModel, objectCreation).Type;
            if (typeSymbol is null)
                return;
            
            if (typeSymbol.IsActorType(akkaContext.AkkaCore))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(RuleDescriptors.Ak1000DoNotNewActors, objectCreation.GetLocation()));
            }
        }, SyntaxKind.ObjectCreationExpression);
    }
}