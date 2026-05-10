// -----------------------------------------------------------------------
//  <copyright file="UntypedActorContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IUntypedActorContext
{
    public ImmutableArray<IMethodSymbol> RunTask { get; }
}

public sealed class EmptyUntypedActorContext : IUntypedActorContext
{
    public static readonly EmptyUntypedActorContext Instance = new();

    private EmptyUntypedActorContext() { }

    public ImmutableArray<IMethodSymbol> RunTask => ImmutableArray<IMethodSymbol>.Empty;
}

public sealed class UntypedActorContext : IUntypedActorContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyRunTask;

    private UntypedActorContext(AkkaCoreActorContext context)
    {
        _lazyRunTask = new Lazy<ImmutableArray<IMethodSymbol>>(() =>
            context.UntypedActorType is null
                ? ImmutableArray<IMethodSymbol>.Empty
                : context.UntypedActorType.GetMembers("RunTask")
                    .OfType<IMethodSymbol>().ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> RunTask => _lazyRunTask.Value;

    public static UntypedActorContext Get(AkkaCoreActorContext context)
        => new(context);
}
