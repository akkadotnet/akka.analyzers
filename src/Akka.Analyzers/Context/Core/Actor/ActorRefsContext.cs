// -----------------------------------------------------------------------
//  <copyright file="ActorRefsContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IActorRefsContext
{
    public IFieldSymbol? Nobody { get; }
    public IFieldSymbol? NoSender { get; }
}

public sealed class EmptyActorRefsContext : IActorRefsContext
{
    public static readonly EmptyActorRefsContext Empty = new();
    
    private EmptyActorRefsContext() { }
    
    public IFieldSymbol? Nobody => null;
    public IFieldSymbol? NoSender => null;
}

public sealed class ActorRefsContext : IActorRefsContext
{
    private readonly Lazy<IFieldSymbol> _lazyNobody;
    private readonly Lazy<IFieldSymbol> _lazyNoSender;

    private ActorRefsContext(IAkkaCoreActorContext context)
    {
        _lazyNobody = new Lazy<IFieldSymbol>(() => (IFieldSymbol) context.ActorRefsType!
            .GetMembers("Nobody").First());
        _lazyNoSender = new Lazy<IFieldSymbol>(() => (IFieldSymbol) context.ActorRefsType!
            .GetMembers("NoSender").First());
    }

    public IFieldSymbol? Nobody => _lazyNobody.Value;
    public IFieldSymbol? NoSender => _lazyNoSender.Value;

    public static ActorRefsContext Get(IAkkaCoreActorContext context)
        => new(context);
}