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
    public ImmutableArray<IMethodSymbol> ReceiveAsync { get; }
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync { get; }
}

public sealed class EmptyReceiveActorContext : IReceiveActorContext
{
    public static readonly EmptyReceiveActorContext Instance = new();
    
    private EmptyReceiveActorContext() { }
    
    public ImmutableArray<IMethodSymbol> ReceiveAsync => new();
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync => new();
}

public sealed class ReceiveActorContext : IReceiveActorContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceiveAsync;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceiveAnyAsync;

    private ReceiveActorContext(AkkaCoreActorContext context)
    {
        _lazyReceiveAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ReceiveActorType!
            .GetMembers("ReceiveAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyReceiveAnyAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ReceiveActorType!
            .GetMembers("ReceiveAnyAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> ReceiveAsync => _lazyReceiveAsync.Value;
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync => _lazyReceiveAnyAsync.Value;

    public static ReceiveActorContext Get(AkkaCoreActorContext context)
        => new(context);
}