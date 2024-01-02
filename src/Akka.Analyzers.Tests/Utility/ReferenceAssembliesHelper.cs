// -----------------------------------------------------------------------
//  <copyright file="ReferenceAssembliesHelper.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis.Testing;

namespace Akka.Analyzers.Tests.Utility;

/// <summary>
///     Used to help us load the correct reference assemblies for the versions of Akka.NET being tested.
/// </summary>
/// <remarks>
///     When making changes to any of these assemblies, make sure to update the 'PackageDownload' elements
///     inside of the 'Akka.Analyzers.Tests.csproj' file.
/// </remarks>
internal static class ReferenceAssembliesHelper
{
    public static readonly ReferenceAssemblies CurrentAkka;

#pragma warning disable CA1810
    static ReferenceAssembliesHelper()
#pragma warning restore CA1810
    {
        // Can't use ReferenceAssemblies.Net.Net80 because it's too new for Microsoft.CodeAnalysis 4.2.0
        var defaultAssemblies =
            new ReferenceAssemblies(
                "net8.0",
                new PackageIdentity("Microsoft.NETCore.App.Ref", "8.0.0"),
                Path.Combine("ref", "net8.0")
            );

        CurrentAkka = defaultAssemblies.AddPackages(
            [new PackageIdentity("Akka", "1.5.14")]
        );
    }
}