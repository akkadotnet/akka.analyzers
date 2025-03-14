// -----------------------------------------------------------------------
//  <copyright file="AkkaStreamsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Streams;

public sealed class EmptyStreamsContext : IAkkaStreamsContext
{
    private EmptyStreamsContext()
    {
    }

    public static EmptyStreamsContext Instance { get; } = new();

    public Version Version { get; } = new();
    public INamedTypeSymbol? ActorMaterializerExtensionsType => null;
    
    public IActorMaterializerExtensionsContext? ActorMaterializerExtensions => EmptyActorMaterializerExtensionsContext.Instance;
}

/// <summary>
/// Default AkkaStreamsContext.
/// </summary>
/// <remarks>
/// Used to indicate whether Akka.Streams is present inside the solution being scanned and
/// provides access to some of the built-in type symbols that are used in analysis rules.
/// </remarks>
public sealed class AkkaStreamsContext : IAkkaStreamsContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyActorMaterializerExtensionsType;
    
    private readonly Lazy<IActorMaterializerExtensionsContext> _lazyActorMaterializerExtensions;
    
    private AkkaStreamsContext(Compilation compilation, Version version)
    {
        Version = version;
        _lazyActorMaterializerExtensionsType = new Lazy<INamedTypeSymbol?>(() => StreamsSymbolFactory.AkkaStreams(compilation));
        
        _lazyActorMaterializerExtensions = new Lazy<IActorMaterializerExtensionsContext>(() => ActorMaterializerExtensionsContext.Get(this));
    }
    
    public static IAkkaStreamsContext Get(Compilation compilation, Version? versionOverride = null)
    {
        // assert that compilation is not null
        Guard.AssertIsNotNull(compilation);

        var version =
            versionOverride ??
            compilation
                .ReferencedAssemblyNames
                .FirstOrDefault(a => a.Name.Equals(StreamsSymbolFactory.StreamsNamespace, StringComparison.OrdinalIgnoreCase))
                ?.Version;

        return version is null ? EmptyStreamsContext.Instance : new AkkaStreamsContext(compilation, version);
    }

    public Version Version { get; }
    public INamedTypeSymbol? ActorMaterializerExtensionsType => _lazyActorMaterializerExtensionsType.Value;
    
    public IActorMaterializerExtensionsContext? ActorMaterializerExtensions => _lazyActorMaterializerExtensions.Value;
}