// -----------------------------------------------------------------------
//  <copyright file="IAkkaCoreActorContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Core.Actor;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

// ReSharper disable InconsistentNaming
public interface IAkkaCoreActorContext
{
    public INamedTypeSymbol? ActorBaseType { get; }
    public INamedTypeSymbol? IActorRefType { get; }
    public INamedTypeSymbol? PropsType { get; }
    public INamedTypeSymbol? IActorContextType { get; }
    public INamedTypeSymbol? IIndirectActorProducerType { get; }
    public INamedTypeSymbol? ReceiveActorType { get; }
    public INamedTypeSymbol? GracefulStopSupportType { get; }
    public INamedTypeSymbol? ITellSchedulerType { get; }
    public INamedTypeSymbol? ActorRefsType { get; }
    public INamedTypeSymbol? ITimerSchedulerType { get; } 
    
    public IGracefulStopSupportContext GracefulStopSupportSupport { get; }
    public IIndirectActorProducerContext IIndirectActorProducer { get; }
    public IReceiveActorContext ReceiveActor { get; }
    public IActorBaseContext ActorBase { get; }
    public IActorContextContext IActorContext { get; }
    public IPropsContext Props { get; }
    public ITellSchedulerInterfaceContext ITellScheduler { get; }
    public IActorRefsContext ActorRefs { get; }
    public ITimerSchedulerContext ITimerScheduler { get; }
}
