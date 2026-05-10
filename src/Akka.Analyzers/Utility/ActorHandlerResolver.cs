// -----------------------------------------------------------------------
//  <copyright file="ActorHandlerResolver.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Akka.Analyzers;

/// <summary>
/// Resolves the delegate handler(s) bound by an actor handler-registration invocation.
/// </summary>
/// <remarks>
/// Recognizes the following <b>handler-registration methods</b>:
/// <list type="bullet">
///   <item><description><c>ReceiveActor.Receive</c>, <c>ReceiveAny</c>, <c>ReceiveAsync</c>, <c>ReceiveAnyAsync</c></description></item>
///   <item><description><c>ReceivePersistentActor.Command</c>, <c>CommandAny</c>, <c>CommandAsync</c>, <c>CommandAnyAsync</c>, <c>Recover</c>, <c>RecoverAny</c></description></item>
///   <item><description><c>UntypedActor.RunTask</c> (both <c>Action</c> and <c>Func&lt;Task&gt;</c> overloads)</description></item>
/// </list>
///
/// For each recognized invocation, returns one <see cref="ActorHandlerBinding"/> per delegate-typed
/// argument. Predicate arguments (e.g. the <c>shouldHandle</c> parameter on <c>Receive&lt;T&gt;</c> overloads)
/// are also returned because they run in actor context and may contain handler-adjacent code. Type-marker
/// arguments (e.g. <c>Type messageType</c>) are skipped.
///
/// <para><b>Resolved shapes</b> (returned with a specific <see cref="ActorHandlerBindingKind"/>):</para>
/// <list type="bullet">
///   <item><description>Inline lambda expressions, including <c>async</c> variants</description></item>
///   <item><description>Inline anonymous methods (<c>delegate(...)</c>)</description></item>
///   <item><description>Method groups: instance methods, static methods, local functions, constructed generics, methods on field/property receivers</description></item>
///   <item><description>The above wrapped in parentheses, casts (<c>(Func&lt;...&gt;)X</c>), or explicit delegate constructors (<c>new Func&lt;...&gt;(X)</c>)</description></item>
/// </list>
///
/// <para><b>Unresolved shapes</b> (returned with <see cref="ActorHandlerBindingKind.Unknown"/>):</para>
/// <list type="bullet">
///   <item><description>Local variable / parameter of delegate type — would require single-assignment flow analysis</description></item>
///   <item><description>Field / property of delegate type — external mutation possible</description></item>
///   <item><description>Method-call result (<c>ReceiveAsync(GetHandler())</c>) — opaque without interprocedural analysis</description></item>
///   <item><description>Conditional or null-coalescing expressions — could resolve to a set of bindings, complicates API for marginal gain</description></item>
///   <item><description>Multicast delegate combination (<c>A + B</c>) — degenerate for <c>Func&lt;..., Task&gt;</c></description></item>
/// </list>
/// </remarks>
public static class ActorHandlerResolver
{
    /// <summary>
    /// Returns <c>true</c> if <paramref name="methodSymbol"/> is one of the recognized handler-registration
    /// methods (<c>Receive*</c>, <c>Command*</c>, <c>Recover*</c>, <c>RunTask</c>).
    /// </summary>
    public static bool IsHandlerRegistration(IMethodSymbol methodSymbol, AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(methodSymbol);
        Guard.AssertIsNotNull(akkaContext);

        var receiveActor = akkaContext.AkkaCore.Actor.ReceiveActor;
        if (methodSymbol.MatchesAny(receiveActor.Receive)
            || methodSymbol.MatchesAny(receiveActor.ReceiveAsync)
            || methodSymbol.MatchesAny(receiveActor.ReceiveAny)
            || methodSymbol.MatchesAny(receiveActor.ReceiveAnyAsync))
            return true;

        if (methodSymbol.MatchesAny(akkaContext.AkkaCore.Actor.UntypedActor.RunTask))
            return true;

        if (akkaContext.HasAkkaPersistenceInstalled)
        {
            var rpa = akkaContext.AkkaPersistence.ReceivePersistentActor;
            if (methodSymbol.MatchesAny(rpa.Command)
                || methodSymbol.MatchesAny(rpa.CommandAsync)
                || methodSymbol.MatchesAny(rpa.Recover))
                return true;

            if (rpa.CommandAny is not null && IsSameOriginalDefinition(methodSymbol, rpa.CommandAny))
                return true;
            if (rpa.CommandAnyAsync is not null && IsSameOriginalDefinition(methodSymbol, rpa.CommandAnyAsync))
                return true;
            if (rpa.RecoverAny is not null && IsSameOriginalDefinition(methodSymbol, rpa.RecoverAny))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves the handler binding(s) of <paramref name="invocation"/>. Returns an empty enumerable
    /// when the invocation is not a recognized handler-registration call. Returns one
    /// <see cref="ActorHandlerBinding"/> per delegate or predicate argument.
    /// </summary>
    public static IEnumerable<ActorHandlerBinding> Resolve(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        AkkaContext akkaContext)
    {
        Guard.AssertIsNotNull(invocation);
        Guard.AssertIsNotNull(semanticModel);
        Guard.AssertIsNotNull(akkaContext);

        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
            yield break;

        if (!IsHandlerRegistration(methodSymbol, akkaContext))
            yield break;

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (!IsDelegateLikeParameter(methodSymbol, argument))
                continue;

            yield return ResolveArgument(argument.Expression, semanticModel);
        }
    }

    private static ActorHandlerBinding ResolveArgument(ExpressionSyntax originalArg, SemanticModel semanticModel)
    {
        var unwrapped = Unwrap(originalArg);

        switch (unwrapped)
        {
            case LambdaExpressionSyntax lambda:
                return ActorHandlerBinding.ForLambda(lambda, originalArg);
            case AnonymousMethodExpressionSyntax anon:
                return ActorHandlerBinding.ForAnonymousMethod(anon, originalArg);
        }

        // Method group: identifier or member access (or generic-name) that resolves to a method.
        // Local variables/parameters/properties also resolve to a symbol but NOT IMethodSymbol —
        // those fall through to Unknown.
        if (unwrapped is IdentifierNameSyntax or MemberAccessExpressionSyntax or GenericNameSyntax)
        {
            var symbol = semanticModel.GetSymbolInfo(unwrapped).Symbol;
            if (symbol is IMethodSymbol method)
                return ActorHandlerBinding.ForMethodGroup(method, originalArg);
        }

        return ActorHandlerBinding.ForUnknown(originalArg);
    }

    /// <summary>
    /// Strips parentheses, casts, and explicit delegate constructors (<c>new Func&lt;...&gt;(X)</c>)
    /// to expose the underlying expression.
    /// </summary>
    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (true)
        {
            switch (expression)
            {
                case ParenthesizedExpressionSyntax paren:
                    expression = paren.Expression;
                    continue;
                case CastExpressionSyntax cast:
                    expression = cast.Expression;
                    continue;
                case ObjectCreationExpressionSyntax { ArgumentList: { Arguments.Count: 1 } argList } ctor
                    when IsDelegateConstruction(ctor):
                    expression = argList.Arguments[0].Expression;
                    continue;
                default:
                    return expression;
            }
        }
    }

    /// <summary>
    /// Heuristic: a single-arg <c>new SomeType(X)</c> where the type name suggests a delegate
    /// (<c>Func</c>, <c>Action</c>, <c>Predicate</c>, or anything ending in <c>Receive</c>/<c>Handler</c>).
    /// We unwrap conservatively without checking the symbol — the resolution of <c>X</c> still
    /// has to succeed for us to return a method-group binding.
    /// </summary>
    private static bool IsDelegateConstruction(ObjectCreationExpressionSyntax ctor)
    {
        var name = ctor.Type switch
        {
            GenericNameSyntax g => g.Identifier.Text,
            IdentifierNameSyntax i => i.Identifier.Text,
            QualifiedNameSyntax q when q.Right is GenericNameSyntax qg => qg.Identifier.Text,
            QualifiedNameSyntax q when q.Right is IdentifierNameSyntax qi => qi.Identifier.Text,
            _ => null,
        };

        return name is "Func" or "Action" or "Predicate"
               || (name is not null && (name.EndsWith("Receive", StringComparison.Ordinal)
                                        || name.EndsWith("Handler", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="argument"/> binds to a parameter whose type is
    /// a delegate-like type (<c>System.Delegate</c>-derived). Type-marker arguments such as
    /// <c>Type messageType</c> are excluded.
    /// </summary>
    private static bool IsDelegateLikeParameter(IMethodSymbol method, ArgumentSyntax argument)
    {
        var index = -1;
        if (argument.NameColon is { Name.Identifier.Text: { } namedArg })
        {
            for (var i = 0; i < method.Parameters.Length; i++)
            {
                if (method.Parameters[i].Name == namedArg)
                {
                    index = i;
                    break;
                }
            }
        }
        else if (argument.Parent is ArgumentListSyntax argList)
        {
            index = argList.Arguments.IndexOf(argument);
        }

        if (index < 0 || index >= method.Parameters.Length)
            return false;

        return method.Parameters[index].Type.TypeKind == TypeKind.Delegate;
    }

    private static bool IsSameOriginalDefinition(IMethodSymbol candidate, IMethodSymbol target)
    {
        var from = candidate.ConstructedFrom;
        while (!ReferenceEquals(from, from.ConstructedFrom))
            from = from.ConstructedFrom;
        return ReferenceEquals(from, target);
    }
}
