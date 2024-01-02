// -----------------------------------------------------------------------
//  <copyright file="AkkaVerifier.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Akka.Analyzers.Tests.Utility;

[SuppressMessage("Design", "CA1000:Do not declare static members on generic types")]
public sealed class AkkaVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <summary>
    ///     Creates a diagnostic result for the diagnostic referenced in <see cref="TAnalyzer" />.
    /// </summary>
    public static DiagnosticResult Diagnostic()
    {
        return CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, DefaultVerifier>.Diagnostic();
    }

    public static Task VerifyAnalyzer(string source, params DiagnosticResult[] diagnostics)
    {
        return VerifyAnalyzer(new[] { source }, diagnostics);
    }

    public static Task VerifyAnalyzer(string[] sources, params DiagnosticResult[] diagnostics)
    {
        Guard.AssertIsNotNull(sources);

        var test = new AkkaTest();
#pragma warning disable CA1062
        foreach (var source in sources)
#pragma warning restore CA1062
            test.TestState.Sources.Add(source);

        test.ExpectedDiagnostics.AddRange(diagnostics);
        return test.RunAsync();
    }

    public static Task VerifyCodeFix(string before, string after, string fixerActionKey,
        params DiagnosticResult[] diagnostics)
    {
        Guard.AssertIsNotNull(before);
        Guard.AssertIsNotNull(after);

        var test = new AkkaTest
        {
            TestCode = before,
            FixedCode = after,
            CodeActionEquivalenceKey = fixerActionKey
        };
        test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
        return test.RunAsync();
    }

    private sealed class AkkaTest() : TestBase(ReferenceAssembliesHelper.CurrentAkka);

    private class TestBase : CSharpCodeFixTest<TAnalyzer, EmptyCodeFixProvider, DefaultVerifier>
    {
        protected TestBase(ReferenceAssemblies referenceAssemblies)
        {
            ReferenceAssemblies = referenceAssemblies;

            // Diagnostics are reported in both normal and generated code
            TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;

            // Tests that check for messages should run independent of current system culture.
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }

        protected override IEnumerable<CodeFixProvider> GetCodeFixProviders()
        {
            var analyzer = new TAnalyzer();

            foreach (var provider in CodeFixProviderDiscovery.GetCodeFixProviders(Language))
                if (analyzer.SupportedDiagnostics.Any(diagnostic =>
                        provider.FixableDiagnosticIds.Contains(diagnostic.Id)))
                    yield return provider;
        }
    }
}