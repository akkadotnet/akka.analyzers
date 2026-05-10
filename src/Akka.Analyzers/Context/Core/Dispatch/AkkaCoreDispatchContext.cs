// -----------------------------------------------------------------------
//  <copyright file="AkkaCoreDispatchContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Dispatch;

public sealed class EmptyAkkaCoreDispatchContext : IAkkaCoreDispatchContext
{
    private EmptyAkkaCoreDispatchContext() { }
    public static EmptyAkkaCoreDispatchContext Instance { get; } = new();
    public INamedTypeSymbol? ISystemMessageType => null;
    public INamedTypeSymbol? ActorTaskSchedulerType => null;
    public ImmutableArray<IMethodSymbol> ActorTaskSchedulerRunTask => ImmutableArray<IMethodSymbol>.Empty;
}

public sealed class AkkaCoreDispatchContext : IAkkaCoreDispatchContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyISystemMessageType;
    private readonly Lazy<INamedTypeSymbol?> _lazyActorTaskSchedulerType;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyActorTaskSchedulerRunTask;

    private AkkaCoreDispatchContext(Compilation compilation)
    {
        _lazyISystemMessageType = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName("Akka.Dispatch.SysMsg.ISystemMessage"));
        _lazyActorTaskSchedulerType = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName("Akka.Dispatch.ActorTaskScheduler"));
        _lazyActorTaskSchedulerRunTask = new Lazy<ImmutableArray<IMethodSymbol>>(() =>
            _lazyActorTaskSchedulerType.Value is null
                ? ImmutableArray<IMethodSymbol>.Empty
                : _lazyActorTaskSchedulerType.Value.GetMembers("RunTask")
                    .OfType<IMethodSymbol>().ToImmutableArray());
    }

    public INamedTypeSymbol? ISystemMessageType => _lazyISystemMessageType.Value;
    public INamedTypeSymbol? ActorTaskSchedulerType => _lazyActorTaskSchedulerType.Value;
    public ImmutableArray<IMethodSymbol> ActorTaskSchedulerRunTask => _lazyActorTaskSchedulerRunTask.Value;

    public static IAkkaCoreDispatchContext Get(Compilation compilation)
        => new AkkaCoreDispatchContext(compilation);
}
