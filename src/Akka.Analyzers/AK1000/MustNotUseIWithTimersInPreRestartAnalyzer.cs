// -----------------------------------------------------------------------
//  <copyright file="MustNotUseIWithTimersInPreRestartAnalyzer.cs" company="Akka.NET Project">
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
public class MustNotUseIWithTimersInPreRestartAnalyzer(): AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1007MustNotUseIWithTimersInPreRestart)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);
        
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;

            if (semanticModel.GetSymbolInfo(invocationExpr).Symbol is not IMethodSymbol methodInvocationSymbol)
                return;
            
            // Invocation expression must be either `ITimerScheduler.StartPeriodicTimer()` or `ITimerScheduler.StartSingleTimer()`
            var iWithTimers = akkaContext.AkkaCore.Actor.ITimerScheduler;
            var refMethods = iWithTimers.StartPeriodicTimer.AddRange(iWithTimers.StartSingleTimer);
            if (!methodInvocationSymbol.MatchesAny(refMethods))
                return;
            
            // Grab the enclosing method declaration
            var methodDeclaration = invocationExpr.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodDeclaration is null)
                return;

            var methodDeclarationSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if(methodDeclarationSymbol is null)
                return;
            
            // Method declaration must be `ActorBase.PreRestart()` or `ActorBase.AroundPreRestart()`
            var actorBase = akkaContext.AkkaCore.Actor.ActorBase;
            refMethods = new[] { actorBase.PreRestart!, actorBase.AroundPreRestart! }.ToImmutableArray();
            if (!methodDeclarationSymbol.OverridesAny(refMethods))
                return;

            var diagnostic = Diagnostic.Create(
                descriptor: RuleDescriptors.Ak1007MustNotUseIWithTimersInPreRestart, 
                location: invocationExpr.GetLocation(),
                messageArgs: [methodInvocationSymbol.Name, methodDeclarationSymbol.Name]);
            ctx.ReportDiagnostic(diagnostic);

        }, SyntaxKind.InvocationExpression);
    }
}