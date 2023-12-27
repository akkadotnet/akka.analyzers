// -----------------------------------------------------------------------
//  <copyright file="AkkaVerifier.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Akka.Analyzers.Tests.Utility;

// public class AkkaVerifier<TAnalyzer>
//     where TAnalyzer : DiagnosticAnalyzer, new()
// {
//     internal class TestBase<TVerifier> : CSharpCodeFixTest<TAnalyzer, EmptyCodeFixProvider, TVerifier>
//         where TVerifier : IVerifier, new()
//     {
//         protected TestBase(
//             LanguageVersion languageVersion,
//             ReferenceAssemblies referenceAssemblies)
//         {
//             LanguageVersion = languageVersion;
//             ReferenceAssemblies = referenceAssemblies;
//
//             // Diagnostics are reported in both normal and generated code
//             TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;
//
//             // Tests that check for messages should run independent of current system culture.
//             CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
//         }
//
//         public LanguageVersion LanguageVersion { get; }
//
//         protected override ParseOptions CreateParseOptions() =>
//             new CSharpParseOptions(LanguageVersion, DocumentationMode.Diagnose);
//     }
//     
//     internal sealed class AkkaTest : TestBase<XUnitVerifier>
//     {
//         public AkkaTest(LanguageVersion languageVersion) : base(languageVersion, referenceAssemblies)
//         {
//         }
//     }
// }