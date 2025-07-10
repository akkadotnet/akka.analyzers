// -----------------------------------------------------------------------
//  <copyright file="IAkkaStreamsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Streams;

public interface IAkkaStreamsContext
{
    Version Version { get; }
    
    INamedTypeSymbol? ActorMaterializerExtensionsType { get; }
    INamedTypeSymbol? FlowOperationsType { get; }
    INamedTypeSymbol? SinkType { get; }
    INamedTypeSymbol? SourceOperationsType { get; }
    INamedTypeSymbol? SubFlowOperationsType { get; }
    INamedTypeSymbol? SourceType { get; }
    
    IActorMaterializerExtensionsContext? ActorMaterializerExtensions { get; }
    IFlowOperationsContext? FlowOperations { get; }
    ISinkContext? Sink { get; }
    ISourceOperationsContext? SourceOperations { get; }
    ISubFlowOperationsContext? SubFlowOperations { get; }
    ISourceContext? Source { get; }
}