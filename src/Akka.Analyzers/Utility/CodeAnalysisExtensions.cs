// -----------------------------------------------------------------------
//  <copyright file="CodeAnalysisExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

internal static class CodeAnalysisExtensions
{
    // public static bool IsInsidePropsCreateMethod(this IOperation operation,
    //     AkkaContext akkaContext)
    // {
    //     Guard.AssertIsNotNull(operation);
    //     Guard.AssertIsNotNull(akkaContext);
    //
    //     if (akkaContext.AkkaCore.PropsType is null)
    //         return false;
    //
    //     var semanticModel = operation.SemanticModel;
    //     if (semanticModel is null)
    //         return false;
    // }

    public static bool IsActorBaseSubclass(this INamedTypeSymbol typeSymbol, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(typeSymbol);
        Guard.AssertIsNotNull(akkaContext);

        if (akkaContext.AkkaCore.ActorBaseType is null)
            return false;

        var currentBaseType = typeSymbol;
        while (currentBaseType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentBaseType, akkaContext.AkkaCore.ActorBaseType)) return true;
            currentBaseType = currentBaseType.BaseType;
        }

        return false;
    }
}