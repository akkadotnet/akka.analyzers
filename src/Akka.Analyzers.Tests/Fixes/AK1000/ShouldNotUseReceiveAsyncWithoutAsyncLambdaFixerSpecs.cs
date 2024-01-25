// -----------------------------------------------------------------------
//  <copyright file="ShouldNotUseReceiveAsyncWithoutAsyncLambdaFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.ShouldNotUseReceiveAsyncWithoutAsyncLambdaAnalyzer>;

namespace Akka.Analyzers.Tests.Fixes.AK1000;

public class ShouldNotUseReceiveAsyncWithoutAsyncLambdaFixerSpecs
{
    [Fact]
    public Task ConvertReceiveAsyncToReceiveAndRemoveAsync()
    {
        const string before =
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str =>
        {
            Sender.Tell(str);
        });
    }
}
""";

        const string  after =
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str =>
        {
            Sender.Tell(str);
        });
    }
}
""";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(8, 9, 11, 11)
            .WithArguments("lambda expression");

        return Verify.VerifyCodeFix(before, after, ShouldNotUseReceiveAsyncWithoutAsyncLambdaFixer.Key_FixReceiveAsyncWithoutAsync,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task ConvertReceiveAsyncToReceiveAndRemoveAsyncAlternate()
    {
        const string before =
            """
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAsync<string>(str => true, async str =>
                    {
                        Sender.Tell(str);
                    });
                }
            }
            """;

        const string  after =
            """
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    Receive<string>(str => true, str =>
                    {
                        Sender.Tell(str);
                    });
                }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(8, 9, 11, 11)
            .WithArguments("lambda expression");

        return Verify.VerifyCodeFix(before, after, ShouldNotUseReceiveAsyncWithoutAsyncLambdaFixer.Key_FixReceiveAsyncWithoutAsync,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task ConvertReceiveAnyAsyncToReceiveAnyAndRemoveAsync()
    {
        const string before =
            """
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAnyAsync(async o =>
                    {
                        Sender.Tell(o);
                    });
                }
            }
            """;

        const string  after =
            """
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAny(o =>
                    {
                        Sender.Tell(o);
                    });
                }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(8, 9, 11, 11)
            .WithArguments("lambda expression");

        return Verify.VerifyCodeFix(before, after, ShouldNotUseReceiveAsyncWithoutAsyncLambdaFixer.Key_FixReceiveAsyncWithoutAsync,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task ConvertBlocklessReceiveAsyncToReceiveAndRemoveAsync()
    {
        const string before =
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str => Sender.Tell(str));
    }
}
""";

        const string  after =
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str => Sender.Tell(str));
    }
}
""";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(8, 9, 8, 60)
            .WithArguments("lambda expression");

        return Verify.VerifyCodeFix(before, after, ShouldNotUseReceiveAsyncWithoutAsyncLambdaFixer.Key_FixReceiveAsyncWithoutAsync,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task ConvertBlocklessReceiveAsyncToReceiveAndRemoveAsyncAlternate()
    {
        const string before =
            """
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAsync<string>(str => true, async str => Sender.Tell(str));
                }
            }
            """;

        const string  after =
            """
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    Receive<string>(str => true, str => Sender.Tell(str));
                }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(8, 9, 8, 73)
            .WithArguments("lambda expression");

        return Verify.VerifyCodeFix(before, after, ShouldNotUseReceiveAsyncWithoutAsyncLambdaFixer.Key_FixReceiveAsyncWithoutAsync,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task ConvertBlocklessReceiveAnyAsyncToReceiveAnyAndRemoveAsync()
    {
        const string before =
            """
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAnyAsync(async o => Sender.Tell(o));
                }
            }
            """;

        const string  after =
            """
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAny(o => Sender.Tell(o));
                }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(8, 9, 8, 51)
            .WithArguments("lambda expression");

        return Verify.VerifyCodeFix(before, after, ShouldNotUseReceiveAsyncWithoutAsyncLambdaFixer.Key_FixReceiveAsyncWithoutAsync,
            expectedDiagnostic);
    }
}