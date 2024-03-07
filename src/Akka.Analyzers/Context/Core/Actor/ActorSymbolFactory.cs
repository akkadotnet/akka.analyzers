// -----------------------------------------------------------------------
//  <copyright file="TypeSymbolFactory.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core;

public static class ActorSymbolFactory
{
    public const string AkkaActorNamespace = AkkaCoreContext.AkkaNamespace + ".Actor";
    
    public static INamedTypeSymbol? ActorBase(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{AkkaActorNamespace}.ActorBase");

    public static INamedTypeSymbol? ActorReference(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{AkkaActorNamespace}.IActorRef");

    public static INamedTypeSymbol? Props(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{AkkaActorNamespace}.Props");

    public static INamedTypeSymbol? ActorContext(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{AkkaActorNamespace}.IActorContext");

    public static INamedTypeSymbol? IndirectActorProducer(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{AkkaActorNamespace}.IIndirectActorProducer");
    
    public static INamedTypeSymbol? ReceiveActor(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{AkkaActorNamespace}.ReceiveActor");
    
    public static INamedTypeSymbol? GracefulStopSupport(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{AkkaActorNamespace}.GracefulStopSupport");
    
    public static INamedTypeSymbol? TellSchedulerInterface(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.ITellScheduler");
}