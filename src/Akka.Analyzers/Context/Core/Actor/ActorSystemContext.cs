// -----------------------------------------------------------------------
//  <copyright file="ActorSystemContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Context.Core.Actor;

public interface IActorSystemContext
{
    public IPropertySymbol? Settings { get; }
    public IPropertySymbol? Name { get; }
    public IPropertySymbol? Serialization { get; }
    public IPropertySymbol? EventStream { get; }
    public IPropertySymbol? DeadLetters { get; }
    public IPropertySymbol? IgnoreRef { get; }
    public IPropertySymbol? Dispatchers { get; }
    public IPropertySymbol? Mailboxes { get; }
    public IPropertySymbol? Scheduler { get; }
    public IPropertySymbol? Log { get; }
    public IPropertySymbol? StartTime { get; }
    public IPropertySymbol? Uptime { get; }
    public IPropertySymbol? WhenTerminated { get; }
    
    public ImmutableArray<IMethodSymbol> GetExtension { get; }
    public ImmutableArray<IMethodSymbol> HasExtension { get; }
    public ImmutableArray<IMethodSymbol> TryGetExtension { get; }
    public IMethodSymbol? RegisterOnTermination { get; }
    public IMethodSymbol? Terminate { get; }
    public IMethodSymbol? FinalTerminate { get; }
#pragma warning disable CA1716
    public IMethodSymbol? Stop { get; }
#pragma warning restore CA1716
    public ImmutableArray<IMethodSymbol> Dispose { get; }
    public IMethodSymbol? RegisterExtension { get; }
    public IMethodSymbol? ActorOf { get; }
    public ImmutableArray<IMethodSymbol> ActorSelection { get; }
}

public sealed class EmptyActorSystemContext : IActorSystemContext
{
    public static readonly IActorSystemContext Instance = new EmptyActorSystemContext();
    private EmptyActorSystemContext() { }
    
    public IPropertySymbol? Settings => null;
    public IPropertySymbol? Name => null;
    public IPropertySymbol? Serialization => null;
    public IPropertySymbol? EventStream => null;
    public IPropertySymbol? DeadLetters => null;
    public IPropertySymbol? IgnoreRef => null;
    public IPropertySymbol? Dispatchers => null;
    public IPropertySymbol? Mailboxes => null;
    public IPropertySymbol? Scheduler => null;
    public IPropertySymbol? Log => null;
    public IPropertySymbol? StartTime => null;
    public IPropertySymbol? Uptime => null;
    public IPropertySymbol? WhenTerminated => null;
    
    public ImmutableArray<IMethodSymbol> GetExtension => ImmutableArray<IMethodSymbol>.Empty;
    public ImmutableArray<IMethodSymbol> HasExtension => ImmutableArray<IMethodSymbol>.Empty;
    public ImmutableArray<IMethodSymbol> TryGetExtension => ImmutableArray<IMethodSymbol>.Empty;
    public IMethodSymbol? RegisterOnTermination => null;
    public IMethodSymbol? Terminate => null;
    public IMethodSymbol? FinalTerminate => null;
    public IMethodSymbol? Stop => null;
    public ImmutableArray<IMethodSymbol> Dispose => ImmutableArray<IMethodSymbol>.Empty;
    public IMethodSymbol? RegisterExtension => null;
    public IMethodSymbol? ActorOf => null;
    public ImmutableArray<IMethodSymbol> ActorSelection => ImmutableArray<IMethodSymbol>.Empty;
}

public sealed class ActorSystemContext : IActorSystemContext
{
    private readonly Lazy<IPropertySymbol> _lazySettings;
    private readonly Lazy<IPropertySymbol> _lazyName;
    private readonly Lazy<IPropertySymbol> _lazySerialization;
    private readonly Lazy<IPropertySymbol> _lazyEventStream;
    private readonly Lazy<IPropertySymbol> _lazyDeadLetters;
    private readonly Lazy<IPropertySymbol> _lazyIgnoreRef;
    private readonly Lazy<IPropertySymbol> _lazyDispatchers;
    private readonly Lazy<IPropertySymbol> _lazyMailboxes;
    private readonly Lazy<IPropertySymbol> _lazyScheduler;
    private readonly Lazy<IPropertySymbol> _lazyLog;
    private readonly Lazy<IPropertySymbol> _lazyStartTime;
    private readonly Lazy<IPropertySymbol> _lazyUptime;
    private readonly Lazy<IPropertySymbol> _lazyWhenTerminated;
    
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyGetExtension;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyHasExtension;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyTryGetExtension;
    private readonly Lazy<IMethodSymbol> _lazyRegisterOnTermination;
    private readonly Lazy<IMethodSymbol> _lazyTerminate;
    private readonly Lazy<IMethodSymbol> _lazyFinalTerminate;
    private readonly Lazy<IMethodSymbol> _lazyStop;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyDispose;
    private readonly Lazy<IMethodSymbol> _lazyRegisterExtension;
    private readonly Lazy<IMethodSymbol> _lazyActorOf;
    private readonly Lazy<ImmutableArray<IMethodSymbol>> _lazyActorSelection;
    
    private ActorSystemContext(IAkkaCoreActorContext context)
    {
        _lazySettings = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(Settings)).First());
        _lazyName = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(Name)).First());
        _lazySerialization = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(Serialization)).First());
        _lazyEventStream = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(EventStream)).First());
        _lazyDeadLetters = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(DeadLetters)).First());
        _lazyIgnoreRef = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(IgnoreRef)).First());
        _lazyDispatchers = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(Dispatchers)).First());
        _lazyMailboxes = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(Mailboxes)).First());
        _lazyScheduler = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(Scheduler)).First());
        _lazyLog = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(Log)).First());
        _lazyStartTime = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(StartTime)).First());
        _lazyUptime = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(Uptime)).First());
        _lazyWhenTerminated = new Lazy<IPropertySymbol>(() => (IPropertySymbol) context.ActorSystemType!
            .GetMembers(nameof(WhenTerminated)).First());
        
        _lazyGetExtension = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ActorSystemType!
            .GetMembers(nameof(GetExtension)).Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyHasExtension = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ActorSystemType!
            .GetMembers(nameof(HasExtension)).Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyTryGetExtension = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ActorSystemType!
            .GetMembers(nameof(TryGetExtension)).Select(m => (IMethodSymbol)m).ToImmutableArray());
        
        _lazyRegisterOnTermination = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorSystemType!
            .GetMembers(nameof(RegisterOnTermination)).First());
        _lazyTerminate = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorSystemType!
            .GetMembers(nameof(Terminate)).First());
        _lazyFinalTerminate = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorSystemType!
            .GetMembers(nameof(FinalTerminate)).First());
        _lazyStop = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorSystemType!
            .GetMembers(nameof(Stop)).First());
        
        _lazyDispose = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ActorSystemType!
            .GetMembers(nameof(Dispose)).Select(m => (IMethodSymbol)m).ToImmutableArray());
        _lazyRegisterExtension = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorSystemType!
            .GetMembers(nameof(RegisterExtension)).First());
        _lazyActorOf = new Lazy<IMethodSymbol>(() => (IMethodSymbol) context.ActorSystemType!
            .GetMembers(nameof(ActorOf)).First());
        _lazyActorSelection = new Lazy<ImmutableArray<IMethodSymbol>>(() => context.ActorSystemType!
            .GetMembers(nameof(ActorSelection)).Select(m => (IMethodSymbol)m).ToImmutableArray());
    }
    
    public IPropertySymbol? Settings => _lazySettings.Value;
    public IPropertySymbol? Name => _lazyName.Value;
    public IPropertySymbol? Serialization => _lazySerialization.Value;
    public IPropertySymbol? EventStream => _lazyEventStream.Value;
    public IPropertySymbol? DeadLetters => _lazyDeadLetters.Value;
    public IPropertySymbol? IgnoreRef => _lazyIgnoreRef.Value;
    public IPropertySymbol? Dispatchers => _lazyDispatchers.Value;
    public IPropertySymbol? Mailboxes => _lazyMailboxes.Value;
    public IPropertySymbol? Scheduler => _lazyScheduler.Value;
    public IPropertySymbol? Log => _lazyLog.Value;
    public IPropertySymbol? StartTime => _lazyStartTime.Value;
    public IPropertySymbol? Uptime => _lazyUptime.Value;
    public IPropertySymbol? WhenTerminated => _lazyWhenTerminated.Value;
    
    public ImmutableArray<IMethodSymbol> GetExtension => _lazyGetExtension.Value;
    public ImmutableArray<IMethodSymbol> HasExtension => _lazyHasExtension.Value;
    public ImmutableArray<IMethodSymbol> TryGetExtension => _lazyTryGetExtension.Value;
    public IMethodSymbol? RegisterOnTermination => _lazyRegisterOnTermination.Value;
    public IMethodSymbol? Terminate => _lazyTerminate.Value;
    public IMethodSymbol? FinalTerminate => _lazyFinalTerminate.Value;
    public IMethodSymbol? Stop => _lazyStop.Value;
    public ImmutableArray<IMethodSymbol> Dispose => _lazyDispose.Value;
    public IMethodSymbol? RegisterExtension => _lazyRegisterExtension.Value;
    public IMethodSymbol? ActorOf => _lazyActorOf.Value;
    public ImmutableArray<IMethodSymbol> ActorSelection => _lazyActorSelection.Value;
    
    public static ActorSystemContext Get(IAkkaCoreActorContext context)
        => new(context);
}