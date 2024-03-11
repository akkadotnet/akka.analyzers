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
    public IPropertySymbol? Self { get; }
}

public sealed class EmptyActorBaseContext : IActorBaseContext
{
    public static readonly EmptyActorBaseContext Instance = new();
    private EmptyActorBaseContext() { }
    public IPropertySymbol? Self => null;
}

public sealed class ActorBaseContext : IActorBaseContext
{
    private readonly Lazy<IPropertySymbol> _lazySelf;
    
    private ActorBaseContext(AkkaCoreActorContext context)
    {
        _lazySelf = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorBaseType!.GetMembers("Self").First());
    }

    public IPropertySymbol? Self => _lazySelf.Value;

    public static ActorBaseContext Get(AkkaCoreActorContext context)
        => new(context);
}