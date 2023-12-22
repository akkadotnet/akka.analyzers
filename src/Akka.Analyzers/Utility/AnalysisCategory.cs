// -----------------------------------------------------------------------
//  <copyright file="AnalysisCategory.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

public enum AnalysisCategory
{
    /// <summary>
    /// 1xxx
    /// </summary>
    ActorDesign,
}

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
        var helpLink = $"https://getakka.net/";

        return new DiagnosticDescriptor(id, title, messageFormat, category.ToString(), defaultSeverity,
            isEnabledByDefault: true, helpLinkUri: helpLink);
    }

    public static DiagnosticDescriptor Ak1000DoNotNewActors { get; } = Rule("Akka1000",
        "Do not use `new` to create actors", AnalysisCategory.ActorDesign, DiagnosticSeverity.Error,
        "Actors must be instantiated using `ActorOf` or `ActorOfAsTestActorRef` via a `Props` class.");

    public static DiagnosticDescriptor Ak1001CloseOverSenderUsingPipeTo { get; } = Rule("Akka1002",
        "Should always close over `Sender` when using `PipeTo`", AnalysisCategory.ActorDesign, DiagnosticSeverity.Error,
        "When using `PipeTo`, you must always close over `Sender` to ensure that the actor's `Sender` property " +
        "is captured at the time you're scheduling the `PipeTo`, as this value may change asynchronously.");
}