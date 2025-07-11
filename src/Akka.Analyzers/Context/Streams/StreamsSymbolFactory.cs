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
    public const string StreamsDslNamespace = StreamsNamespace + ".Dsl";
    
    public static INamedTypeSymbol? AkkaStreams(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsNamespace}.ActorMaterializerExtensions");
    
    public static INamedTypeSymbol? FlowOperations(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsDslNamespace}.FlowOperations");
    
    public static INamedTypeSymbol? Sink(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsDslNamespace}.Sink");
    
    public static INamedTypeSymbol? SourceOperations(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsDslNamespace}.SourceOperations");
    
    public static INamedTypeSymbol? SubFlowOperations(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsDslNamespace}.SubFlowOperations");
    
    public static INamedTypeSymbol? Source(Compilation compilation)
        => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{StreamsDslNamespace}.Source`2");
}