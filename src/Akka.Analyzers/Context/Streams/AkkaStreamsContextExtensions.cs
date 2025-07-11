// -----------------------------------------------------------------------
//  <copyright file="AkkaStreamsContextExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Streams;

public static class AkkaStreamsContextExtensions
{
    public static ImmutableArray<IMethodSymbol> GetAllAggregateMethods(this IAkkaStreamsContext ctx)
    {
        Guard.AssertIsNotNull(ctx);
        
        var aggregateMethods = new List<IMethodSymbol>();
        aggregateMethods.AddRange(ctx.FlowOperations.Aggregate);
        aggregateMethods.AddRange(ctx.FlowOperations.AggregateAsync);
        aggregateMethods.AddRange(ctx.Sink.Aggregate);
        aggregateMethods.AddRange(ctx.Sink.AggregateAsync);
        aggregateMethods.AddRange(ctx.SourceOperations.Aggregate);
        aggregateMethods.AddRange(ctx.SourceOperations.AggregateAsync);
        aggregateMethods.AddRange(ctx.SubFlowOperations.Aggregate);
        aggregateMethods.AddRange(ctx.SubFlowOperations.AggregateAsync);
        aggregateMethods.AddRange(ctx.Source.RunAggregate);
        aggregateMethods.AddRange(ctx.Source.RunAggregateAsync);
        
        return aggregateMethods.ToImmutableArray();
    }
} 