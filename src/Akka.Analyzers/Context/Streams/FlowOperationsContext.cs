// -----------------------------------------------------------------------
//  <copyright file="FlowOperationsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Streams;

public sealed class EmptyFlowOperationsContext : IFlowOperationsContext
{
    public static readonly EmptyFlowOperationsContext Instance = new();
    
    private EmptyFlowOperationsContext() { }
    
    public ImmutableArray<IMethodSymbol> Aggregate => new();
    public ImmutableArray<IMethodSymbol> AggregateAsync => new();
}

public sealed class FlowOperationsContext : IFlowOperationsContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyAggregate;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyAggregateAsync;

    private FlowOperationsContext(AkkaStreamsContext context)
    {
        _lazyAggregate = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.FlowOperationsType!
            .GetMembers("Aggregate").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyAggregateAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.FlowOperationsType!
            .GetMembers("AggregateAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> Aggregate => _lazyAggregate.Value;
    public ImmutableArray<IMethodSymbol> AggregateAsync => _lazyAggregateAsync.Value;

    public static FlowOperationsContext Get(AkkaStreamsContext context)
        => new(context);
} 