// -----------------------------------------------------------------------
//  <copyright file="SystemCollectionsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.System;

public sealed class SystemCollectionsContext : ISystemCollectionsContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyIEnumerableType;
    private readonly Lazy<INamedTypeSymbol?> _lazyIEnumerableGenericType;

    public SystemCollectionsContext(Compilation compilation)
    {
        _lazyIEnumerableType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.IEnumerable"));
        _lazyIEnumerableGenericType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1"));
    }

    public INamedTypeSymbol? IEnumerableType => _lazyIEnumerableType.Value;
    public INamedTypeSymbol? IEnumerableGenericType => _lazyIEnumerableGenericType.Value;
}
