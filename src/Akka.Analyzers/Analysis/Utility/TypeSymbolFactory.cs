// -----------------------------------------------------------------------
//  <copyright file="TypeSymbolFactory.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

public static class TypeSymbolFactory
{
    public static INamedTypeSymbol? ActorBase(Compilation compilation) => Guard.AssertIsNotNull(compilation)
        .GetTypeByMetadataName("Akka.Actor.ActorBase");
    
    public static INamedTypeSymbol? ActorReference(Compilation compilation) => Guard.AssertIsNotNull(compilation)
        .GetTypeByMetadataName("Akka.Actor.IActorRef");
    
    public static INamedTypeSymbol? Props(Compilation compilation) => Guard.AssertIsNotNull(compilation)
        .GetTypeByMetadataName("Akka.Actor.Props");
}