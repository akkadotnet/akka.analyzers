// -----------------------------------------------------------------------
//  <copyright file="ITellSchedulerFactory.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Akka.Analyzers.Context.Core;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Core.Actor;

// ReSharper disable once InconsistentNaming
public static class ITellSchedulerFactory
{
    public static ImmutableArray<ISymbol> ScheduleTellOnce(Compilation compilation)
        => ActorSymbolFactory.TellSchedulerInterface(compilation)!
            .GetMembers("ScheduleTellOnce");

    public static ImmutableArray<ISymbol> ScheduleTellRepeatedly(Compilation compilation)
        => ActorSymbolFactory.TellSchedulerInterface(compilation)!
            .GetMembers("ScheduleTellRepeatedly");
}