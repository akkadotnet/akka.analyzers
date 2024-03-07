// -----------------------------------------------------------------------
//  <copyright file="AkkaCoreActorContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public sealed class EmptyAkkaCoreActorContext : IAkkaCoreActorContext
{
    private EmptyAkkaCoreActorContext() { }
    public static EmptyAkkaCoreActorContext Instance { get; } = new();
    public INamedTypeSymbol? ActorBaseType => null;
    public INamedTypeSymbol? IActorRefType => null;
    public INamedTypeSymbol? PropsType => null;
    public INamedTypeSymbol? IActorContextType => null;
    public INamedTypeSymbol? IIndirectActorProducerType => null;
    public INamedTypeSymbol? ReceiveActorType => null;
    public INamedTypeSymbol? GracefulStopSupportType => null;
    public INamedTypeSymbol? ITellSchedulerType => null;
    public ITellSchedulerInterfaceContext ITellScheduler => EmptyTellSchedulerInterfaceContext.Instance;
}

public sealed class AkkaCoreActorContext : IAkkaCoreActorContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyActorBaseType;
    private readonly Lazy<INamedTypeSymbol?> _lazyActorRefType;
    private readonly Lazy<INamedTypeSymbol?> _lazyPropsType;
    private readonly Lazy<INamedTypeSymbol?> _lazyActorContextType;
    private readonly Lazy<INamedTypeSymbol?> _lazyIIndirectActorProducerType;
    private readonly Lazy<INamedTypeSymbol?> _lazyReceiveActorType;
    private readonly Lazy<INamedTypeSymbol?> _lazyGracefulStopSupport;
    private readonly Lazy<INamedTypeSymbol?> _lazyTellSchedulerInterface;

    private AkkaCoreActorContext(Compilation compilation)
    {
        _lazyActorBaseType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.ActorBase(compilation));
        _lazyActorRefType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.ActorReference(compilation));
        _lazyPropsType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.Props(compilation));
        _lazyActorContextType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.ActorContext(compilation));
        _lazyIIndirectActorProducerType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.IndirectActorProducer(compilation));
        _lazyReceiveActorType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.ReceiveActor(compilation));
        _lazyGracefulStopSupport = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.GracefulStopSupport(compilation));
        _lazyTellSchedulerInterface = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.TellSchedulerInterface(compilation));
        ITellScheduler = TellSchedulerInterfaceContext.Get(compilation);
    }

    public INamedTypeSymbol? ActorBaseType => _lazyActorBaseType.Value;
    public INamedTypeSymbol? IActorRefType => _lazyActorRefType.Value;
    public INamedTypeSymbol? PropsType => _lazyPropsType.Value;
    public INamedTypeSymbol? IActorContextType => _lazyActorContextType.Value;
    public INamedTypeSymbol? IIndirectActorProducerType => _lazyIIndirectActorProducerType.Value;
    public INamedTypeSymbol? ReceiveActorType => _lazyReceiveActorType.Value;
    public INamedTypeSymbol? GracefulStopSupportType => _lazyGracefulStopSupport.Value;
    public INamedTypeSymbol? ITellSchedulerType => _lazyTellSchedulerInterface.Value;
    public ITellSchedulerInterfaceContext ITellScheduler { get; }

    public static IAkkaCoreActorContext Get(Compilation compilation)
        => new AkkaCoreActorContext(compilation);
}