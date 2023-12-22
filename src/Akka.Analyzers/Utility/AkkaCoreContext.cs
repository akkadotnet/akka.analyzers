using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

/// <summary>
/// Provides information about the core Akka.dll
/// </summary>
public interface IAkkaCoreContext
{
    /// <summary>
    /// Gets the version number of the core Akka.NET assembly.
    /// </summary>
    Version Version { get; }
    
    public INamedTypeSymbol? ActorBaseType { get; }
    public INamedTypeSymbol? ActorRefType { get; }
    public INamedTypeSymbol? PropsType { get; }
}

/// <summary>
/// No-op context for Akka.NET core dll.
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
}

/// <summary>
/// Default AkkaCoreContext.
/// </summary>
/// <remarks>
/// At some point in the future, this class may be sub-classed or extended with additional
/// properties to include data about specific major versions of Akka.NET.
/// </remarks>
public class AkkaCoreContext : IAkkaCoreContext
{
    private readonly Lazy<INamedTypeSymbol?> _lazyActorBaseType;
    private readonly Lazy<INamedTypeSymbol?> _lazyActorRefType;
    private readonly Lazy<INamedTypeSymbol?> _lazyPropsType;
    
    private AkkaCoreContext(Compilation compilation, Version version)
    {
        Version = version;
        _lazyActorBaseType = new(() => TypeSymbolFactory.ActorBase(compilation));
        _lazyActorRefType = new(() => TypeSymbolFactory.ActorReference(compilation));
        _lazyPropsType = new(() => TypeSymbolFactory.Props(compilation));
    }

    /// <inheritdoc/>
    public Version Version { get; }
    
    public INamedTypeSymbol? ActorBaseType => _lazyActorBaseType.Value;
    public INamedTypeSymbol? ActorRefType => _lazyActorRefType.Value;
    public INamedTypeSymbol? PropsType => _lazyPropsType.Value;

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