// -----------------------------------------------------------------------
//  <copyright file="ISystemCollectionsImmutableContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.System;

public interface ISystemCollectionsImmutableContext
{
    INamedTypeSymbol? ImmutableArrayType { get; }
    INamedTypeSymbol? ImmutableListType { get; }
    INamedTypeSymbol? ImmutableDictionaryType { get; }
    INamedTypeSymbol? ImmutableHashSetType { get; }
    INamedTypeSymbol? ImmutableQueueType { get; }
    INamedTypeSymbol? ImmutableStackType { get; }
    INamedTypeSymbol? ImmutableSortedSetType { get; }
    INamedTypeSymbol? ImmutableSortedDictionaryType { get; }
    INamedTypeSymbol? ImmutableSetType { get; }
}