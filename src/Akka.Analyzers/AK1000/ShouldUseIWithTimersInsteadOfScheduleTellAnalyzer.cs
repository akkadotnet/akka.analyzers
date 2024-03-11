// -----------------------------------------------------------------------
//  <copyright file="ShouldUseIWithTimerInsteadOfITellScheduler.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using System.Data.HashFunction.MurmurHash;
using System.Text;
using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShouldUseIWithTimersInsteadOfScheduleTellAnalyzer(): AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1004ShouldUseIWithTimersInsteadOfScheduleTell)
{
    public const string HashKey = "hash";
    
    private static readonly IMurmurHash2 Hasher = MurmurHash2Factory.Instance.Create(new MurmurHash2Config
    {
        HashSizeInBits = 32
    });
    
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);
        
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;
            
            // invocation must be a member access expression
            if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccessExpr)
                return;
            
            var semanticModel = ctx.SemanticModel;
            // Get the member symbol from the invocation expression
            if(semanticModel.GetSymbolInfo(memberAccessExpr).Symbol is not IMethodSymbol methodSymbol)
                return;
            
            // Check if the method name is `ScheduleTellOnce` or `ScheduleTellRepeatedly`
            ArgumentSyntax? receiver = null;
            ArgumentSyntax? sender = null;
            var refSymbols = akkaContext.AkkaCore.Actor.ITellScheduler.ScheduleTellOnce;
            if (refSymbols.Any(s => ReferenceEquals(methodSymbol, s)))
            {
                receiver = invocationExpr.ArgumentList.Arguments[1];
                sender = invocationExpr.ArgumentList.Arguments[3];
            }
            else
            {
                refSymbols = akkaContext.AkkaCore.Actor.ITellScheduler.ScheduleTellRepeatedly;
                if (refSymbols.Any(s => ReferenceEquals(methodSymbol, s)))
                {
                    receiver = invocationExpr.ArgumentList.Arguments[2];
                    sender = invocationExpr.ArgumentList.Arguments[4];
                }
            }

            // Check that both receiver and sender is Self
            if (!IsReferenceToSelf(receiver) || !IsReferenceToSelf(sender))
                return;
            
            var classDeclaration = invocationExpr.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration is null)
                return;
            
            var classBase = semanticModel.GetDeclaredSymbol(classDeclaration)?.BaseType;
            var coreContext = akkaContext.AkkaCore;
            // Check that the class declaration inherits from ActorBase
            if (classBase is null || !classBase.IsDerivedOrImplements(coreContext.Actor.ActorBaseType!))
                return;

            var invocationText = invocationExpr.WithoutTrivia().GetText().ToString();
            var hash = Hasher.ComputeHash(Encoding.UTF8.GetBytes(invocationText));
            
            ReportDiagnostic();
            return;
            
            void ReportDiagnostic()
            {
                var diagnostic = Diagnostic.Create(
                    descriptor: RuleDescriptors.Ak1004ShouldUseIWithTimersInsteadOfScheduleTell, 
                    location: invocationExpr.GetLocation(), 
                    properties: new Dictionary<string, string?>
                    {
                        [HashKey] = hash.AsHexString()
                    }.ToImmutableDictionary(),
                    "ScheduleTell invocation");
                ctx.ReportDiagnostic(diagnostic);
            }
        }, SyntaxKind.InvocationExpression);
    }

    private static bool IsReferenceToSelf(ArgumentSyntax? argument)
    {
        if (argument is null)
            return false;
        
        switch (argument.Expression)
        {
            case IdentifierNameSyntax { Identifier.Text: "Self" }:
            case MemberAccessExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.Text: "Context" }, Name.Identifier.Text: "Self"
            }:
                return true;
            default:
                return false;
        }
    }
}