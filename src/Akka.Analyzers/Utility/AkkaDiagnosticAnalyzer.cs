// -----------------------------------------------------------------------
//  <copyright file="AkkaDiagnosticAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

/// <summary>
/// Base class for all Akka.NET diagnostic analyzers.
/// </summary>
public abstract class AkkaDiagnosticAnalyzer(params DiagnosticDescriptor[] descriptors) : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = descriptors.ToImmutableArray();
    
    
    public abstract void AnalyzeCompilation(
        CompilationStartAnalysisContext context,
        AkkaContext akkaContext);
}