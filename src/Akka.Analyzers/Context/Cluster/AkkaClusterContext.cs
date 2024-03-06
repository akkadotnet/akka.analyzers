// -----------------------------------------------------------------------
//  &lt;copyright file="AkkaClusterContext.cs" company="Akka.NET Project"&gt;&#xD;
//      Copyright (C) 2009-2024 Lightbend Inc. &lt;http://www.lightbend.com&gt;&#xD;
//      Copyright (C) 2013-2024 .NET Foundation &lt;https://github.com/akkadotnet/akka.net&gt;&#xD;
//  &lt;/copyright&gt;&#xD;
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Cluster;

public sealed class EmptyClusterContext : IAkkaClusterContext
{
    private EmptyClusterContext()
    {
    }

    public static EmptyClusterContext Instance { get; } = new();

    public Version Version { get; } = new();
    public INamedTypeSymbol? ClusterType => null;
}

/// <summary>
/// Default AkkaClusterContext.
/// </summary>
/// <remarks>
/// Used to indicate whether or not Akka.Cluster is present inside the solution being scanned and
/// provides access to some of the built-in type symbols that are used in analysis rules.
/// </remarks>
public sealed class AkkaClusterContext : IAkkaClusterContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyClusterType;
    
    private AkkaClusterContext(Compilation compilation, Version version)
    {
        Version = version;
        _lazyClusterType = new Lazy<INamedTypeSymbol?>(() => ClusterSymbolFactory.AkkaCluster(compilation));
    }
    
    public static IAkkaClusterContext Get(Compilation compilation, Version? versionOverride = null)
    {
        // assert that compilation is not null
        Guard.AssertIsNotNull(compilation);

        var version =
            versionOverride ??
            compilation
                .ReferencedAssemblyNames
                .FirstOrDefault(a => a.Name.Equals("Akka.Cluster", StringComparison.OrdinalIgnoreCase))
                ?.Version;

        return version is null ? EmptyClusterContext.Instance : new AkkaClusterContext(compilation, version);
    }

    public Version Version { get; }
    public INamedTypeSymbol? ClusterType => _lazyClusterType.Value;
}