// -----------------------------------------------------------------------
//  <copyright file="AkkaContext.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

/// <summary>
/// Provides information about the Akka.NET context (i.e. which libraries, which versions) in which the analyzer is running.
/// </summary>
public class AkkaContext
{
    private IAkkaCoreContext? _akkaCore;

    /// <summary>
    /// Initializes a new instance of the <see cref="AkkaContext"/> class.
    /// </summary>
    /// <param name="compilation">The Roslyn compilation object used to look up types and
    /// inspect references</param>
    public AkkaContext(Compilation compilation)
    {
        _akkaCore = AkkaCoreContext.Get(compilation);
    }

    private AkkaContext()
    {
    }

    /// <summary>
    /// Data about the core Akka.NET library.
    /// </summary>
    public IAkkaCoreContext AkkaCore
    {
        get { return _akkaCore ??= EmptyCoreContext.Instance; }
    }

    /// <summary>
    /// Does the current compilation context even have Akka.NET installed?
    /// </summary>
    public bool HasAkkaInstalled => AkkaCore is not null;
}