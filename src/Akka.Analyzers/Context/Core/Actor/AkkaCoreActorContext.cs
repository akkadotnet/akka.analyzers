// -----------------------------------------------------------------------
//  <copyright file="AkkaCoreActorContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Core.Actor;
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
    public INamedTypeSymbol? ActorRefsType => null;

    public IGracefulStopSupportContext GracefulStopSupportSupport => EmptyGracefulStopSupportContext.Instance;
    public IIndirectActorProducerContext IIndirectActorProducer => EmptyIndirectActorProducerContext.Instance;
    public IReceiveActorContext ReceiveActor => EmptyReceiveActorContext.Instance;
    public IActorBaseContext ActorBase => EmptyActorBaseContext.Instance;
    public IActorContextContext IActorContext => EmptyActorContextContext.Instance;
    public IPropsContext Props => EmptyPropsContext.Instance;
    public ITellSchedulerInterfaceContext ITellScheduler => EmptyTellSchedulerInterfaceContext.Instance;
    public IActorRefsContext ActorRefs => EmptyActorRefsContext.Empty;
}

public sealed class AkkaCoreActorContext : IAkkaCoreActorContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyActorBaseType;
    private readonly Lazy<INamedTypeSymbol?> _lazyActorRefType;
    private readonly Lazy<INamedTypeSymbol?> _lazyPropsType;
    private readonly Lazy<INamedTypeSymbol?> _lazyActorContextType;
    private readonly Lazy<INamedTypeSymbol?> _lazyIIndirectActorProducerType;
    private readonly Lazy<INamedTypeSymbol?> _lazyReceiveActorType;
    private readonly Lazy<INamedTypeSymbol?> _lazyGracefulStopSupportType;
    private readonly Lazy<INamedTypeSymbol?> _lazyTellSchedulerInterface;
    private readonly Lazy<INamedTypeSymbol?> _lazyActorRefsType;
    private readonly Lazy<IGracefulStopSupportContext> _lazyGracefulStopSupport;
    private readonly Lazy<IIndirectActorProducerContext> _lazyIIndirectActorProducer;
    private readonly Lazy<IReceiveActorContext> _lazyReceiveActor;
    private readonly Lazy<IActorBaseContext> _lazyActorBase;
    private readonly Lazy<IActorContextContext> _lazyActorContext;
    private readonly Lazy<IPropsContext> _lazyProps;

    private AkkaCoreActorContext(Compilation compilation)
    {
        _lazyActorBaseType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.ActorBase(compilation));
        _lazyActorRefType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.ActorReference(compilation));
        _lazyPropsType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.Props(compilation));
        _lazyActorContextType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.ActorContext(compilation));
        _lazyIIndirectActorProducerType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.IndirectActorProducer(compilation));
        _lazyReceiveActorType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.ReceiveActor(compilation));
        _lazyGracefulStopSupportType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.GracefulStopSupport(compilation));
        _lazyTellSchedulerInterface = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.TellSchedulerInterface(compilation));
        _lazyActorRefsType = new Lazy<INamedTypeSymbol?>(() => ActorSymbolFactory.ActorRefs(compilation));
        _lazyGracefulStopSupport = new Lazy<IGracefulStopSupportContext>(() => GracefulStopSupportContext.Get(this));
        _lazyIIndirectActorProducer = new Lazy<IIndirectActorProducerContext>(() => IndirectActorProducerContext.Get(this));
        _lazyReceiveActor = new Lazy<IReceiveActorContext>(() => ReceiveActorContext.Get(this));
        _lazyActorBase = new Lazy<IActorBaseContext>(() => ActorBaseContext.Get(this));
        _lazyActorContext = new Lazy<IActorContextContext>(() => ActorContextContext.Get(this));
        _lazyProps = new Lazy<IPropsContext>(() => PropsContext.Get(this));
        ITellScheduler = TellSchedulerInterfaceContext.Get(compilation);
        ActorRefs = ActorRefsContext.Get(this);
    }

    public INamedTypeSymbol? ActorBaseType => _lazyActorBaseType.Value;
    public INamedTypeSymbol? IActorRefType => _lazyActorRefType.Value;
    public INamedTypeSymbol? PropsType => _lazyPropsType.Value;
    public INamedTypeSymbol? IActorContextType => _lazyActorContextType.Value;
    public INamedTypeSymbol? IIndirectActorProducerType => _lazyIIndirectActorProducerType.Value;
    public INamedTypeSymbol? ReceiveActorType => _lazyReceiveActorType.Value;
    public INamedTypeSymbol? ITellSchedulerType => _lazyTellSchedulerInterface.Value;
    public INamedTypeSymbol? ActorRefsType => _lazyActorRefsType.Value;
    public INamedTypeSymbol? GracefulStopSupportType => _lazyGracefulStopSupportType.Value;
    public IGracefulStopSupportContext GracefulStopSupportSupport => _lazyGracefulStopSupport.Value;
    public IIndirectActorProducerContext IIndirectActorProducer => _lazyIIndirectActorProducer.Value;
    public IReceiveActorContext ReceiveActor => _lazyReceiveActor.Value;
    public IActorBaseContext ActorBase => _lazyActorBase.Value;
    public IActorContextContext IActorContext => _lazyActorContext.Value;
    public IPropsContext Props => _lazyProps.Value;
    public ITellSchedulerInterfaceContext ITellScheduler { get; }
    public IActorRefsContext ActorRefs { get; }

    public static IAkkaCoreActorContext Get(Compilation compilation)
        => new AkkaCoreActorContext(compilation);
}