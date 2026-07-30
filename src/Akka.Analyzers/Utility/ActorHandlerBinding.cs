// -----------------------------------------------------------------------
//  <copyright file="ActorHandlerBinding.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Akka.Analyzers;

/// <summary>
/// Discriminator for what shape was passed as a delegate argument to an actor
/// handler-registration call (e.g. <c>ReceiveAsync</c>, <c>CommandAsync</c>, <c>RunTask</c>).
/// </summary>
public enum ActorHandlerBindingKind
{
    /// <summary>The argument was an inline lambda expression.</summary>
    Lambda,

    /// <summary>The argument was an inline anonymous method (<c>delegate(...)</c>).</summary>
    AnonymousMethod,

    /// <summary>
    /// The argument was a method group reference that resolved to a single <see cref="IMethodSymbol"/>.
    /// Includes instance methods, static methods, local functions, and constructed generic method groups.
    /// </summary>
    MethodGroup,

    /// <summary>
    /// The argument could not be statically resolved to a specific delegate target. Examples:
    /// delegate-typed local variables, fields/properties, method-call results, conditional expressions,
    /// null-coalescing expressions. Doing so would require flow analysis or interprocedural analysis.
    /// </summary>
    Unknown,
}

/// <summary>
/// Describes the handler delegate bound to a single argument of an actor handler-registration
/// invocation (e.g. <c>ReceiveAsync&lt;T&gt;(handler)</c>, <c>CommandAsync&lt;T&gt;(handler)</c>,
/// <c>RunTask(handler)</c>, including predicate-having overloads).
/// </summary>
/// <remarks>
/// Exactly one of <see cref="Lambda"/>, <see cref="AnonymousMethod"/>, <see cref="MethodGroup"/> is
/// non-null when <see cref="Kind"/> is the corresponding value. <see cref="ArgumentExpression"/> is
/// always populated and points at the original argument expression in source (before unwrap).
/// </remarks>
public readonly struct ActorHandlerBinding : IEquatable<ActorHandlerBinding>
{
    private ActorHandlerBinding(
        ActorHandlerBindingKind kind,
        ExpressionSyntax argumentExpression,
        LambdaExpressionSyntax? lambda,
        AnonymousMethodExpressionSyntax? anonymousMethod,
        IMethodSymbol? methodGroup)
    {
        Kind = kind;
        ArgumentExpression = argumentExpression;
        Lambda = lambda;
        AnonymousMethod = anonymousMethod;
        MethodGroup = methodGroup;
    }

    /// <summary>The shape of the bound delegate.</summary>
    public ActorHandlerBindingKind Kind { get; }

    /// <summary>The original argument expression node from source.</summary>
    public ExpressionSyntax ArgumentExpression { get; }

    /// <summary>The lambda expression, if <see cref="Kind"/> is <see cref="ActorHandlerBindingKind.Lambda"/>.</summary>
    public LambdaExpressionSyntax? Lambda { get; }

    /// <summary>The anonymous method expression, if <see cref="Kind"/> is
    /// <see cref="ActorHandlerBindingKind.AnonymousMethod"/>.</summary>
    public AnonymousMethodExpressionSyntax? AnonymousMethod { get; }

    /// <summary>The resolved method symbol, if <see cref="Kind"/> is
    /// <see cref="ActorHandlerBindingKind.MethodGroup"/>.</summary>
    public IMethodSymbol? MethodGroup { get; }

    internal static ActorHandlerBinding ForLambda(LambdaExpressionSyntax lambda, ExpressionSyntax originalArg)
        => new(ActorHandlerBindingKind.Lambda, originalArg, lambda, null, null);

    internal static ActorHandlerBinding ForAnonymousMethod(AnonymousMethodExpressionSyntax anon, ExpressionSyntax originalArg)
        => new(ActorHandlerBindingKind.AnonymousMethod, originalArg, null, anon, null);

    internal static ActorHandlerBinding ForMethodGroup(IMethodSymbol method, ExpressionSyntax originalArg)
        => new(ActorHandlerBindingKind.MethodGroup, originalArg, null, null, method);

    internal static ActorHandlerBinding ForUnknown(ExpressionSyntax originalArg)
        => new(ActorHandlerBindingKind.Unknown, originalArg, null, null, null);

    public bool Equals(ActorHandlerBinding other)
        => Kind == other.Kind
           && ReferenceEquals(ArgumentExpression, other.ArgumentExpression)
           && ReferenceEquals(Lambda, other.Lambda)
           && ReferenceEquals(AnonymousMethod, other.AnonymousMethod)
           && SymbolEqualityComparer.Default.Equals(MethodGroup, other.MethodGroup);

    public override bool Equals(object? obj) => obj is ActorHandlerBinding b && Equals(b);

    public override int GetHashCode()
    {
        unchecked
        {
            var h = (int)Kind;
            h = (h * 397) ^ (ArgumentExpression?.GetHashCode() ?? 0);
            h = (h * 397) ^ (Lambda?.GetHashCode() ?? 0);
            h = (h * 397) ^ (AnonymousMethod?.GetHashCode() ?? 0);
            h = (h * 397) ^ (MethodGroup is null ? 0 : SymbolEqualityComparer.Default.GetHashCode(MethodGroup));
            return h;
        }
    }

    public static bool operator ==(ActorHandlerBinding left, ActorHandlerBinding right) => left.Equals(right);
    public static bool operator !=(ActorHandlerBinding left, ActorHandlerBinding right) => !left.Equals(right);
}
