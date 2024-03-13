// -----------------------------------------------------------------------
//  <copyright file="ShouldUseIWithTimerInsteadOfITellScheduler.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Akka.Analyzers.Context.Core.Actor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShouldUseIWithTimersInsteadOfScheduleTellAnalyzer(): AkkaDiagnosticAnalyzer(RuleDescriptors.Ak1004ShouldUseIWithTimersInsteadOfScheduleTell)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);
        
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var invocationExpr = (InvocationExpressionSyntax)ctx.Node;
            var semanticModel = ctx.SemanticModel;
            
            var classDeclaration = invocationExpr.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration is null)
                return;
            var classBase = semanticModel.GetDeclaredSymbol(classDeclaration)?.BaseType;
            var coreContext = akkaContext.AkkaCore;
            
            // Check that the class declaration inherits from ActorBase
            if (classBase is null || !classBase.IsDerivedOrImplements(coreContext.Actor.ActorBaseType!))
                return;

            // invocation must be a member access expression
            if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccessExpr)
                return;
        
            // Get the member symbol from the invocation expression
            if(semanticModel.GetSymbolInfo(memberAccessExpr).Symbol is not IMethodSymbol methodSymbol)
                return;
        
            // Check if the method name is `ScheduleTellOnce` or `ScheduleTellRepeatedly`
            ArgumentSyntax? receiver = null;
            ArgumentSyntax? sender = null;
            var refSymbols = akkaContext.AkkaCore.Actor.ITellScheduler.ScheduleTellOnce;
            if (refSymbols.Any(s => ReferenceEquals(methodSymbol, s)))
            {
                receiver = invocationExpr.ArgumentList.Arguments[1];
                sender = invocationExpr.ArgumentList.Arguments[3];
            }
            else
            {
                refSymbols = akkaContext.AkkaCore.Actor.ITellScheduler.ScheduleTellRepeatedly;
                if (refSymbols.Any(s => ReferenceEquals(methodSymbol, s)))
                {
                    receiver = invocationExpr.ArgumentList.Arguments[2];
                    sender = invocationExpr.ArgumentList.Arguments[4];
                }
            }

            // Check that both receiver and sender is Self or if sender is Nobody or NoSender
            if (!IsReferenceToSelf(receiver, semanticModel, coreContext.Actor) || 
                !IsReferenceToSelfOrNobody(sender, semanticModel, coreContext.Actor))
                return;
        
            var diagnostic = Diagnostic.Create(
                descriptor: RuleDescriptors.Ak1004ShouldUseIWithTimersInsteadOfScheduleTell, 
                location: invocationExpr.GetLocation(),
                "ScheduleTell invocation");
            ctx.ReportDiagnostic(diagnostic);

        }, SyntaxKind.InvocationExpression);
    }

    private static bool IsReferenceToSelfOrNobody(ArgumentSyntax? argument, SemanticModel semanticModel, IAkkaCoreActorContext context)
    {
        if (argument is null)
            return false;

        var expression = argument.Expression;

        // null is considered as NoSender
        if (expression is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.NullLiteralExpression)
            return true;

        // argument must be an identifier
        var identifier = expression.DescendantNodesAndSelf(node => node is IdentifierNameSyntax).FirstOrDefault();
        if (identifier is null)
            return false;

        // Check for field symbols
        if (semanticModel.GetSymbolInfo(identifier).Symbol is IFieldSymbol fieldSymbol)
        {
            // Argument is `ActorRefs.Nobody`
            if (ReferenceEquals(fieldSymbol, context.ActorRefs.Nobody))
                return true;

            // Argument is `ActorRefs.NoSender`
            if (ReferenceEquals(fieldSymbol, context.ActorRefs.NoSender))
                return true;
        }
        
        // identifier must be a property
        if (semanticModel.GetSymbolInfo(identifier).Symbol is not IPropertySymbol propertySymbol)
            return false;

        // Argument is `ActorBase.Self`
        if (ReferenceEquals(propertySymbol, context.ActorBase.Self))
            return true;

        // Argument is `IActorRef.Self`
        if (ReferenceEquals(propertySymbol, context.IActorContext.Self))
            return true;

        return false;
    }
    
    private static bool IsReferenceToSelf(ArgumentSyntax? argument, SemanticModel semanticModel, IAkkaCoreActorContext context)
    {
        if (argument is null)
            return false;
        
        var expression = argument.Expression;
        
        // argument must be an identifier
        var identifier = expression.DescendantNodesAndSelf(node => node is IdentifierNameSyntax).FirstOrDefault();
        if (identifier is null)
            return false;
        
        // identifier must be a property
        if (semanticModel.GetSymbolInfo(identifier).Symbol is not IPropertySymbol propertySymbol)
            return false;

        // Argument is `ActorBase.Self`
        if (ReferenceEquals(propertySymbol, context.ActorBase.Self))
            return true;

        // Argument is `IActorRef.Self`
        if (ReferenceEquals(propertySymbol, context.IActorContext.Self))
            return true;

        return false;
    }
}