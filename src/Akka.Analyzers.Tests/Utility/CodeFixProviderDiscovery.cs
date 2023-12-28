using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.VisualStudio.Composition;

namespace Akka.Analyzers.Tests.Utility;

internal static class CodeFixProviderDiscovery
{
    private static readonly Lazy<IExportProviderFactory> ExportProviderFactory = new(
        () =>
        {
            var discovery = new AttributedPartDiscovery(Resolver.DefaultInstance, isNonPublicSupported: true);
            var parts = Task.Run(() => discovery.CreatePartsAsync(typeof(CodeAnalysisExtensions).Assembly)).GetAwaiter()
                .GetResult();
            var catalog = ComposableCatalog.Create(Resolver.DefaultInstance).AddParts(parts);

            var configuration = CompositionConfiguration.Create(catalog);
            var runtimeComposition = RuntimeComposition.CreateRuntimeComposition(configuration);
            return runtimeComposition.CreateExportProviderFactory();
        },
        LazyThreadSafetyMode.ExecutionAndPublication
    );

    public static IEnumerable<CodeFixProvider> GetCodeFixProviders(string language)
    {
        var exportProvider = ExportProviderFactory.Value.CreateExportProvider();
        var exports = exportProvider.GetExports<CodeFixProvider, LanguageMetadata>();

        return exports.Where(export => export.Metadata.Languages.Contains(language)).Select(export => export.Value);
    }

    #pragma warning disable CA1812 // internal class that is apparently never instantiated.
    sealed class LanguageMetadata
    {
        public LanguageMetadata(IDictionary<string, object> data)
        {
            if (!data.TryGetValue(nameof(ExportCodeFixProviderAttribute.Languages), out var languages))
                languages = Array.Empty<string>();

            Languages = ((string[])languages).ToImmutableArray();
        }

        public ImmutableArray<string> Languages { get; }
    }
    #pragma warning restore CA1812 // internal class that is apparently never instantiated.
}