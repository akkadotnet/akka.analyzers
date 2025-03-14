// -----------------------------------------------------------------------
//  <copyright file="ShouldNotCallContextMaterializerMultipleTimes.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ShouldNotInvokeContextMaterializerMultipleTimesAnalyzer()
    : AkkaDiagnosticAnalyzer(RuleDescriptors.Ak2002ShouldNotCallContextMaterializerMultipleTimes)
{
    public override void AnalyzeCompilation(CompilationStartAnalysisContext context, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(context);
        Guard.AssertIsNotNull(akkaContext);

        context.RegisterSyntaxNodeAction(ctx =>
        {
            // No need to check if Akka.Streams is not installed
            if (!akkaContext.HasAkkaStreamsInstalled || 
                akkaContext.AkkaCore.Actor.ActorBaseType is null ||
                akkaContext.AkkaStreams.ActorMaterializerExtensions is null)
                return;
            
            var classDeclaration = (ClassDeclarationSyntax)ctx.Node;

            // Obtain the symbol for the class declaration.
            var classSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (classSymbol is null)
                return;
            
            // Check if the class (or one of its base types) is Akka.Actor.ActorBase.
            if(!classSymbol.IsDerivedOrImplements(akkaContext.AkkaCore.Actor.ActorBaseType))
                return;
            
            var materializerMethod = akkaContext.AkkaStreams.ActorMaterializerExtensions.Materializer;
            var iActorContextType = akkaContext.AkkaCore.Actor.IActorContextType;
            var actorMaterializerExtensionsType = akkaContext.AkkaStreams.ActorMaterializerExtensionsType;
            
            // Search for all invocation expressions within the class declaration.
            var materializerInvocationCount = 0;
            var invocations = classDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invocation in invocations)
            {
                var symbolInfo = ctx.SemanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                    continue;

                if (!methodSymbol.IsExtensionMethod)
                    continue;
                
                var isReduced = false;
                while (methodSymbol.ReducedFrom != null)
                {
                    methodSymbol = methodSymbol.ReducedFrom;
                    isReduced = true;
                }
                
                // Check if this invocation is for the extension method Materializer()
                // defined in Akka.Streams.ActorMaterializerExtensions.
                if (!ReferenceEquals(methodSymbol, materializerMethod))
                    continue;

                ITypeSymbol? receiverType;
                // if it's not a reduced form, we expect that it is a static function call
                if (!isReduced)
                {
                    // In static form, the first argument is the "this" parameter and the "receiver".
                    var firstArg = invocation.ArgumentList.Arguments[0];
                    receiverType = ctx.SemanticModel.GetTypeInfo(firstArg.Expression).Type;
                }
                else
                {
                    // in reduced type, the "receiver" is the accessed member type
                    if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                        continue;

                    receiverType = ctx.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
                }
                
                // If no receiver type could be determined, skip.
                if (receiverType is null)
                    continue;

                // If receiver type is not IActorContext, skip.
                // Only IActorContext (ActorCell and its brethren) is problematic
                if (!receiverType.IsDerivedOrImplements(iActorContextType!))
                    continue;
                
                materializerInvocationCount++;
                    
                // Report a diagnostic if more than one invocation is found.
                if (materializerInvocationCount > 1)
                {
                    var diagnostic = Diagnostic.Create(
                        RuleDescriptors.Ak2002ShouldNotCallContextMaterializerMultipleTimes,
                        invocation.GetLocation());
                    ctx.ReportDiagnostic(diagnostic);
                }
            }
        }, SyntaxKind.ClassDeclaration);
    }
}