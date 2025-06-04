// -----------------------------------------------------------------------
//  <copyright file="ActorDslContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor.Dsl;

public interface IIActorDslContext
{
    public ImmutableArray<IMethodSymbol> Receive { get; }
    public ImmutableArray<IMethodSymbol> ReceiveAsync { get; }
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync { get; }
}

public sealed class EmptyIActorDslContext : IIActorDslContext
{
    public static readonly EmptyIActorDslContext Instance = new();
    
    private EmptyIActorDslContext() { }
    
    public ImmutableArray<IMethodSymbol> Receive => new();
    public ImmutableArray<IMethodSymbol> ReceiveAsync => new();
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync => new();
}

public sealed class IActorDslContext : IIActorDslContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceive;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceiveAsync;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceiveAnyAsync;

    private IActorDslContext(DslContext context)
    {
        _lazyReceive = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.IActorDslType!
            .GetMembers("Receive").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyReceiveAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.IActorDslType!
            .GetMembers("ReceiveAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyReceiveAnyAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.IActorDslType!
            .GetMembers("ReceiveAnyAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> Receive => _lazyReceive.Value;
    public ImmutableArray<IMethodSymbol> ReceiveAsync => _lazyReceiveAsync.Value;
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync => _lazyReceiveAnyAsync.Value;

    public static IActorDslContext Get(DslContext context)
        => new(context);
}