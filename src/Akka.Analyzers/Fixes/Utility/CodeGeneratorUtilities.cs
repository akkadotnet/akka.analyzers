// -----------------------------------------------------------------------
//  <copyright file="CodeGeneratorUtilities.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Akka.Analyzers.Fixes;

internal static class CodeGeneratorUtilities
{
    public static LocalDeclarationStatementSyntax IntroduceLocalVariableStatement(
        string parameterName,
        ExpressionSyntax replacementNode)
    {
        var equalsToReplacementNode = EqualsValueClause(replacementNode);

        var oneItemVariableDeclaration = VariableDeclaration(
            ParseTypeName("var"),
            SingletonSeparatedList(
                VariableDeclarator(Identifier(parameterName))
                    .WithInitializer(equalsToReplacementNode)
            )
        );

        return LocalDeclarationStatement(oneItemVariableDeclaration);
    }
}