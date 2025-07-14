// -----------------------------------------------------------------------
//  <copyright file="ISystemCollectionsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.Analyzers.Context.System;

public interface ISystemCollectionsContext
{
    /// <summary>
    /// Gets the System.Collections.IEnumerable type symbol.
    /// </summary>
    Microsoft.CodeAnalysis.INamedTypeSymbol? IEnumerableType { get; }

    /// <summary>
    /// Gets the System.Collections.Generic.IEnumerable<T> type symbol.
    /// </summary>
    Microsoft.CodeAnalysis.INamedTypeSymbol? IEnumerableGenericType { get; }
} 