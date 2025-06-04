using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor.Dsl;

public interface IActContext
{
    public ImmutableArray<IMethodSymbol> Receive { get; }
    public ImmutableArray<IMethodSymbol> ReceiveAsync { get; }
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync { get; }
}

public sealed class EmptyActContext : IActContext
{
    public static readonly EmptyActContext Instance = new();
    
    private EmptyActContext() { }
    
    public ImmutableArray<IMethodSymbol> Receive => new();
    public ImmutableArray<IMethodSymbol> ReceiveAsync => new();
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync => new();
}

public sealed class ActContext : IActContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceive;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceiveAsync;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyReceiveAnyAsync;

    private ActContext(DslContext context)
    {
        _lazyReceive = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ActType!
            .GetMembers("Receive").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyReceiveAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ActType!
            .GetMembers("ReceiveAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyReceiveAnyAsync = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ActType!
            .GetMembers("ReceiveAnyAsync").Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> Receive => _lazyReceive.Value;
    public ImmutableArray<IMethodSymbol> ReceiveAsync => _lazyReceiveAsync.Value;
    public ImmutableArray<IMethodSymbol> ReceiveAnyAsync => _lazyReceiveAnyAsync.Value;

    public static ActContext Get(DslContext context)
        => new(context);
}