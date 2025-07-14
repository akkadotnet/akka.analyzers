// -----------------------------------------------------------------------
//  <copyright file="SourceContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Streams;

public sealed class EmptySourceContext : ISourceContext
{
    public static readonly EmptySourceContext Instance = new();
    
    private EmptySourceContext() { }
    
    public ImmutableArray<IMethodSymbol> RunAggregate => new();
    public ImmutableArray<IMethodSymbol> RunAggregateAsync => new();
}

public sealed class SourceContext : ISourceContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyRunAggregate;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyRunAggregateAsync;

    private SourceContext(AkkaStreamsContext context)
    {
        _lazyRunAggregate = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.SourceType!
            .GetMembers("RunAggregate").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyRunAggregateAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.SourceType!
            .GetMembers("RunAggregateAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> RunAggregate => _lazyRunAggregate.Value;
    public ImmutableArray<IMethodSymbol> RunAggregateAsync => _lazyRunAggregateAsync.Value;

    public static SourceContext Get(AkkaStreamsContext context)
        => new(context);
} 