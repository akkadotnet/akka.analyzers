// -----------------------------------------------------------------------
//  <copyright file="AkkaDiagnosticAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

/// <summary>
///     Base class for all Akka.NET diagnostic analyzers.
/// </summary>
public abstract class AkkaDiagnosticAnalyzer(params DiagnosticDescriptor[] descriptors) : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = descriptors.ToImmutableArray();

    /// <summary>
    ///     Analyzes compilation to discover diagnostics.
    /// </summary>
    /// <param name="context">The Roslyn diagnostic context</param>
    /// <param name="akkaContext">The Akka.NET context</param>
    public abstract void AnalyzeCompilation(
        CompilationStartAnalysisContext context,
        AkkaContext akkaContext);

    protected virtual AkkaContext CreateAkkaContext(Compilation compilation)
    {
        return new AkkaContext(compilation);
    }

    /// <inheritdoc />
    public sealed override void Initialize(AnalysisContext context)
    {
        Guard.AssertIsNotNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                               GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(ctx =>
        {
            var akkaContext = CreateAkkaContext(ctx.Compilation);
            if (ShouldAnalyze(akkaContext))
                AnalyzeCompilation(ctx, akkaContext);
        });
    }

    /// <summary>
    ///     Override this method to influence when we should consider diagnostic analysis. By
    ///     default analyzes all assemblies that have a reference to the core Akka.NET dll.
    /// </summary>
    /// <param name="akkaContext">The Akka.NET context</param>
    /// <returns>Return <c>true</c> to analyze source; return <c>false</c> to skip analysis</returns>
    /// <remarks>
    ///     This could be extended to do things like apply rules that apply to Akka.Hosting, Akka.Cluster, etc..
    /// </remarks>
    protected virtual bool ShouldAnalyze(AkkaContext akkaContext)
    {
        return Guard.AssertIsNotNull(akkaContext).HasAkkaInstalled;
    }
}