// -----------------------------------------------------------------------
//  &lt;copyright file="AkkaClusterShardingContext.cs" company="Akka.NET Project"&gt;&#xD;
//      Copyright (C) 2013-2024 .NET Foundation &lt;https://github.com/akkadotnet/akka.net&gt;&#xD;
//  &lt;/copyright&gt;&#xD;
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

/// <summary>
/// Data about the Akka.Cluster.Sharding assembly in the solution being analyzed.
/// </summary>
public interface IAkkaClusterShardingContext
{
    Version Version { get; }
    
    INamedTypeSymbol? ClusterShardingType { get; }
    
    INamedTypeSymbol? IMessageExtractorType { get; }
    
    INamedTypeSymbol? ShardEnvelopeType { get; }
    
    INamedTypeSymbol? StartEntityType { get; }
}

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
    public INamedTypeSymbol? IMessageExtractorType => null;
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
    public INamedTypeSymbol? IMessageExtractorType => _lazyMessageExtractorType.Value;
    public INamedTypeSymbol? ShardEnvelopeType => _lazyShardEnvelopeType.Value;
    public INamedTypeSymbol? StartEntityType => _lazyStartEntityType.Value;

    private AkkaClusterShardingContext(Compilation compilation, Version version)
    {
        Version = version;
        _lazyClusterShardingType = new Lazy<INamedTypeSymbol?>(() => TypeSymbolFactory.AkkaClusterSharding(compilation));
        _lazyMessageExtractorType = new Lazy<INamedTypeSymbol?>(() => TypeSymbolFactory.AkkaMessageExtractor(compilation));
        _lazyShardEnvelopeType = new Lazy<INamedTypeSymbol?>(() => TypeSymbolFactory.AkkaShardEnvelope(compilation));
        _lazyStartEntityType = new Lazy<INamedTypeSymbol?>(() => TypeSymbolFactory.AkkaStartEntity(compilation));
    }
    
    public static IAkkaClusterShardingContext? Get(Compilation compilation, Version? versionOverride = null)
    {
        // assert that compilation is not null
        Guard.AssertIsNotNull(compilation);

        var version =
            versionOverride ??
            compilation
                .ReferencedAssemblyNames
                .FirstOrDefault(a => a.Name.Equals("Akka.Cluster.Sharding", StringComparison.OrdinalIgnoreCase))
                ?.Version;

        return version is null ? null : new AkkaClusterShardingContext(compilation, version);
    }
}