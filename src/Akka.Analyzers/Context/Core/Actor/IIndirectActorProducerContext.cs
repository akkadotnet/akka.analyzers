// -----------------------------------------------------------------------
//  <copyright file="IIndirectActorProducerContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IIndirectActorProducerContext
{
    public ImmutableArray<IMethodSymbol> Produce { get; }
}

public sealed class EmptyIndirectActorProducerContext: IIndirectActorProducerContext
{
    public static readonly EmptyIndirectActorProducerContext Instance = new();
    public ImmutableArray<IMethodSymbol> Produce => new();
    private EmptyIndirectActorProducerContext() { }
}

public sealed class IndirectActorProducerContext : IIndirectActorProducerContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyProduce;
    
    private IndirectActorProducerContext(AkkaCoreActorContext context)
    {
        _lazyProduce = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.IIndirectActorProducerType!
            .GetMembers("Produce").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> Produce => _lazyProduce.Value;

    public static IndirectActorProducerContext Get(AkkaCoreActorContext context)
        => new (context);
}