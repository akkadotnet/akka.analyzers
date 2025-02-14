// -----------------------------------------------------------------------
//  <copyright file="ActorRefFactoryExtensionsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IActorRefFactoryExtensionsContext
{
    public IMethodSymbol? ActorOf { get; }
    public IMethodSymbol? ActorSelection { get; }
}

public sealed class EmptyActorRefFactoryExtensionsContext : IActorRefFactoryExtensionsContext
{
    public static EmptyActorRefFactoryExtensionsContext Instance { get; } = new ();
    
    private EmptyActorRefFactoryExtensionsContext() { }

    public IMethodSymbol? ActorOf => null;
    public IMethodSymbol? ActorSelection => null;
}

public sealed class ActorRefFactoryExtensionsContext : IActorRefFactoryExtensionsContext
{
    private readonly Lazy<IMethodSymbol> _lazyActorOf;
    private readonly Lazy<IMethodSymbol> _lazyActorSelection;

    private ActorRefFactoryExtensionsContext(IAkkaCoreActorContext context)
    {
        _lazyActorOf = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorRefFactoryExtensionsType!
            .GetMembers(nameof(ActorOf)).First());
        _lazyActorSelection = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorRefFactoryExtensionsType!
            .GetMembers(nameof(ActorSelection)).First());
    }
    
    public IMethodSymbol? ActorOf => _lazyActorOf.Value;
    public IMethodSymbol? ActorSelection => _lazyActorSelection.Value;
    
    public static ActorRefFactoryExtensionsContext Get(IAkkaCoreActorContext context)
        => new(context);
}
