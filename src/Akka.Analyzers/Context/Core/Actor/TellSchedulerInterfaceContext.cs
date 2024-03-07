// -----------------------------------------------------------------------
//  <copyright file="TellSchedulerContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Akka.Analyzers.Core.Actor;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface ITellSchedulerInterfaceContext
{
    public ImmutableArray<ISymbol> ScheduleTellOnce { get; }
    public ImmutableArray<ISymbol> ScheduleTellRepeatedly { get; }
}

public class EmptyTellSchedulerInterfaceContext : ITellSchedulerInterfaceContext
{
    private EmptyTellSchedulerInterfaceContext() { }

    public static readonly EmptyTellSchedulerInterfaceContext Instance = new();

    public ImmutableArray<ISymbol> ScheduleTellOnce => new();
    public ImmutableArray<ISymbol> ScheduleTellRepeatedly => new();
}

public class TellSchedulerInterfaceContext: ITellSchedulerInterfaceContext
{
    private readonly Lazy<ImmutableArray<ISymbol>> _lazyScheduleTellOnce;
    private readonly Lazy<ImmutableArray<ISymbol>> _lazyScheduleTellRepeatedly;
    
    private TellSchedulerInterfaceContext(Compilation compilation)
    {
        _lazyScheduleTellOnce = new Lazy<ImmutableArray<ISymbol>>(() => ITellSchedulerFactory.ScheduleTellOnce(compilation));
        _lazyScheduleTellRepeatedly = new Lazy<ImmutableArray<ISymbol>>(() => ITellSchedulerFactory.ScheduleTellRepeatedly(compilation));
    }

    public ImmutableArray<ISymbol> ScheduleTellOnce => _lazyScheduleTellOnce.Value;
    public ImmutableArray<ISymbol> ScheduleTellRepeatedly => _lazyScheduleTellRepeatedly.Value;

    public static ITellSchedulerInterfaceContext Get(Compilation compilation)
        => new TellSchedulerInterfaceContext(compilation);
}