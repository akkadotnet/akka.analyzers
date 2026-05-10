// -----------------------------------------------------------------------
//  <copyright file="ActorHandlerMethodCache.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Akka.Analyzers;

/// <summary>
/// Compilation-scoped cache of method symbols that appear as method-group arguments to actor
/// handler-registration calls (<c>ReceiveAsync(SomeMethod)</c>, <c>CommandAsync(SomeMethod)</c>,
/// <c>RunTask(SomeMethod)</c>, etc.).
/// </summary>
/// <remarks>
/// Used by analyzers that need to answer the inverse of what <see cref="ActorHandlerResolver"/>
/// answers: given a method body, "is this method bound as a handler somewhere in the compilation?"
///
/// <para>The cache is built lazily — the compilation walk only runs when the first call to
/// <see cref="Contains"/> happens. Analyzers that never reach a candidate site never pay the cost.</para>
///
/// <para><b>Population:</b> walks every <see cref="InvocationExpressionSyntax"/> in the compilation
/// and feeds it through <see cref="ActorHandlerResolver.Resolve"/>. For each binding with
/// <see cref="ActorHandlerBindingKind.MethodGroup"/>, the resolved <see cref="IMethodSymbol"/> is
/// normalized to its <see cref="ISymbol.OriginalDefinition"/> and added to the set. Lambdas and
/// anonymous methods are not added — their bodies live at the registration site and are caught by
/// the existing lambda-ancestor walk in each analyzer.</para>
///
/// <para><b>Lifetime:</b> one instance per <see cref="Compilation"/>. Each compilation pass creates
/// a fresh cache (via <see cref="CompilationStartAnalysisContext"/>) so there is no risk of stale
/// data across builds.</para>
///
/// <para><b>Cost:</b> the walk visits every invocation in every syntax tree once. For a project
/// with N actor files and M total invocations, this is O(M) syntax-node traversals plus O(K)
/// semantic resolutions where K is the number of receive-handler invocations. Typically K &lt;&lt; M.
/// In practice, building the cache costs ~100–500ms for medium projects on cold start.</para>
/// </remarks>
public sealed class ActorHandlerMethodCache
{
    private readonly Lazy<HashSet<IMethodSymbol>> _lazyHandlers;

    private ActorHandlerMethodCache(Compilation compilation, AkkaContext akkaContext)
    {
        _lazyHandlers = new Lazy<HashSet<IMethodSymbol>>(
            () => Build(compilation, akkaContext),
            isThreadSafe: true);
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="method"/> is bound as a method-group argument to a
    /// receive-handler registration call somewhere in the compilation. Comparison normalizes via
    /// <see cref="ISymbol.OriginalDefinition"/>.
    /// </summary>
    public bool Contains(IMethodSymbol method)
    {
        Guard.AssertIsNotNull(method);
        return _lazyHandlers.Value.Contains(method.OriginalDefinition);
    }

    /// <summary>
    /// Number of distinct method symbols in the cache. Forces materialization. Intended for tests
    /// and diagnostics, not hot-path use.
    /// </summary>
    public int Count => _lazyHandlers.Value.Count;

    public static ActorHandlerMethodCache Create(Compilation compilation, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(compilation);
        Guard.AssertIsNotNull(akkaContext);
        return new ActorHandlerMethodCache(compilation, akkaContext);
    }

    private static HashSet<IMethodSymbol> Build(Compilation compilation, AkkaContext akkaContext)
    {
        // RS1024 false-positives on Roslyn 3.11 against the standard HashSet+SymbolEqualityComparer
        // construction pattern. This is the recommended pattern from the Roslyn team.
#pragma warning disable RS1024
        var set = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024
        foreach (var tree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(tree);
            foreach (var invocation in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                foreach (var binding in ActorHandlerResolver.Resolve(invocation, semanticModel, akkaContext))
                {
                    if (binding.Kind == ActorHandlerBindingKind.MethodGroup && binding.MethodGroup is { } method)
                        set.Add(method.OriginalDefinition);
                }
            }
        }
        return set;
    }
}
