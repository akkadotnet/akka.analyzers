// -----------------------------------------------------------------------
//  <copyright file="IAkkaClusterToolsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.ClusterTools;

public interface IAkkaClusterToolsContext
{
    Version Version { get; }
    INamedTypeSymbol? SingletonManagerType { get; }
    INamedTypeSymbol? SingletonProxyType { get; }
    INamedTypeSymbol? ClusterClientType { get; }
}