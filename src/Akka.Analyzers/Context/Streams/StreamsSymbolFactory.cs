// -----------------------------------------------------------------------
//  <copyright file="StreamsSymbolFactory.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context.Core;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Streams;

public static class StreamsSymbolFactory
{
    public const string StreamsNamespace = AkkaCoreContext.AkkaNamespace + ".Streams";
    
    public static INamedTypeSymbol? AkkaStreams(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsNamespace}.ActorMaterializerExtensions");
    
    public static INamedTypeSymbol? FlowOperations(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsNamespace}.FlowOperations");
    
    public static INamedTypeSymbol? Sink(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsNamespace}.Sink");
    
    public static INamedTypeSymbol? SourceOperations(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsNamespace}.SourceOperations");
    
    public static INamedTypeSymbol? SubFlowOperations(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsNamespace}.SubFlowOperations");
    
    public static INamedTypeSymbol? Source(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsNamespace}.Source");
}