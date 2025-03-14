// -----------------------------------------------------------------------
//  <copyright file="ActorMaterializerExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Streams;

public interface IActorMaterializerExtensionsContext
{
    IMethodSymbol? Materializer { get; }
}

public sealed class EmptyActorMaterializerExtensionsContext : IActorMaterializerExtensionsContext
{
    public static EmptyActorMaterializerExtensionsContext Instance { get; } = new();
    
    private EmptyActorMaterializerExtensionsContext() { }
    
    public IMethodSymbol? Materializer => null;
}


public class ActorMaterializerExtensionsContext: IActorMaterializerExtensionsContext
{
    private Lazy<IMethodSymbol> _lazyMaterializer;
    
    private ActorMaterializerExtensionsContext(AkkaStreamsContext context)
    {
        _lazyMaterializer = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorMaterializerExtensionsType!.GetMembers("Materializer").First());
    }

    public IMethodSymbol? Materializer => _lazyMaterializer.Value;
    
    public static ActorMaterializerExtensionsContext Get(AkkaStreamsContext context)
        => new(context);
    
}