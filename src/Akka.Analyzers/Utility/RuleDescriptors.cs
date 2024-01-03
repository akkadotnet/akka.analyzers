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
        // TODO: need real help links for each rule code
        var helpLink = "https://getakka.net/";

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

    #endregion
    
    #region AK2000 Rules

    public static DiagnosticDescriptor Ak2000DoNotUseZeroTimeoutWithAsk { get; } = Rule("AK2000",
        "Do not use `Ask` with `TimeSpan.Zero` for timeout.", AnalysisCategory.ApiUsage, DiagnosticSeverity.Error,
        "When using `Ask`, you must always specify a timeout value greater than `TimeSpan.Zero`.");

    #endregion

}