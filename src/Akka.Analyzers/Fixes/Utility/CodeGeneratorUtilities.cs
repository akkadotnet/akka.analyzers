// -----------------------------------------------------------------------
//  <copyright file="CodeGeneratorUtilities.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

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