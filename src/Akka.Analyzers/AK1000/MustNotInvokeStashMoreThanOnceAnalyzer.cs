// -----------------------------------------------------------------------
//  <copyright file="MustNotInvokeStashMoreThanOnce.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotInvokeStashMoreThanOnceAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1008MustNotInvokeStashMoreThanOnce)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        // For methods inside the actor
        context.RegisterSyntaxNodeAction(ctx => AnalyzeMethodDeclaration(ctx, akkaContext), SyntaxKind.MethodDeclaration);
        
        // For lambdas, namely Receive<T> and ReceiveAny
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;
            var akkaCore = akkaContext.AkkaCore;
            var stashMethod = akkaCore.Actor.IStash.Stash!;

            var invocationSymbol = semanticModel.GetSymbolInfo(invocationExpr.Expression).Symbol;
            
            // if this invocation expression is not invoking a method OR it's not part of an actor base type, skip
            if (invocationSymbol is not null && invocationSymbol.ContainingType.IsActorBaseSubclass(akkaCore))
            {
                // if we've made it here, we are inside a context where at least one Stash call has been found
                // scope out and see if there are any other stash calls inside the same branch
                var invocationParent = invocationExpr.DescendantNodes();

                DiagnoseSyntaxNodes(invocationParent.ToArray(), semanticModel, stashMethod, ctx);
            }
            
            
            
        }, SyntaxKind.InvocationExpression); 
    }

    private static void DiagnoseSyntaxNodes(IReadOnlyList<SyntaxNode> invocationParent, SemanticModel semanticModel,
        IMethodSymbol stashMethod, SyntaxNodeAnalysisContext ctx)
    {
        // Find all "Stash.Stash()" calls in the method
        var stashCalls = invocationParent
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation => IsStashInvocation(semanticModel, invocation, stashMethod)).ToArray();

        // aren't enough calls to merit further analysis
        if (stashCalls.Length < 2)
            return;
        
        foreach(var stashCall in stashCalls)
        {
          stashCall.GetLocation();
        }
        
        // Group calls by their parent block
        var callsGroupedByBlock = stashCalls
            .GroupBy(GetContainingBlock)
            .Where(group => group.Key != null).ToArray();
        
        // so we don't log multiple warnings for the same call, in the event someone has done something truly stupid
        var alreadyTouched = new Dictionary<InvocationExpressionSyntax, bool>();
        
        foreach (var group in callsGroupedByBlock)
        {
            var callsInBlock = group.ToList();

            // simple / dumb check - are there multiple calls to Stash in the same block?
            if (callsInBlock.Count > 1)
            {
                foreach (var stashCall in callsInBlock)
                {
                    ReportIfNotDoneAlready(alreadyTouched, stashCall);
                }
            }
            
            // depth check
            var block = group.Key!;
            var descendantCalls = block.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Where(invocation => IsStashInvocation(semanticModel, invocation, stashMethod))
                .ToList();
                
            // if there are no other Stash calls in the block, we can skip this block
            if (descendantCalls.Count <= 1)
                continue;
                
            // if there are other Stash calls in the block, they're bad - flag em
            foreach (var descendantCall in descendantCalls)
            {
                ReportIfNotDoneAlready(alreadyTouched, descendantCall);
            }
                
            // also need to flag the original call
            ReportIfNotDoneAlready(alreadyTouched, callsInBlock[0]);
            
        }

        return;

        void ReportIfNotDoneAlready(Dictionary<InvocationExpressionSyntax, bool> dictionary, InvocationExpressionSyntax descendantCall)
        {
            // we've already flagged this call as a duplicate, skip it
            if (dictionary.TryGetValue(descendantCall, out var value) && value)
                return;
                    
            dictionary[descendantCall] = true;
            var diagnostic = Diagnostic.Create(
                descriptor:RuleDescriptors.Ak1008MustNotInvokeStashMoreThanOnce,
                location:descendantCall.GetLocation());
            ctx.ReportDiagnostic(diagnostic);
        }
    }
    
    /*
 // we could skip the first stash call here, but since we don't know _which_ call is the offending
                // duplicate, we'll just report all of them
                */

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context, AkkaContext akkaContext)
    {
        var semanticModel = context.SemanticModel;
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        
        // First: need to check if this method is declared in an ActorBase subclass
        var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
        if (methodSymbol is null || !methodSymbol.ContainingType.IsActorBaseSubclass(akkaContext.AkkaCore))
            return;

        var stashMethod = akkaContext.AkkaCore.Actor.IStash.Stash!;
        
        var invocationParent = methodDeclaration.DescendantNodes();
        DiagnoseSyntaxNodes(invocationParent.ToArray(), semanticModel, stashMethod, context);
    }
    
    private static bool IsStashInvocation(SemanticModel model, InvocationExpressionSyntax invocation, IMethodSymbol stashMethod)
    {
        var symbol = model.GetSymbolInfo(invocation).Symbol;
        return SymbolEqualityComparer.Default.Equals(symbol, stashMethod);
    }
    
    private static BlockSyntax? GetContainingBlock(SyntaxNode node)
    {
        return node.AncestorsAndSelf().OfType<BlockSyntax>().FirstOrDefault();
    }
    
    private static bool AreCallsSeparatedByConditionals(BlockSyntax block, List<InvocationExpressionSyntax> calls)
    {
        var conditionals = block.DescendantNodes().OfType<IfStatementSyntax>().ToList();

        foreach (var call in calls)
        {
            var isInConditional = conditionals.Any(ifStatement => ifStatement.Contains(call));

            if (!isInConditional)
            {
                return false;
            }
        }

        return true;
    }
}

