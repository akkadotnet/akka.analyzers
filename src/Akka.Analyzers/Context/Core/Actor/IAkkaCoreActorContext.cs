// -----------------------------------------------------------------------
//  <copyright file="IAkkaCoreActorContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

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
}
