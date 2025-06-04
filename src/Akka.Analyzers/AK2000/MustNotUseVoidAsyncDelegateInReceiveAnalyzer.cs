// -----------------------------------------------------------------------
//  <copyright file="MustNotUseVoidAsyncDelegateInReceiveOrCommandAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotUseVoidAsyncDelegateInReceiveAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak2003MustNotUseVoidAsyncDelegateInReceive)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            var inv = (InvocationExpressionSyntax)ctx.Node;
            if(ctx.SemanticModel.GetSymbolInfo(inv.Expression).Symbol is not IMethodSymbol method)
                return;

            if (method.IsGenericMethod)
                method = method.OriginalDefinition;
            
            var compilation = context.Compilation;
            INamedTypeSymbol? actionSymbol;
            if(akkaContext.AkkaCore.Actor.ReceiveActor.Receive.Any(m => SymbolEqualityComparer.Default.Equals(method, m)))
            {
                actionSymbol = compilation.GetTypeByMetadataName("System.Action`1");
            } 
            else if (akkaContext.AkkaCore.Actor.Dsl.IActorDsl.Receive.Any(m => ReferenceEquals(method, m)))
            {
                actionSymbol = compilation.GetTypeByMetadataName("System.Action`2");
            }
            else
            {
                return;
            }
            
            var index = 0;
            foreach (var p in method.Parameters)
            {
                if(ReferenceEquals(p.Type.OriginalDefinition, actionSymbol))
                    break;
                index++;
            }
            if(index >= method.Parameters.Length)
            {
                return;
            }
            
            var argExpr = inv.ArgumentList.Arguments[index].Expression;

            // async lambda?
            if (argExpr is LambdaExpressionSyntax lam && lam.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(RuleDescriptors.Ak2003MustNotUseVoidAsyncDelegateInReceive, lam.AsyncKeyword.GetLocation()));
                return;
            }
            // async anonymous method?
            if (argExpr is AnonymousMethodExpressionSyntax anon && anon.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(RuleDescriptors.Ak2003MustNotUseVoidAsyncDelegateInReceive, anon.AsyncKeyword.GetLocation()));
                return;
            }
            // method‐group: look up symbol
            if (ctx.SemanticModel.GetSymbolInfo(argExpr).Symbol is IMethodSymbol { IsAsync: true, ReturnsVoid: true })
            {
                ctx.ReportDiagnostic(Diagnostic.Create(RuleDescriptors.Ak2003MustNotUseVoidAsyncDelegateInReceive, argExpr.GetLocation()));
            }
        }, SyntaxKind.InvocationExpression);
    }
}