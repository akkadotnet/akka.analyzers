// -----------------------------------------------------------------------
//  <copyright file="IAkkaCoreActorContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IAkkaCoreActorContext
{
    public INamedTypeSymbol? ActorBaseType { get; }
    public INamedTypeSymbol? ActorRefInterface { get; }
    public INamedTypeSymbol? PropsType { get; }
    public INamedTypeSymbol? ActorContextInterface { get; }
    public INamedTypeSymbol? IndirectActorProducerInterface { get; }
    public INamedTypeSymbol? ReceiveActorType { get; }
    public INamedTypeSymbol? GracefulStopSupportType { get; }
}
