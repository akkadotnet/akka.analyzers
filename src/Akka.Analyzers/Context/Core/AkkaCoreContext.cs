// -----------------------------------------------------------------------
//  <copyright file="AkkaCoreContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context.Core.Actor;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core;

/// <summary>
///     No-op context for Akka.NET core dll.
/// </summary>
public sealed class EmptyCoreContext : IAkkaCoreContext
{
    private EmptyCoreContext()
    {
    }

    public static EmptyCoreContext Instance { get; } = new();

    public Version Version { get; } = new();
    public IAkkaCoreActorContext Actor => EmptyAkkaCoreActorContext.Instance;
}

/// <summary>
///     Default AkkaCoreContext.
/// </summary>
/// <remarks>
///     At some point in the future, this class may be sub-classed or extended with additional
///     properties to include data about specific major versions of Akka.NET.
/// </remarks>
public sealed class AkkaCoreContext : IAkkaCoreContext
{
    public const string AkkaNamespace = "Akka";
    
    private AkkaCoreContext(Compilation compilation, Version version)
    {
        Version = version;
        Actor = AkkaCoreActorContext.Get(compilation);
    }

    /// <inheritdoc />
    public Version Version { get; }
    public IAkkaCoreActorContext Actor { get; }

    public static IAkkaCoreContext Get(
        Compilation compilation,
        Version? versionOverride = null)
    {
        // assert that compilation is not null
        Guard.AssertIsNotNull(compilation);

        var version =
            versionOverride ??
            compilation
                .ReferencedAssemblyNames
                .FirstOrDefault(a => a.Name.Equals("Akka", StringComparison.OrdinalIgnoreCase))
                ?.Version;

        return version is null ? EmptyCoreContext.Instance : new AkkaCoreContext(compilation, version);
    }
}