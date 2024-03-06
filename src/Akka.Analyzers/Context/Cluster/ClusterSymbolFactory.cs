// -----------------------------------------------------------------------
//  <copyright file="TypeSymbolFactor.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context.Core;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Cluster;

public static class ClusterSymbolFactory
{
    public const string ClusterNamespace = AkkaCoreContext.AkkaNamespace + ".Cluster";
    
    public static INamedTypeSymbol? AkkaCluster(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{ClusterNamespace}.Cluster");
}