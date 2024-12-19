// -----------------------------------------------------------------------
//  <copyright file="MustNotInvokeStashMoreThanOnce.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        context.RegisterSyntaxNodeAction(ctx => AnalyzeMethod(ctx, akkaContext), SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, AkkaContext akkaContext)
    {
        var semanticModel = context.SemanticModel;
        
        // TODO: ControlFlowGraph does not recurse into local functions and lambda anonymous functions, how to grab those? 
        var controlFlowGraph = ControlFlowGraph.Create(context.Node, semanticModel);

        if (controlFlowGraph == null)
            return;

        var stashMethod = akkaContext.AkkaCore.Actor.IStash.Stash!;
        var stashInvocations = new Dictionary<BasicBlock, int>();
        
        // Track Stash.Stash() calls inside each blocks
        foreach (var block in controlFlowGraph.Blocks)
        {
            AnalyzeBlock(block, stashMethod, stashInvocations);
        }

        var entryBlock = controlFlowGraph.Blocks.First(b => b.Kind == BasicBlockKind.Entry);
        RecurseBlocks(entryBlock, stashInvocations, 0);
    }

    private static void AnalyzeBlock(BasicBlock block, IMethodSymbol stashMethod, Dictionary<BasicBlock, int> stashInvocations)
    {
        var stashInvocationCount = 0;
        
        foreach (var operation in block.Descendants())
        {
            switch (operation)
            {
                case IInvocationOperation invocation:
                    if(SymbolEqualityComparer.Default.Equals(invocation.TargetMethod, stashMethod))
                        stashInvocationCount++;
                    break;
                
                case IFlowAnonymousFunctionOperation flow:
                    // TODO: check for flow anonymous lambda function invocation
                    break;
                
                // TODO: check for local function invocation
            }
        }
        
        if(stashInvocationCount > 0)
            stashInvocations.Add(block, stashInvocationCount);
    }

    private static void RecurseBlocks(BasicBlock block, Dictionary<BasicBlock, int> stashInvocations, int totalInvocations)
    {
        if (stashInvocations.TryGetValue(block, out var blockInvocation))
        {
            totalInvocations += blockInvocation;
        }

        if (totalInvocations > 1)
        {
            // TODO: report diagnostic
        }
        
        if(block.ConditionalSuccessor is { Destination: not null })
            RecurseBlocks(block.ConditionalSuccessor.Destination, stashInvocations, totalInvocations);
        
        if(block.FallThroughSuccessor is { Destination: not null })
            RecurseBlocks(block.FallThroughSuccessor.Destination, stashInvocations, totalInvocations);
    }
}

