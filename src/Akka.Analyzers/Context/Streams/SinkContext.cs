// -----------------------------------------------------------------------
//  <copyright file="SinkContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Streams;

public sealed class EmptySinkContext : ISinkContext
{
    public static readonly EmptySinkContext Instance = new();
    
    private EmptySinkContext() { }
    
    public ImmutableArray<IMethodSymbol> Aggregate => new();
    public ImmutableArray<IMethodSymbol> AggregateAsync => new();
}

public sealed class SinkContext : ISinkContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyAggregate;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyAggregateAsync;

    private SinkContext(AkkaStreamsContext context)
    {
        _lazyAggregate = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.SinkType!
            .GetMembers("Aggregate").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyAggregateAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.SinkType!
            .GetMembers("AggregateAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> Aggregate => _lazyAggregate.Value;
    public ImmutableArray<IMethodSymbol> AggregateAsync => _lazyAggregateAsync.Value;

    public static SinkContext Get(AkkaStreamsContext context)
        => new(context);
} 