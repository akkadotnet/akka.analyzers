// -----------------------------------------------------------------------
//  <copyright file="AkkaClusterToolsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.ClusterTools;

public sealed class EmptyAkkaClusterToolsContext : IAkkaClusterToolsContext
{
    private EmptyAkkaClusterToolsContext() { }

    public static EmptyAkkaClusterToolsContext Instance { get; } = new();
    
    public Version Version => new();
    public INamedTypeSymbol? SingletonManagerType => null;
    public INamedTypeSymbol? SingletonProxyType => null;
    public INamedTypeSymbol? ClusterClientType => null;
}

public sealed class AkkaClusterToolsContext: IAkkaClusterToolsContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazySingletonManagerType;
    private readonly Lazy<INamedTypeSymbol?> _lazySingletonProxyType;
    private readonly Lazy<INamedTypeSymbol?> _lazyClusterClientType;
    
    public Version Version { get; }
    public INamedTypeSymbol? SingletonManagerType => _lazySingletonManagerType.Value;
    public INamedTypeSymbol? SingletonProxyType => _lazySingletonProxyType.Value;
    public INamedTypeSymbol? ClusterClientType => _lazyClusterClientType.Value;
    
    private AkkaClusterToolsContext(Compilation compilation, Version version)
    {
        Version = version;
        _lazySingletonManagerType = new Lazy<INamedTypeSymbol?>(() => ClusterToolsSymbolFactory.AkkaClusterSingletonManager(compilation));
        _lazySingletonProxyType = new Lazy<INamedTypeSymbol?>(() => ClusterToolsSymbolFactory.AkkaClusterSingletonProxy(compilation));
        _lazyClusterClientType = new Lazy<INamedTypeSymbol?>(() => ClusterToolsSymbolFactory.AkkaClusterClient(compilation));
    }
    
    public static IAkkaClusterToolsContext Get(Compilation compilation, Version? versionOverride = null)
    {
        // assert that compilation is not null
        Guard.AssertIsNotNull(compilation);

        var version =
            versionOverride ??
            compilation
                .ReferencedAssemblyNames
                .FirstOrDefault(a => a.Name.Equals("Akka.Cluster.Tools", StringComparison.OrdinalIgnoreCase))
                ?.Version;

        return version is null ? EmptyAkkaClusterToolsContext.Instance : new AkkaClusterToolsContext(compilation, version);
    }

}