// -----------------------------------------------------------------------
//  <copyright file="ISourceContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Streams;

public interface ISourceContext
{
    public ImmutableArray<IMethodSymbol> RunAggregate { get; }
    public ImmutableArray<IMethodSymbol> RunAggregateAsync { get; }
} 