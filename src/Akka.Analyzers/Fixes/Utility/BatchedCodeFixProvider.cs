// -----------------------------------------------------------------------
//  <copyright file="BatchedCodeFixProvider.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Akka.Analyzers.Fixes;

public abstract class BatchedCodeFixProvider(params string[] diagnostics) : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = diagnostics.ToImmutableArray();

    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }
}