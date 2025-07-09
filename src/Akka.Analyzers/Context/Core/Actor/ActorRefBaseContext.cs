// -----------------------------------------------------------------------
//  <copyright file="ActorRefBaseContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IActorRefBaseContext
{
    public IPropertySymbol? Path { get; }
    public IMethodSymbol? Tell { get; }
    public IMethodSymbol? TellInternal { get; }
}

public sealed class EmptyActorRefBaseContext : IActorRefBaseContext
{
    public static readonly EmptyActorRefBaseContext Instance = new();
    private EmptyActorRefBaseContext() { }
    public IPropertySymbol? Path => null;
    public IMethodSymbol? Tell => null;
    public IMethodSymbol? TellInternal => null;
}

public sealed class ActorRefBaseContext : IActorRefBaseContext
{
    private readonly Lazy<IPropertySymbol> _lazyPath;
    private readonly Lazy<IMethodSymbol> _lazyTell;
    private readonly Lazy<IMethodSymbol> _lazyTellInternal;

    private ActorRefBaseContext(AkkaCoreActorContext context)
    {
        Guard.AssertIsNotNull(context.ActorRefBaseType);
        _lazyPath = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorRefBaseType!
            .GetMembers("Path").First());
        _lazyTell = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorRefBaseType!
            .GetMembers("Tell").First());
        _lazyTellInternal = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorRefBaseType!
            .GetMembers("TellInternal").First());
    }

    public IPropertySymbol? Path => _lazyPath.Value;
    public IMethodSymbol? Tell => _lazyTell.Value;
    public IMethodSymbol? TellInternal => _lazyTellInternal.Value;

    public static ActorRefBaseContext Get(AkkaCoreActorContext context)
    {
        Guard.AssertIsNotNull(context);
        return new ActorRefBaseContext(context);
    }
} 