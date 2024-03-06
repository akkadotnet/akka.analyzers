// -----------------------------------------------------------------------
//  <copyright file="IAkkaClusterContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Cluster;

public interface IAkkaClusterContext
{
    Version Version { get; }
    
    INamedTypeSymbol? ClusterType { get; }
}
