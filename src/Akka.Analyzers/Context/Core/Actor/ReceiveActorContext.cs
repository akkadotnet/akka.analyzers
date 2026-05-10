// -----------------------------------------------------------------------
//  <copyright file="ReceiveActorContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IReceiveActorContext
{
    public ImmutableArray<IMethodSymbol> Receive { get; }
    public ImmutableArray<IMethodSymbol> ReceiveAsync { get; }
    public ImmutableArray<IMethodSymbol> ReceiveAny { get; }
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync { get; }
}

public sealed class EmptyReceiveActorContext : IReceiveActorContext
{
    public static readonly EmptyReceiveActorContext Instance = new();

    private EmptyReceiveActorContext() { }

    public ImmutableArray<IMethodSymbol> Receive => ImmutableArray<IMethodSymbol>.Empty;
    public ImmutableArray<IMethodSymbol> ReceiveAsync => ImmutableArray<IMethodSymbol>.Empty;
    public ImmutableArray<IMethodSymbol> ReceiveAny => ImmutableArray<IMethodSymbol>.Empty;
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync => ImmutableArray<IMethodSymbol>.Empty;
}

public sealed class ReceiveActorContext : IReceiveActorContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceive;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceiveAsync;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceiveAny;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceiveAnyAsync;

    private ReceiveActorContext(AkkaCoreActorContext context)
    {
        _lazyReceive = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ReceiveActorType!
            .GetMembers("Receive").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyReceiveAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ReceiveActorType!
            .GetMembers("ReceiveAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyReceiveAny = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ReceiveActorType!
            .GetMembers("ReceiveAny").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyReceiveAnyAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ReceiveActorType!
            .GetMembers("ReceiveAnyAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> Receive => _lazyReceive.Value;
    public ImmutableArray<IMethodSymbol> ReceiveAsync => _lazyReceiveAsync.Value;
    public ImmutableArray<IMethodSymbol> ReceiveAny => _lazyReceiveAny.Value;
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync => _lazyReceiveAnyAsync.Value;

    public static ReceiveActorContext Get(AkkaCoreActorContext context)
        => new(context);
}