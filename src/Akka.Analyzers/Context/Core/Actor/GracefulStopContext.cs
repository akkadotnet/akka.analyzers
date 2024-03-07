// -----------------------------------------------------------------------
//  <copyright file="GracefulStopContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IGracefulStopSupportContext
{
    public ImmutableArray<IMethodSymbol> GracefulStop { get; }
}

public sealed class EmptyGracefulStopSupportContext : IGracefulStopSupportContext
{
    public static readonly EmptyGracefulStopSupportContext Instance = new();
    
    private EmptyGracefulStopSupportContext() { }
    
    public ImmutableArray<IMethodSymbol> GracefulStop => new();
}

public class GracefulStopSupportContext: IGracefulStopSupportContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyGracefulStop;
    
    private GracefulStopSupportContext(AkkaCoreActorContext context)
    {
        _lazyGracefulStop = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.GracefulStopSupportType!
                .GetMembers("GracefulStop").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> GracefulStop => _lazyGracefulStop.Value;

    public static GracefulStopSupportContext Get(AkkaCoreActorContext context)
        => new(context);
}