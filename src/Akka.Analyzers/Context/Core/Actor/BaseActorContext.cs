// -----------------------------------------------------------------------
//  <copyright file="BaseActorContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IActorBaseContext
{
    #region Properties

    public IPropertySymbol? Self { get; }

    #endregion

    #region Methods

    public IMethodSymbol? AroundPreRestart { get; }
    public IMethodSymbol? AroundPreStart { get; }
    public IMethodSymbol? PreStart { get; }
    public IMethodSymbol? AroundPostRestart { get; }
    public IMethodSymbol? PreRestart { get; }
    public IMethodSymbol? PostRestart { get; }
    public IMethodSymbol? AroundPostStop { get; }
    public IMethodSymbol? PostStop { get; }

    #endregion
}

public sealed class EmptyActorBaseContext : IActorBaseContext
{
    public static readonly EmptyActorBaseContext Instance = new();
    private EmptyActorBaseContext() { }
    public IPropertySymbol? Self => null;
    public IMethodSymbol? AroundPreRestart => null;
    public IMethodSymbol? AroundPreStart => null;
    public IMethodSymbol? PreStart => null;
    public IMethodSymbol? AroundPostRestart => null;
    public IMethodSymbol? PreRestart => null;
    public IMethodSymbol? PostRestart => null;
    public IMethodSymbol? AroundPostStop => null;
    public IMethodSymbol? PostStop => null;
}

public sealed class ActorBaseContext : IActorBaseContext
{
    private readonly Lazy<IPropertySymbol> _lazySelf;
    private readonly Lazy<IMethodSymbol> _lazyAroundPreRestart;
    private readonly Lazy<IMethodSymbol> _lazyAroundPreStart;
    private readonly Lazy<IMethodSymbol> _lazyPreStart;
    private readonly Lazy<IMethodSymbol> _lazyAroundPostRestart;
    private readonly Lazy<IMethodSymbol> _lazyPreRestart;
    private readonly Lazy<IMethodSymbol> _lazyPostRestart;
    private readonly Lazy<IMethodSymbol> _lazyAroundPostStop;
    private readonly Lazy<IMethodSymbol> _lazyPostStop;
    
    private ActorBaseContext(AkkaCoreActorContext context)
    {
        Guard.AssertIsNotNull(context.ActorBaseType);
        
        _lazySelf = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorBaseType!.GetMembers("Self").First());

        _lazyAroundPreRestart = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorBaseType!
            .GetMembers(nameof(AroundPreRestart)).First());
        _lazyAroundPreStart = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorBaseType!
            .GetMembers(nameof(AroundPreStart)).First());
        _lazyPreStart = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorBaseType!
            .GetMembers(nameof(PreStart)).First());
        _lazyAroundPostRestart = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorBaseType!
            .GetMembers(nameof(AroundPostRestart)).First());
        _lazyPreRestart = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorBaseType!
            .GetMembers(nameof(PreRestart)).First());
        _lazyPostRestart = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorBaseType!
            .GetMembers(nameof(PostRestart)).First());
        _lazyAroundPostStop = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorBaseType!
            .GetMembers(nameof(AroundPostStop)).First());
        _lazyPostStop = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorBaseType!
            .GetMembers(nameof(PostStop)).First());
    }

    public IPropertySymbol? Self => _lazySelf.Value;
    public IMethodSymbol? AroundPreRestart => _lazyAroundPreRestart.Value;
    public IMethodSymbol? AroundPreStart => _lazyAroundPreStart.Value;
    public IMethodSymbol? PreStart => _lazyPreStart.Value;
    public IMethodSymbol? AroundPostRestart => _lazyAroundPostRestart.Value;
    public IMethodSymbol? PreRestart => _lazyPreRestart.Value;
    public IMethodSymbol? PostRestart => _lazyPostRestart.Value;
    public IMethodSymbol? AroundPostStop => _lazyAroundPostStop.Value;
    public IMethodSymbol? PostStop => _lazyPostStop.Value;

    public static ActorBaseContext Get(AkkaCoreActorContext context)
    {
        Guard.AssertIsNotNull(context);
        
        return new ActorBaseContext(context);
    }
}