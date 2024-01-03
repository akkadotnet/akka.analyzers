// -----------------------------------------------------------------------
//  <copyright file="TypeSymbolFactory.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

public static class TypeSymbolFactory
{
    public static INamedTypeSymbol? ActorBase(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.ActorBase");
    }

    public static INamedTypeSymbol? ActorReference(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.IActorRef");
    }

    public static INamedTypeSymbol? Props(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.Props");
    }
    
    public static INamedTypeSymbol? ActorContext(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.IActorContext");
    }
    
    public static INamedTypeSymbol? IndirectActorProducer(Compilation compilation)
    {
        return Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName("Akka.Actor.IIndirectActorProducer");
    }
}