// -----------------------------------------------------------------------
//  <copyright file="DslContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor.Dsl;

public interface IDslContext
{
    public INamedTypeSymbol? ActType { get; }
    public INamedTypeSymbol? IActorDslType { get; }
    public IActContext Act { get; }
    public IIActorDslContext IActorDsl { get; }
}

public class EmptyDslContext: IDslContext
{
    public static readonly EmptyDslContext Instance = new ();
    
    private EmptyDslContext() { }
    
    public INamedTypeSymbol? ActType => null;
    public INamedTypeSymbol? IActorDslType => null;
    
    public IActContext Act => EmptyActContext.Instance;
    public IIActorDslContext IActorDsl => EmptyIActorDslContext.Instance;
}

public class DslContext: IDslContext
{
    public const string Namespace = ActorSymbolFactory.AkkaActorNamespace + ".Dsl";
    
    private readonly Lazy<INamedTypeSymbol?> _lazyActType;
    private readonly Lazy<INamedTypeSymbol?> _lazyIActorDslType;

    public DslContext(Compilation compilation)
    {
        _lazyActType = new Lazy<INamedTypeSymbol?>(() => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{Namespace}.Act"));
        _lazyIActorDslType = new Lazy<INamedTypeSymbol?>(() => Guard.AssertIsNotNull(compilation)
            .GetTypeByMetadataName($"{Namespace}.IActorDsl"));

        Act = ActContext.Get(this);
        IActorDsl = IActorDslContext.Get(this);
    }

    public INamedTypeSymbol? ActType => _lazyActType.Value;
    public INamedTypeSymbol? IActorDslType => _lazyIActorDslType.Value;
    public IActContext Act { get; }
    public IIActorDslContext IActorDsl { get; }

    public static IDslContext Get(Compilation compilation)
        => new DslContext(compilation);
}