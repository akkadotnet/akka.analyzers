// -----------------------------------------------------------------------
//  <copyright file="AkkaCoreContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

/// <summary>
///     Provides information about the core Akka.dll
/// </summary>
public interface IAkkaCoreContext
{
    /// <summary>
    ///     Gets the version number of the core Akka.NET assembly.
    /// </summary>
    Version Version { get; }

    public INamedTypeSymbol? ActorBaseType { get; }
    public INamedTypeSymbol? ActorRefType { get; }
    public INamedTypeSymbol? PropsType { get; }
    
    public INamedTypeSymbol? ActorContextType { get; }
    
    public INamedTypeSymbol? IndirectActorProducerType { get; }
}

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
    public INamedTypeSymbol? ActorBaseType => null;
    public INamedTypeSymbol? ActorRefType => null;
    public INamedTypeSymbol? PropsType => null;
    
    public INamedTypeSymbol? ActorContextType => null;
    
    public INamedTypeSymbol? IndirectActorProducerType => null;
}

/// <summary>
///     Default AkkaCoreContext.
/// </summary>
/// <remarks>
///     At some point in the future, this class may be sub-classed or extended with additional
///     properties to include data about specific major versions of Akka.NET.
/// </remarks>
public class AkkaCoreContext : IAkkaCoreContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyActorBaseType;
    private readonly Lazy<INamedTypeSymbol?> _lazyActorRefType;
    private readonly Lazy<INamedTypeSymbol?> _lazyPropsType;
    private readonly Lazy<INamedTypeSymbol?> _lazyActorContextType;
    private readonly Lazy<INamedTypeSymbol?> _lazyIIndirectActorProducerType;

    private AkkaCoreContext(Compilation compilation, Version version)
    {
        Version = version;
        _lazyActorBaseType = new Lazy<INamedTypeSymbol?>(() => TypeSymbolFactory.ActorBase(compilation));
        _lazyActorRefType = new Lazy<INamedTypeSymbol?>(() => TypeSymbolFactory.ActorReference(compilation));
        _lazyPropsType = new Lazy<INamedTypeSymbol?>(() => TypeSymbolFactory.Props(compilation));
        _lazyActorContextType = new Lazy<INamedTypeSymbol?>(() => TypeSymbolFactory.ActorContext(compilation));
        _lazyIIndirectActorProducerType = new Lazy<INamedTypeSymbol?>(() => TypeSymbolFactory.IndirectActorProducer(compilation));
    }

    /// <inheritdoc />
    public Version Version { get; }

    public INamedTypeSymbol? ActorBaseType => _lazyActorBaseType.Value;
    public INamedTypeSymbol? ActorRefType => _lazyActorRefType.Value;
    public INamedTypeSymbol? PropsType => _lazyPropsType.Value;
    
    public INamedTypeSymbol? ActorContextType => _lazyActorContextType.Value;
    
    public INamedTypeSymbol? IndirectActorProducerType => _lazyIIndirectActorProducerType.Value;

    public static AkkaCoreContext? Get(
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

        return version is null ? null : new AkkaCoreContext(compilation, version);
    }
}