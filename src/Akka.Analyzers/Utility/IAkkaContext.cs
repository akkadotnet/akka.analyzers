// -----------------------------------------------------------------------
//  <copyright file="IAkkaContext.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.Analyzers;

/// <summary>
/// Provides information about the Akka.NET context (i.e. which libraries, which versions) in which the analyzer is running.
/// </summary>
public interface IAkkaContext
{
    IAkkaCoreContext AkkaCoreContext { get; }
}