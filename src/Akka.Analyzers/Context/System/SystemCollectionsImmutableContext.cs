// -----------------------------------------------------------------------
//  <copyright file="SystemCollectionsImmutableContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.System;

public sealed class EmptySystemCollectionsImmutableContext : ISystemCollectionsImmutableContext
{
    private EmptySystemCollectionsImmutableContext() { }
    public static EmptySystemCollectionsImmutableContext Instance { get; } = new();
    public INamedTypeSymbol? ImmutableArrayType => null;
    public INamedTypeSymbol? ImmutableListType => null;
    public INamedTypeSymbol? ImmutableDictionaryType => null;
    public INamedTypeSymbol? ImmutableHashSetType => null;
    public INamedTypeSymbol? ImmutableQueueType => null;
    public INamedTypeSymbol? ImmutableStackType => null;
    public INamedTypeSymbol? ImmutableSortedSetType => null;
    public INamedTypeSymbol? ImmutableSortedDictionaryType => null;
    public INamedTypeSymbol? ImmutableSetType => null;
}

public sealed class SystemCollectionsImmutableContext : ISystemCollectionsImmutableContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyImmutableArrayType;
    private readonly Lazy<INamedTypeSymbol?> _lazyImmutableListType;
    private readonly Lazy<INamedTypeSymbol?> _lazyImmutableDictionaryType;
    private readonly Lazy<INamedTypeSymbol?> _lazyImmutableHashSetType;
    private readonly Lazy<INamedTypeSymbol?> _lazyImmutableQueueType;
    private readonly Lazy<INamedTypeSymbol?> _lazyImmutableStackType;
    private readonly Lazy<INamedTypeSymbol?> _lazyImmutableSortedSetType;
    private readonly Lazy<INamedTypeSymbol?> _lazyImmutableSortedDictionaryType;
    private readonly Lazy<INamedTypeSymbol?> _lazyImmutableSetType;

    public INamedTypeSymbol? ImmutableArrayType => _lazyImmutableArrayType.Value;
    public INamedTypeSymbol? ImmutableListType => _lazyImmutableListType.Value;
    public INamedTypeSymbol? ImmutableDictionaryType => _lazyImmutableDictionaryType.Value;
    public INamedTypeSymbol? ImmutableHashSetType => _lazyImmutableHashSetType.Value;
    public INamedTypeSymbol? ImmutableQueueType => _lazyImmutableQueueType.Value;
    public INamedTypeSymbol? ImmutableStackType => _lazyImmutableStackType.Value;
    public INamedTypeSymbol? ImmutableSortedSetType => _lazyImmutableSortedSetType.Value;
    public INamedTypeSymbol? ImmutableSortedDictionaryType => _lazyImmutableSortedDictionaryType.Value;
    public INamedTypeSymbol? ImmutableSetType => _lazyImmutableSetType.Value;

    private SystemCollectionsImmutableContext(Compilation compilation)
    {
        Guard.AssertIsNotNull(compilation);
        
        _lazyImmutableArrayType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1"));
        _lazyImmutableListType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableList`1"));
        _lazyImmutableDictionaryType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableDictionary`2"));
        _lazyImmutableHashSetType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableHashSet`1"));
        _lazyImmutableQueueType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableQueue`1"));
        _lazyImmutableStackType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableStack`1"));
        _lazyImmutableSortedSetType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableSortedSet`1"));
        _lazyImmutableSortedDictionaryType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableSortedDictionary`2"));
        _lazyImmutableSetType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableSet`1"));
    }

    public static SystemCollectionsImmutableContext Get(Compilation compilation)
    {
        Guard.AssertIsNotNull(compilation);
        return new SystemCollectionsImmutableContext(compilation);
    }
} 