// -----------------------------------------------------------------------
//  <copyright file="MustNotHandleISystemMessageInsideTellInternalAnalyzer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustNotHandleISystemMessageInsideTellInternalAnalyzer() 
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak2006MustNotHandleISystemMessageInsideTellInternal)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        return;

        void AnalyzeMethod(SyntaxNodeAnalysisContext ctx)
        {
            var methodDecl = (MethodDeclarationSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDecl);
            if (methodSymbol is null)
                return;
            
            var akkaCore = akkaContext.AkkaCore;
            var actorRefBaseContext = akkaCore.Actor.ActorRefBase;
            var tellInternalSymbol = actorRefBaseContext.TellInternal;

            // Check if this method overrides ActorRefBase.TellInternal
            if (!methodSymbol.Overrides(tellInternalSymbol!))
                return;
            
            var iSystemMessageType = akkaContext.AkkaCore.Dispatch.ISystemMessageType;
            if (iSystemMessageType == null)
                return;

            // Use the existing IsDerivedOrImplements extension for type hierarchy
            // Check all operations in the method body
            var operation = semanticModel.GetOperation(methodDecl);
            if (operation == null)
                return;

            foreach (var descendant in operation.DescendantsAndSelf())
            {
                switch (descendant)
                {
                    // Cast: (ISystemMessage)message
                    case IConversionOperation conv when conv.Operand is IParameterReferenceOperation param && param.Parameter.Name == "message":
                        if (conv.Type != null && conv.Type.IsDerivedOrImplements(iSystemMessageType))
                        {
                            ctx.ReportDiagnostic(Diagnostic.Create(RuleDescriptors.Ak2006MustNotHandleISystemMessageInsideTellInternal, conv.Syntax.GetLocation()));
                        }
                        break;
                    // is/as: message is ISystemMessage, message as ISystemMessage
                    case IIsTypeOperation isType when isType.ValueOperand is IParameterReferenceOperation param && param.Parameter.Name == "message":
                        if (isType.TypeOperand != null && isType.TypeOperand.IsDerivedOrImplements(iSystemMessageType))
                        {
                            ctx.ReportDiagnostic(Diagnostic.Create(RuleDescriptors.Ak2006MustNotHandleISystemMessageInsideTellInternal, isType.Syntax.GetLocation()));
                        }
                        break;
                    case ITypeOfOperation typeOfOp:
                        // Not relevant
                        break;
                    case ITypePatternOperation typePattern:
                        if (typePattern.MatchedType.IsDerivedOrImplements(iSystemMessageType))
                        {
                            ctx.ReportDiagnostic(Diagnostic.Create(RuleDescriptors.Ak2006MustNotHandleISystemMessageInsideTellInternal, typePattern.Syntax.GetLocation()));
                        }
                        break;
                    // Switch/case: case ISystemMessage msg:
                    case IDeclarationPatternOperation { MatchedType: not null } declPattern:
                        if (declPattern.MatchedType.IsDerivedOrImplements(iSystemMessageType))
                        {
                            ctx.ReportDiagnostic(Diagnostic.Create(RuleDescriptors.Ak2006MustNotHandleISystemMessageInsideTellInternal, declPattern.Syntax.GetLocation()));
                        }
                        break;
                }
            }
        }
    }
} 
