// -----------------------------------------------------------------------
//  <copyright file="IAkkaCoreContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context.Core.Actor;

namespace Akka.Analyzers.Context.Core;

/// <summary>
///     Provides information about the core Akka.dll
/// </summary>
public interface IAkkaCoreContext
{
    /// <summary>
    ///     Gets the version number of the core Akka.NET assembly.
    /// </summary>
    public Version Version { get; }

    public IAkkaCoreActorContext Actor { get; }
}
