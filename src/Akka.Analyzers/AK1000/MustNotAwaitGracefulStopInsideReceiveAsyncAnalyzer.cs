// -----------------------------------------------------------------------
//  <copyright file="MustNotAwaitGracefulShutdownInsideReceive.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;
using Akka.Analyzers.Context;
using Akka.Analyzers.Context.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotAwaitGracefulStopInsideReceiveAsyncAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1002DoNotAwaitOnGracefulStop)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;
            var akkaCore = akkaContext.AkkaCore;
            
            if(semanticModel.GetSymbolInfo(invocationExpr.Expression).Symbol is not IMethodSymbol methodSymbol)
                return;
            
            // Unfold reduced extension method to its original method
            methodSymbol = methodSymbol.ReducedFrom ?? methodSymbol;
            
            // Method must be one of the GracefulStop() extension methods
            var refSymbols = akkaCore.Actor.GracefulStopSupportSupport.GracefulStop;
            if(!refSymbols.Any(s => ReferenceEquals(methodSymbol, s)))
                return;
            
            // Check 1: GracefulStop() should not be awaited
            if (invocationExpr.Parent is not AwaitExpressionSyntax awaitExpression)
                return;

            // Check 2: Ensure called within ReceiveAsync<T> or ReceiveAnyAsync lambda expression
            if (!invocationExpr.IsInsideReceiveAsyncLambda(semanticModel, akkaCore))
                return;

            // Check 3: Ensure method is accessing ActorBase.Self or ActorContext.Self
            if(!invocationExpr.IsAccessingActorSelf(semanticModel, akkaCore))
               return;
            
            var diagnostic = Diagnostic.Create(RuleDescriptors.Ak1002DoNotAwaitOnGracefulStop, awaitExpression.GetLocation());
            ctx.ReportDiagnostic(diagnostic);
        }, SyntaxKind.InvocationExpression);
    }
}