// -----------------------------------------------------------------------
//  <copyright file="ClusterToolsSymbolFactory.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context.Cluster;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.ClusterTools;

public static class ClusterToolsSymbolFactory
{
    public const string ClusterToolsNamespace = ClusterSymbolFactory.ClusterNamespace + ".Tools";
    public const string ClusterSingletonNamespace = ClusterToolsNamespace + ".Singleton";
    public const string ClusterClientNamespace = ClusterToolsNamespace + ".Client";

    public static INamedTypeSymbol? AkkaClusterSingletonManager(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{ClusterSingletonNamespace}.ClusterSingletonManager");
    }
    
    public static INamedTypeSymbol? AkkaClusterSingletonProxy(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{ClusterSingletonNamespace}.ClusterSingletonProxy");
    }
    
    public static INamedTypeSymbol? AkkaClusterClient(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{ClusterClientNamespace}.ClusterClient");
    }
}