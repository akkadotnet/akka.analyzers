// -----------------------------------------------------------------------
//  <copyright file="ResolverTestHarness.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Akka.Analyzers.Tests.Utility;

/// <summary>
/// Constructs an in-memory <see cref="CSharpCompilation"/> with Akka.NET references resolved
/// via the same <see cref="ReferenceAssembliesHelper.CurrentAkka"/> the analyzer tests use.
/// Provides direct access to <see cref="SemanticModel"/> + <see cref="SyntaxTree"/> for
/// utility-level tests that want to call resolver/extension methods without going through
/// <see cref="AkkaVerifier{TAnalyzer}"/>.
/// </summary>
public static class ResolverTestHarness
{
    public static async Task<(Compilation Compilation, SyntaxTree Tree, SemanticModel SemanticModel, AkkaContext AkkaContext)>
        CompileAsync(string source)
    {
        var refs = await ReferenceAssembliesHelper.CurrentAkka
            .ResolveAsync(LanguageNames.CSharp, CancellationToken.None);

        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: "ResolverTests",
            syntaxTrees: new[] { tree },
            references: refs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var akkaContext = new AkkaContext(compilation);
        return (compilation, tree, compilation.GetSemanticModel(tree), akkaContext);
    }

    /// <summary>
    /// Find the first invocation expression in <paramref name="tree"/> whose simple method-name
    /// identifier matches <paramref name="methodName"/>. Useful for tests that target a specific
    /// receive-handler registration call.
    /// </summary>
    public static InvocationExpressionSyntax FindInvocation(SyntaxTree tree, string methodName)
    {
        return tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>()
            .First(inv => GetSimpleName(inv.Expression) == methodName);
    }

    private static string? GetSimpleName(ExpressionSyntax expr) => expr switch
    {
        IdentifierNameSyntax id => id.Identifier.Text,
        GenericNameSyntax gen => gen.Identifier.Text,
        MemberAccessExpressionSyntax m => GetSimpleName(m.Name),
        _ => null,
    };
}
