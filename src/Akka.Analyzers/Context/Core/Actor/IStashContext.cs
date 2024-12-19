// -----------------------------------------------------------------------
//  <copyright file="IStashContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Akka.Analyzers.Context.Core.Actor;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Core.Actor;

// ReSharper disable once InconsistentNaming
public interface IStashContext
{
    public IMethodSymbol? Stash { get; }
}

public sealed class EmptyStashContext : IStashContext
{
    public static readonly EmptyStashContext Instance = new();
    private EmptyStashContext() { }
    public IMethodSymbol? Stash => null;
}

public sealed class StashContext : IStashContext
{
    private readonly Lazy<IMethodSymbol> _lazyStash;
    
    public IMethodSymbol Stash => _lazyStash.Value;
    
    private StashContext(AkkaCoreActorContext context)
    {
        Guard.AssertIsNotNull(context);
        _lazyStash = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.IStashType!
            .GetMembers(nameof(Stash)).First());
    }
    
    public static StashContext Get(AkkaCoreActorContext context)
    {
        Guard.AssertIsNotNull(context);
        return new StashContext(context);
    }
}