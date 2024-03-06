// -----------------------------------------------------------------------
//  <copyright file="IAkkaClusterShardingContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.ClusterSharding;

/// <summary>
/// Data about the Akka.Cluster.Sharding assembly in the solution being analyzed.
/// </summary>
public interface IAkkaClusterShardingContext
{
    Version Version { get; }
    
    INamedTypeSymbol? ClusterShardingType { get; }
    
    INamedTypeSymbol? MessageExtractorInterface { get; }
    
    INamedTypeSymbol? ShardEnvelopeType { get; }
    
    INamedTypeSymbol? StartEntityType { get; }
}
