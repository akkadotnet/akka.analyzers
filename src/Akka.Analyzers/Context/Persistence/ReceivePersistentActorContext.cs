// -----------------------------------------------------------------------
//  <copyright file="ReceivePersistentActorContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Persistence;

public interface IReceivePersistentActorContext
{
    public ImmutableArray<IMethodSymbol> CommandAsync { get; }
    public IMethodSymbol? CommandAnyAsync { get; }
    public ImmutableArray<IMethodSymbol> Command { get; }
    public IMethodSymbol? CommandAny { get; }
    public ImmutableArray<IMethodSymbol> Recover { get; }
    public IMethodSymbol? RecoverAny { get; }
}

public class EmptyReceivePersistentActorContext: IReceivePersistentActorContext
{
    public static readonly EmptyReceivePersistentActorContext Instance = new();

    public ImmutableArray<IMethodSymbol> CommandAsync => ImmutableArray<IMethodSymbol>.Empty;
    public IMethodSymbol? CommandAnyAsync => null;
    public ImmutableArray<IMethodSymbol> Command => ImmutableArray<IMethodSymbol>.Empty;
    public IMethodSymbol? CommandAny => null;
    public ImmutableArray<IMethodSymbol> Recover => ImmutableArray<IMethodSymbol>.Empty;
    public IMethodSymbol? RecoverAny => null;
}

public class ReceivePersistentActorContext: IReceivePersistentActorContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyCommandAsync;
    private readonly Lazy<IMethodSymbol?> _lazyCommandAnyAsync;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyCommand;
    private readonly Lazy<IMethodSymbol?> _lazyCommandAny;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyRecover;
    private readonly Lazy<IMethodSymbol?> _lazyRecoverAny;

    private ReceivePersistentActorContext(AkkaPersistenceContext context)
    {
        _lazyCommandAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ReceivePersistentActorType!
            .GetMembers("CommandAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyCommandAnyAsync = new Lazy<IMethodSymbol?>(() => context.ReceivePersistentActorType!
            .GetMembers("CommandAnyAsync").OfType<IMethodSymbol>().FirstOrDefault());
        _lazyCommand = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ReceivePersistentActorType!
            .GetMembers("Command").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyCommandAny = new Lazy<IMethodSymbol?>(() => context.ReceivePersistentActorType!
            .GetMembers("CommandAny").OfType<IMethodSymbol>().FirstOrDefault());
        _lazyRecover = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ReceivePersistentActorType!
            .GetMembers("Recover").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyRecoverAny = new Lazy<IMethodSymbol?>(() => context.ReceivePersistentActorType!
            .GetMembers("RecoverAny").OfType<IMethodSymbol>().FirstOrDefault());
    }

    public ImmutableArray<IMethodSymbol> CommandAsync => _lazyCommandAsync.Value;
    public IMethodSymbol? CommandAnyAsync => _lazyCommandAnyAsync.Value;
    public ImmutableArray<IMethodSymbol> Command => _lazyCommand.Value;
    public IMethodSymbol? CommandAny => _lazyCommandAny.Value;
    public ImmutableArray<IMethodSymbol> Recover => _lazyRecover.Value;
    public IMethodSymbol? RecoverAny => _lazyRecoverAny.Value;

    public static ReceivePersistentActorContext Get(AkkaPersistenceContext context)
        => new(context);
}