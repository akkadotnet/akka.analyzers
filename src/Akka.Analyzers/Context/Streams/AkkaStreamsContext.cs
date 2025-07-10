// -----------------------------------------------------------------------
//  <copyright file="AkkaStreamsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers;
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
    public INamedTypeSymbol? FlowOperationsType => null;
    public INamedTypeSymbol? SinkType => null;
    public INamedTypeSymbol? SourceOperationsType => null;
    public INamedTypeSymbol? SubFlowOperationsType => null;
    public INamedTypeSymbol? SourceType => null;
    
    public IActorMaterializerExtensionsContext? ActorMaterializerExtensions => EmptyActorMaterializerExtensionsContext.Instance;
    public IFlowOperationsContext? FlowOperations => EmptyFlowOperationsContext.Instance;
    public ISinkContext? Sink => EmptySinkContext.Instance;
    public ISourceOperationsContext? SourceOperations => EmptySourceOperationsContext.Instance;
    public ISubFlowOperationsContext? SubFlowOperations => EmptySubFlowOperationsContext.Instance;
    public ISourceContext? Source => EmptySourceContext.Instance;
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
    private readonly Lazy<INamedTypeSymbol?> _lazyFlowOperationsType;
    private readonly Lazy<INamedTypeSymbol?> _lazySinkType;
    private readonly Lazy<INamedTypeSymbol?> _lazySourceOperationsType;
    private readonly Lazy<INamedTypeSymbol?> _lazySubFlowOperationsType;
    private readonly Lazy<INamedTypeSymbol?> _lazySourceType;
    
    private readonly Lazy<IActorMaterializerExtensionsContext> _lazyActorMaterializerExtensions;
    private readonly Lazy<IFlowOperationsContext> _lazyFlowOperations;
    private readonly Lazy<ISinkContext> _lazySink;
    private readonly Lazy<ISourceOperationsContext> _lazySourceOperations;
    private readonly Lazy<ISubFlowOperationsContext> _lazySubFlowOperations;
    private readonly Lazy<ISourceContext> _lazySource;
    
    private AkkaStreamsContext(Compilation compilation, Version version)
    {
        Version = version;
        _lazyActorMaterializerExtensionsType = new Lazy<INamedTypeSymbol?>(() => StreamsSymbolFactory.AkkaStreams(compilation));
        _lazyFlowOperationsType = new Lazy<INamedTypeSymbol?>(() => StreamsSymbolFactory.FlowOperations(compilation));
        _lazySinkType = new Lazy<INamedTypeSymbol?>(() => StreamsSymbolFactory.Sink(compilation));
        _lazySourceOperationsType = new Lazy<INamedTypeSymbol?>(() => StreamsSymbolFactory.SourceOperations(compilation));
        _lazySubFlowOperationsType = new Lazy<INamedTypeSymbol?>(() => StreamsSymbolFactory.SubFlowOperations(compilation));
        _lazySourceType = new Lazy<INamedTypeSymbol?>(() => StreamsSymbolFactory.Source(compilation));
        
        _lazyActorMaterializerExtensions = new Lazy<IActorMaterializerExtensionsContext>(() => ActorMaterializerExtensionsContext.Get(this));
        _lazyFlowOperations = new Lazy<IFlowOperationsContext>(() => FlowOperationsContext.Get(this));
        _lazySink = new Lazy<ISinkContext>(() => SinkContext.Get(this));
        _lazySourceOperations = new Lazy<ISourceOperationsContext>(() => SourceOperationsContext.Get(this));
        _lazySubFlowOperations = new Lazy<ISubFlowOperationsContext>(() => SubFlowOperationsContext.Get(this));
        _lazySource = new Lazy<ISourceContext>(() => SourceContext.Get(this));
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
    public INamedTypeSymbol? FlowOperationsType => _lazyFlowOperationsType.Value;
    public INamedTypeSymbol? SinkType => _lazySinkType.Value;
    public INamedTypeSymbol? SourceOperationsType => _lazySourceOperationsType.Value;
    public INamedTypeSymbol? SubFlowOperationsType => _lazySubFlowOperationsType.Value;
    public INamedTypeSymbol? SourceType => _lazySourceType.Value;
    
    public IActorMaterializerExtensionsContext? ActorMaterializerExtensions => _lazyActorMaterializerExtensions.Value;
    public IFlowOperationsContext? FlowOperations => _lazyFlowOperations.Value;
    public ISinkContext? Sink => _lazySink.Value;
    public ISourceOperationsContext? SourceOperations => _lazySourceOperations.Value;
    public ISubFlowOperationsContext? SubFlowOperations => _lazySubFlowOperations.Value;
    public ISourceContext? Source => _lazySource.Value;
}