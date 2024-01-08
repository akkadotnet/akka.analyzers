// -----------------------------------------------------------------------
//  <copyright file="TypeSymbolFactory.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

public static class TypeSymbolFactory
{
    public static INamedTypeSymbol? ActorBase(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.ActorBase");
    }

    public static INamedTypeSymbol? ActorReference(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.IActorRef");
    }

    public static INamedTypeSymbol? Props(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.Props");
    }
    
    public static INamedTypeSymbol? ActorContext(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.IActorContext");
    }
    
    public static INamedTypeSymbol? IndirectActorProducer(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.IIndirectActorProducer");
    }
    
    public static INamedTypeSymbol? AkkaCluster(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Cluster.Cluster");
    }
    
    public static INamedTypeSymbol? AkkaClusterSingletonManager(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Cluster.Tools.Singleton.ClusterSingletonManager");
    }
    
    public static INamedTypeSymbol? AkkaClusterSingletonProxy(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Cluster.Tools.Singleton.ClusterSingletonProxy");
    }
    
    public static INamedTypeSymbol? AkkaClusterClient(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Cluster.Tools.Client.ClusterClient");
    }
    
    public static INamedTypeSymbol? AkkaClusterSharding(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Cluster.Sharding.ClusterSharding");
    }
    
    public static INamedTypeSymbol? AkkaMessageExtractor(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Cluster.Sharding.IMessageExtractor");
    }

    public static INamedTypeSymbol? AkkaShardEnvelope(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Cluster.Sharding.ShardEnvelope");
    }

    public static INamedTypeSymbol? AkkaStartEntity(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Cluster.Sharding.Shard+StartEntity");
    }
}