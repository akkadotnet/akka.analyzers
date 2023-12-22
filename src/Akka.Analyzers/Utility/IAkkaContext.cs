// -----------------------------------------------------------------------
//  <copyright file="IAkkaContext.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

/// <summary>
/// Provides information about the Akka.NET context (i.e. which libraries, which versions) in which the analyzer is running.
/// </summary>
public interface IAkkaContext
{
    /// <summary>
    /// Data about the core Akka.NET library.
    /// </summary>
    IAkkaCoreContext? AkkaCore { get; }
    
    /// <summary>
    /// Does the current compilation context even have Akka.NET installed?
    /// </summary>
    public bool HasAkkaInstalled { get; }
}

public class AkkaContext : IAkkaContext
{
    private AkkaContext(){}
   
    public IAkkaCoreContext? AkkaCore { get; private set; }
    public bool HasAkkaInstalled  => AkkaCore is not null;

    public static AkkaContext? Get(
        Compilation compilation,
        Version? versionOverride = null)
    {
        // assert that compilation is not null
        Guard.AssertIsNotNull(compilation);

        var akkaCoreContext = AkkaCoreContext.Get(compilation, versionOverride);
        if (akkaCoreContext is null)
            return null;

        return new AkkaContext
        {
            AkkaCore = AkkaCoreContext.Get(compilation, versionOverride)
        };
    }
}