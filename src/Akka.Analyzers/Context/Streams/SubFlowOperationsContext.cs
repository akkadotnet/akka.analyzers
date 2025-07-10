// -----------------------------------------------------------------------
//  <copyright file="SubFlowOperationsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Streams;

public sealed class EmptySubFlowOperationsContext : ISubFlowOperationsContext
{
    public static readonly EmptySubFlowOperationsContext Instance = new();
    
    private EmptySubFlowOperationsContext() { }
    
    public ImmutableArray<IMethodSymbol> Aggregate => new();
    public ImmutableArray<IMethodSymbol> AggregateAsync => new();
}

public sealed class SubFlowOperationsContext : ISubFlowOperationsContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyAggregate;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyAggregateAsync;

    private SubFlowOperationsContext(AkkaStreamsContext context)
    {
        _lazyAggregate = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.SubFlowOperationsType!
            .GetMembers("Aggregate").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyAggregateAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.SubFlowOperationsType!
            .GetMembers("AggregateAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> Aggregate => _lazyAggregate.Value;
    public ImmutableArray<IMethodSymbol> AggregateAsync => _lazyAggregateAsync.Value;

    public static SubFlowOperationsContext Get(AkkaStreamsContext context)
        => new(context);
} 