// -----------------------------------------------------------------------
//  <copyright file="AkkaCoreDispatchContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Dispatch;

public sealed class EmptyAkkaCoreDispatchContext : IAkkaCoreDispatchContext
{
    private EmptyAkkaCoreDispatchContext() { }
    public static EmptyAkkaCoreDispatchContext Instance { get; } = new();
    public INamedTypeSymbol? ISystemMessageType => null;
}

public sealed class AkkaCoreDispatchContext : IAkkaCoreDispatchContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyISystemMessageType;

    private AkkaCoreDispatchContext(Compilation compilation)
    {
        _lazyISystemMessageType = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName("Akka.Dispatch.SysMsg.ISystemMessage"));
    }

    public INamedTypeSymbol? ISystemMessageType => _lazyISystemMessageType.Value;

    public static IAkkaCoreDispatchContext Get(Compilation compilation)
        => new AkkaCoreDispatchContext(compilation);
} 