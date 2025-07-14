// -----------------------------------------------------------------------
//  <copyright file="SystemCollectionsImmutableContextExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.System;

public static class SystemCollectionsImmutableContextExtensions
{
    public static ImmutableArray<INamedTypeSymbol> GetAllImmutableCollectionTypes(this ISystemCollectionsImmutableContext ctx)
    {
        Guard.AssertIsNotNull(ctx);
        
        var types = new List<INamedTypeSymbol?>
        {
            ctx.ImmutableArrayType,
            ctx.ImmutableListType,
            ctx.ImmutableDictionaryType,
            ctx.ImmutableHashSetType,
            ctx.ImmutableQueueType,
            ctx.ImmutableStackType,
            ctx.ImmutableSortedSetType,
            ctx.ImmutableSortedDictionaryType,
            ctx.ImmutableSetType
        };
        return types.Where(t => t != null).Cast<INamedTypeSymbol>().ToImmutableArray();
    }
} 