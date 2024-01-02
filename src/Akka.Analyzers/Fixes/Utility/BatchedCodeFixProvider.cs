using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Akka.Analyzers.Fixes;

public abstract class BatchedCodeFixProvider(params string[] diagnostics) : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = diagnostics.ToImmutableArray();

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;
}
