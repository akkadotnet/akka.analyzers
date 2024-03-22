// -----------------------------------------------------------------------
//  <copyright file="ITimerSchedulerContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface ITimerSchedulerContext
{
    public ImmutableArray<IMethodSymbol> StartPeriodicTimer { get; }
    public ImmutableArray<IMethodSymbol> StartSingleTimer { get; }
}

public sealed class EmptyTimerSchedulerContext : ITimerSchedulerContext
{
    public static readonly EmptyTimerSchedulerContext Instance = new();
    private EmptyTimerSchedulerContext() { }
    public ImmutableArray<IMethodSymbol> StartPeriodicTimer => ImmutableArray<IMethodSymbol>.Empty;
    public ImmutableArray<IMethodSymbol> StartSingleTimer => ImmutableArray<IMethodSymbol>.Empty;
}

public sealed class TimerSchedulerContext : ITimerSchedulerContext
{
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyStartPeriodicTimer;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyStartSingleTimer;

    private TimerSchedulerContext(AkkaCoreActorContext context)
    {
        Guard.AssertIsNotNull(context);
        
        _lazyStartPeriodicTimer = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ITimerSchedulerType!
            .GetMembers(nameof(StartPeriodicTimer)).Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyStartSingleTimer = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ITimerSchedulerType!
            .GetMembers(nameof(StartSingleTimer)).Select(m => (IMethodSymbol)m).ToImmutableArray());
    }

    public ImmutableArray<IMethodSymbol> StartPeriodicTimer => _lazyStartPeriodicTimer.Value;
    public ImmutableArray<IMethodSymbol> StartSingleTimer => _lazyStartSingleTimer.Value;
    
    public static TimerSchedulerContext Get(AkkaCoreActorContext context)
    {
        Guard.AssertIsNotNull(context);
        
        return new TimerSchedulerContext(context);
    }
}