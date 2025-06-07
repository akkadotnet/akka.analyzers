// -----------------------------------------------------------------------
//  <copyright file="AkkaPersistenceContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context.Core;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Persistence;

public interface IAkkaPersistenceContext
{
    Version Version { get; }
    
    INamedTypeSymbol? PersistenceType { get; }
    INamedTypeSymbol? EventsourcedType { get; }
    INamedTypeSymbol? ReceivePersistentActorType { get; }
    
    IEventsourcedContext Eventsourced { get; }
    IReceivePersistentActorContext ReceivePersistentActor { get; }
}

public sealed class EmptyPersistenceContext : IAkkaPersistenceContext
{
    public static EmptyPersistenceContext Instance => new();

    private EmptyPersistenceContext()
    {
    }

    public Version Version => new();
    public INamedTypeSymbol? PersistenceType => null;
    public INamedTypeSymbol? EventsourcedType => null;
    public INamedTypeSymbol? ReceivePersistentActorType => null;
    public IEventsourcedContext Eventsourced => EmptyEventsourcedContext.Instance;
    public IReceivePersistentActorContext ReceivePersistentActor => EmptyReceivePersistentActorContext.Instance;
}

public class AkkaPersistenceContext: IAkkaPersistenceContext
{
    public const string PersistenceNamespace = AkkaCoreContext.AkkaNamespace + ".Persistence";
    
    private readonly Lazy<INamedTypeSymbol?> _lazyPersistenceType;
    private readonly Lazy<INamedTypeSymbol?> _lazyEventsourcedType;
    private readonly Lazy<INamedTypeSymbol?> _lazyReceivePersistentActor;

    private AkkaPersistenceContext(Compilation compilation, Version version)
    {
        Version = version;
        _lazyPersistenceType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName($"{PersistenceNamespace}.Persistence"));
        _lazyEventsourcedType = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName($"{PersistenceNamespace}.Eventsourced"));
        _lazyReceivePersistentActor = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName($"{PersistenceNamespace}.ReceivePersistentActor"));
        
        Eventsourced = EventsourcedContext.Get(this);
        ReceivePersistentActor = ReceivePersistentActorContext.Get(this);
    }
    
    public static IAkkaPersistenceContext Get(Compilation compilation, Version? versionOverride = null)
    {
        // assert that compilation is not null
        Guard.AssertIsNotNull(compilation);

        var version = versionOverride ?? compilation
              .ReferencedAssemblyNames
              .FirstOrDefault(a => a.Name.Equals(PersistenceNamespace, StringComparison.OrdinalIgnoreCase))?
              .Version;

        return version is null ? EmptyPersistenceContext.Instance : new AkkaPersistenceContext(compilation, version);
    }
    
    public Version Version { get; }
    public INamedTypeSymbol? PersistenceType => _lazyPersistenceType.Value;
    public INamedTypeSymbol? EventsourcedType => _lazyEventsourcedType.Value;
    public INamedTypeSymbol? ReceivePersistentActorType => _lazyReceivePersistentActor.Value;
    public IEventsourcedContext Eventsourced { get; }
    public IReceivePersistentActorContext ReceivePersistentActor { get; }
}