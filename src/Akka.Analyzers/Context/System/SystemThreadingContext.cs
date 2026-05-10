// -----------------------------------------------------------------------
//  <copyright file="SystemTask.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.System;

public interface ISystemThreadingTasksContext
{
    public INamedTypeSymbol? TaskType { get; }
    public INamedTypeSymbol? TaskOfTType { get; }
    public INamedTypeSymbol? ValueTaskType { get; }
    public INamedTypeSymbol? ValueTaskOfTType { get; }
}

public sealed class SystemThreadingTasksContext: ISystemThreadingTasksContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyTaskType;
    private readonly Lazy<INamedTypeSymbol?> _lazyTaskOfTType;
    private readonly Lazy<INamedTypeSymbol?> _lazyValueTaskType;
    private readonly Lazy<INamedTypeSymbol?> _lazyValueTaskOfTType;

    private SystemThreadingTasksContext(Compilation compilation)
    {
        Guard.AssertIsNotNull(compilation);

        _lazyTaskType = new Lazy<INamedTypeSymbol?>(() =>
        {
            var type = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            return type ?? throw new InvalidOperationException(
                "The type `System.Threading.Tasks.Task` does not exist, this target framework platform is not supported.");
        });
        _lazyTaskOfTType = new Lazy<INamedTypeSymbol?>(
            () => compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1"));
        _lazyValueTaskType = new Lazy<INamedTypeSymbol?>(
            () => compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask"));
        _lazyValueTaskOfTType = new Lazy<INamedTypeSymbol?>(
            () => compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1"));
    }

    public INamedTypeSymbol? TaskType => _lazyTaskType.Value;
    public INamedTypeSymbol? TaskOfTType => _lazyTaskOfTType.Value;
    public INamedTypeSymbol? ValueTaskType => _lazyValueTaskType.Value;
    public INamedTypeSymbol? ValueTaskOfTType => _lazyValueTaskOfTType.Value;

    public static SystemThreadingTasksContext Get(Compilation compilation)
        => new(compilation);
}