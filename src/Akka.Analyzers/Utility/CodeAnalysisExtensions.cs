// -----------------------------------------------------------------------
//  <copyright file="CodeAnalysisExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

internal static class CodeAnalysisExtensions
{
    public static bool IsInsidePropsCreateMethod(this IOperation operation,
        AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(operation);
        Guard.AssertIsNotNull(akkaContext);

        if (akkaContext.AkkaCore.PropsType is null)
            return false;

        var semanticModel = operation.SemanticModel;
        if (semanticModel is null)
            return false;
    }
}