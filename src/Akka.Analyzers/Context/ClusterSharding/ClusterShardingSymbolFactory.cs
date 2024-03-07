// -----------------------------------------------------------------------
//  <copyright file="TypeSymbolFactory.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context.Cluster;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.ClusterSharding;

public static class ClusterShardingSymbolFactory
{
    public const string ClusterShardingNamespace = ClusterSymbolFactory.ClusterNamespace + ".Sharding";
    
    public static INamedTypeSymbol? AkkaClusterSharding(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{ClusterShardingNamespace}.ClusterSharding");
    }
    
    public static INamedTypeSymbol? AkkaMessageExtractorInterface(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{ClusterShardingNamespace}.IMessageExtractor");
    }

    public static INamedTypeSymbol? AkkaShardEnvelope(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{ClusterShardingNamespace}.ShardingEnvelope");
    }

    public static INamedTypeSymbol? AkkaStartEntity(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{ClusterShardingNamespace}.ShardRegion+StartEntity");
    }
}