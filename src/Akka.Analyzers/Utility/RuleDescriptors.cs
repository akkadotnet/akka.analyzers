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
    #endregion
    
    #region AK2000 Rules

    public static DiagnosticDescriptor Ak2000DoNotUseZeroTimeoutWithAsk { get; } = Rule("AK2000",
        "Do not use `Ask` with `TimeSpan.Zero` for timeout.", AnalysisCategory.ApiUsage, DiagnosticSeverity.Error,
        "When using `Ask`, you must always specify a timeout value greater than `TimeSpan.Zero`.");
    
    public static DiagnosticDescriptor Ak2001DoNotUseAutomaticallyHandledMessagesInShardMessageExtractor { get; } = Rule("AK2001",
        "Do not use automatically handled messages in inside `Akka.Cluster.Sharding.IMessageExtractor`s.", AnalysisCategory.ApiUsage, DiagnosticSeverity.Warning,
        "When using any implementation of `Akka.Cluster.Sharding.IMessageExtractor`, including `HashCodeMessageExtractor`, you should not use messages " +
        "that are automatically handled by Akka.NET such as `Shard.StartEntity` and `ShardingEnvelope`.");

    #endregion

}
