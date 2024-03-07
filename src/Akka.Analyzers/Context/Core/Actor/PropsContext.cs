// -----------------------------------------------------------------------
//  <copyright file="PropsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IPropsContext
{
    public ImmutableArray<IMethodSymbol> Create { get; }
}

public sealed class EmptyPropsContext : IPropsContext
{
    public static readonly EmptyPropsContext Instance = new();
    private EmptyPropsContext() { }
    public ImmutableArray<IMethodSymbol> Create => new();
}

public sealed class PropsContext : IPropsContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyCreate;

    private PropsContext(AkkaCoreActorContext context)
    {
        _lazyCreate = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.PropsType!
            .GetMembers("Create").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> Create => _lazyCreate.Value;

    public static PropsContext Get(AkkaCoreActorContext context)
        => new(context);
}