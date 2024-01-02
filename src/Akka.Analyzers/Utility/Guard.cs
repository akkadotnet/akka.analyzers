// -----------------------------------------------------------------------
//  <copyright file="Guard.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

namespace Akka.Analyzers;

/// <summary>
///     INTERNAL API
/// </summary>
internal static class Guard
{
    /// <summary>
    ///     Asserts that a reference type argument is not null.
    /// </summary>
    /// <param name="arg">The reference type to be evaluated.</param>
    /// <typeparam name="T">Type of class.</typeparam>
    /// <returns>The original value if the assertion passes.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T AssertIsNotNull<T>(T arg) where T : class
    {
        if (arg is null)
            throw new ArgumentNullException(nameof(arg));

        return arg;
    }
}