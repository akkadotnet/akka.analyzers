// -----------------------------------------------------------------------
//  &lt;copyright file="AkkaClusterShardingContext.cs" company="Akka.NET Project"&gt;&#xD;
//      Copyright (C) 2013-2024 .NET Foundation &lt;https://github.com/akkadotnet/akka.net&gt;&#xD;
//  &lt;/copyright&gt;&#xD;
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.ClusterSharding;

/// <summary>
/// INTERNAL API
/// </summary>
public sealed class EmptyAkkaClusterShardingContext : IAkkaClusterShardingContext
{
    private EmptyAkkaClusterShardingContext()
    {
    }

    public static EmptyAkkaClusterShardingContext Instance { get; } = new();

    public Version Version { get; } = new();
    public INamedTypeSymbol? ClusterShardingType => null;
    public INamedTypeSymbol? MessageExtractorInterface => null;
    public INamedTypeSymbol? ShardEnvelopeType => null;
    public INamedTypeSymbol? StartEntityType => null;
}

/// <summary>
/// INTERNAL API
/// </summary>
public sealed class AkkaClusterShardingContext : IAkkaClusterShardingContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyClusterShardingType;
    private readonly Lazy<INamedTypeSymbol?> _lazyMessageExtractorType;
    private readonly Lazy<INamedTypeSymbol?> _lazyShardEnvelopeType;
    private readonly Lazy<INamedTypeSymbol?> _lazyStartEntityType;
    
    public Version Version { get; }
    public INamedTypeSymbol? ClusterShardingType => _lazyClusterShardingType.Value;
    public INamedTypeSymbol? MessageExtractorInterface => _lazyMessageExtractorType.Value;
    public INamedTypeSymbol? ShardEnvelopeType => _lazyShardEnvelopeType.Value;
    public INamedTypeSymbol? StartEntityType => _lazyStartEntityType.Value;

    private AkkaClusterShardingContext(Compilation compilation, Version version)
    {
        Version = version;
        _lazyClusterShardingType = new Lazy<INamedTypeSymbol?>(() => ClusterShardingSymbolFactory.AkkaClusterSharding(compilation));
        _lazyMessageExtractorType = new Lazy<INamedTypeSymbol?>(() => ClusterShardingSymbolFactory.AkkaMessageExtractorInterface(compilation));
        _lazyShardEnvelopeType = new Lazy<INamedTypeSymbol?>(() => ClusterShardingSymbolFactory.AkkaShardEnvelope(compilation));
        _lazyStartEntityType = new Lazy<INamedTypeSymbol?>(() => ClusterShardingSymbolFactory.AkkaStartEntity(compilation));
    }
    
    public static IAkkaClusterShardingContext Get(Compilation compilation, Version? versionOverride = null)
    {
        // assert that compilation is not null
        Guard.AssertIsNotNull(compilation);

        var version =
            versionOverride ??
            compilation
                .ReferencedAssemblyNames
                .FirstOrDefault(a => a.Name.Equals("Akka.Cluster.Sharding", StringComparison.OrdinalIgnoreCase))
                ?.Version;

        return version is null ? EmptyAkkaClusterShardingContext.Instance : new AkkaClusterShardingContext(compilation, version);
    }
}