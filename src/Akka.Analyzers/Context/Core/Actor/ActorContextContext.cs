// -----------------------------------------------------------------------
//  <copyright file="ActorContextContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context.Core.Actor;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Core.Actor;

public interface IActorContextContext
{
    public IPropertySymbol? Self { get; }
}

public sealed class EmptyActorContextContext : IActorContextContext
{
    public static readonly EmptyActorContextContext Instance = new();
    private EmptyActorContextContext() { }
    
    public IPropertySymbol? Self => null;
}

public sealed class ActorContextContext : IActorContextContext
{
    private readonly Lazy<IPropertySymbol> _lazySelf;

    private ActorContextContext(AkkaCoreActorContext context)
    {
        _lazySelf = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.IActorContextType!.GetMembers("Self").First());
    }

    public IPropertySymbol? Self => _lazySelf.Value;

    public static ActorContextContext Get(AkkaCoreActorContext context)
        => new(context);
}