// -----------------------------------------------------------------------
//  <copyright file="IAkkaCoreDispatchContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Dispatch;

// ReSharper disable InconsistentNaming
public interface IAkkaCoreDispatchContext
{
    // Only focus on the ISystemMessage interface type for now
    public INamedTypeSymbol? ISystemMessageType { get; }
} 