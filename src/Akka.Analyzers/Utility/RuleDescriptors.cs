// -----------------------------------------------------------------------
//  <copyright file="RuleDescriptors.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

public static class RuleDescriptors
{
    private static DiagnosticDescriptor Rule(
        string id,
        string title,
        AnalysisCategory category,
        DiagnosticSeverity defaultSeverity,
        string messageFormat)
    {
        var helpLink = "https://getakka.net/articles/debugging/rules/" + id.ToUpperInvariant() + ".html";

        return new DiagnosticDescriptor(id, title, messageFormat, category.ToString(), defaultSeverity,
            true, helpLinkUri: helpLink);
    }

    #region AK1000 Rules

    public static DiagnosticDescriptor Ak1000DoNotNewActors { get; } = Rule("AK1000",
        "Do not use `new` to create actors", AnalysisCategory.ActorDesign, DiagnosticSeverity.Error,
        "Actors must be instantiated using `ActorOf` or `ActorOfAsTestActorRef` via a `Props` class.");

    public static DiagnosticDescriptor Ak1001CloseOverSenderUsingPipeTo { get; } = Rule("AK1001",
        "Should always close over `Sender` when using `PipeTo`", AnalysisCategory.ActorDesign, DiagnosticSeverity.Error,
        "When using `PipeTo`, you must always close over `Sender` to ensure that the actor's `Sender` property " +
        "is captured at the time you're scheduling the `PipeTo`, as this value may change asynchronously.");

    public static DiagnosticDescriptor Ak1002DoNotAwaitOnGracefulStop { get; } = Rule(
        id: "AK1002",
        title: "Should never await on Self.GracefulStop() inside ReceiveAsync()", 
        category: AnalysisCategory.ActorDesign, 
        defaultSeverity: DiagnosticSeverity.Error,
        messageFormat: "Do not await on `Self.GracefulStop()` inside `ReceiveAsync()` because this will lead into " +
                       "a deadlock inside the `ReceiveAsync()` and the actor will never receive the `PoisonPill` message sent by `GracefulStop` while it's `await`-ing.");
    
    public static DiagnosticDescriptor Ak1003ShouldNotUseReceiveAsyncSynchronously { get; } = Rule(
        id: "AK1003",
        title: "ReceiveAsync<T>() or ReceiveAnyAsync<T>() message handler without async lambda body", 
        category: AnalysisCategory.ActorDesign, 
        defaultSeverity: DiagnosticSeverity.Warning,
        messageFormat: "ReceiveAsync<T>() or ReceiveAnyAsync() message handler with synchronous code body or " +
                       "block is less performant compared to Receive<T>() or ReceiveAny(). " +
                       "Consider changing this message handler to Receive<T>() or ReceiveAny() instead.");
    
    public static DiagnosticDescriptor Ak1004ShouldUseIWithTimersInsteadOfScheduleTell { get; } = Rule(
        id: "AK1004",
        title: "ScheduleTellOnce() and ScheduleTellRepeatedly() can cause memory leak if not properly canceled", 
        category: AnalysisCategory.ActorDesign, 
        defaultSeverity: DiagnosticSeverity.Warning,
        messageFormat: "Usage of ScheduleTellOnce() and ScheduleTellRepeatedly() inside an Akka actor, " +
                       "especially the variant that does not accept an ICancelable parameter, " +
                       "can cause memory leak and unnecessary CPU usage if they are not canceled properly inside PostStop(). " +
                       "Consider implementing the IWithTimers interface and use the Timers.StartSingleTimer() or " +
                       "Timers.StartPeriodicTimer() instead.");

    public static DiagnosticDescriptor Ak1005MustCloseOverSenderWhenUsedInsideLambdaArgument { get; } = Rule(
        id: "AK1005",
        title: "Must close over `Sender` or `Self`", 
        category: AnalysisCategory.ActorDesign, 
        defaultSeverity: DiagnosticSeverity.Warning,
        messageFormat: "When accessing `{0}` inside a lambda expression passed as an asynchronous method argument, " +
                       "you must always close over `{0}` to ensure that the `{0}` property is captured before the " +
                       "method is invoked, because there is no guarantee that asynchronous context will be preserved " +
                       "when the lambda is invoked inside the method.");
    
    public static DiagnosticDescriptor Ak1006ShouldNotUsePersistInsideLoop { get; } = Rule(
        id: "AK1006",
        title: "Should not call `Persist` inside a loop", 
        category: AnalysisCategory.ActorDesign, 
        defaultSeverity: DiagnosticSeverity.Warning,
        messageFormat: "Calling `{0}()` inside a loop is discouraged as it is non-performant. Collect all of your " +
                       "changes inside the loop and then call `{1}()` after the loop instead.");
    
    public static DiagnosticDescriptor Ak1007MustNotUseIWithTimersInPreRestart { get; } = Rule(
        id: "AK1007",
        title: "Timers.StartSingleTimer() and Timers.StartPeriodicTimer() must not be used inside AroundPreRestart() or PreRestart()", 
        category: AnalysisCategory.ActorDesign, 
        defaultSeverity: DiagnosticSeverity.Error,
        messageFormat: "Creating timer registration using `{0}()` in `{1}()` will not be honored because they will be " +
                       "cleared immediately. Move timer creation to `PostRestart()` instead.");
    
    public static DiagnosticDescriptor Ak1008ShouldNotUseSystemToCreateChildActor { get; } = Rule(
        id: "AK1008",
        title: "Creating actors using `ActorSystem.ActorOf()` inside an actor.", 
        category: AnalysisCategory.ActorDesign, 
        defaultSeverity: DiagnosticSeverity.Warning,
        messageFormat: "Creating actors using `ActorSystem.ActorOf` inside an actor is discouraged because the " +
                       "resulting actor would not be the child of this actor but the system itself. Please use " +
                       "`ActorContext.ActorOf` if your intention is to create a child actor of this actor.");
    #endregion
    
    #region AK2000 Rules

    public static DiagnosticDescriptor Ak2000DoNotUseZeroTimeoutWithAsk { get; } = Rule("AK2000",
        "Do not use `Ask` with `TimeSpan.Zero` for timeout.", AnalysisCategory.ApiUsage, DiagnosticSeverity.Error,
        "When using `Ask`, you must always specify a timeout value greater than `TimeSpan.Zero`.");
    
    public static DiagnosticDescriptor Ak2001DoNotUseAutomaticallyHandledMessagesInShardMessageExtractor { get; } = Rule("AK2001",
        "Do not use automatically handled messages in inside `Akka.Cluster.Sharding.IMessageExtractor`s.", AnalysisCategory.ApiUsage, DiagnosticSeverity.Warning,
        "When using any implementation of `Akka.Cluster.Sharding.IMessageExtractor`, including `HashCodeMessageExtractor`, you should not use messages " +
        "that are automatically handled by Akka.NET such as `Shard.StartEntity` and `ShardingEnvelope`.");

    public static DiagnosticDescriptor Ak2002ShouldNotCallContextMaterializerMultipleTimes { get; } = Rule(
        id: "AK2002",
        title: "Should not invoke Context.Materializer() multiple times.", 
        category: AnalysisCategory.ApiUsage, 
        defaultSeverity: DiagnosticSeverity.Warning,
        messageFormat: "Context.Materializer() should not be invoked multiple times, use a cached value instead.");

    public static DiagnosticDescriptor Ak2003MustNotUseVoidAsyncDelegateInReceiveActorReceive { get; } = Rule(
        id: "AK2003",
        title: "ReceiveActor `Receive` message handler must not be a void async delegate", 
        category: AnalysisCategory.ApiUsage, 
        defaultSeverity: DiagnosticSeverity.Error,
        messageFormat: "ReceiveActor `Receive` message handler delegate must not return a `Task` or be a void async delegate. Use `ReceiveAsync` instead.");

    public static DiagnosticDescriptor Ak2004MustNotUseVoidAsyncDelegateInDslActorReceive { get; } = Rule(
        id: "AK2004",
        title: "DslActor `Receive` message handler must not be a void async delegate", 
        category: AnalysisCategory.ApiUsage, 
        defaultSeverity: DiagnosticSeverity.Error,
        messageFormat: "DslActor `Receive` message handler delegate must not return a `Task` or be a void async delegate. Use `ReceiveAsync` instead.");

    public static DiagnosticDescriptor Ak2005MustNotUseVoidAsyncDelegateInReceivePersistentActorCommand { get; } = Rule(
        id: "AK2005",
        title: "ReceivePersistentActor `Command` message handler must not be a void async delegate", 
        category: AnalysisCategory.ApiUsage, 
        defaultSeverity: DiagnosticSeverity.Error,
        messageFormat: "ReceivePersistentActor `Command` message handler delegate must not return a `Task` or be a void async delegate. Use `CommandAsync` instead.");
    
    #endregion

}
