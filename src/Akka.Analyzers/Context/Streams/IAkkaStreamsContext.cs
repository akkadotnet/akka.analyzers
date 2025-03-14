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
    
    IActorMaterializerExtensionsContext? ActorMaterializerExtensions { get; }
}