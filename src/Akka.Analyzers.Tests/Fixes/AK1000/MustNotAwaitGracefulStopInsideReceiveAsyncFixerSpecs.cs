// -----------------------------------------------------------------------
//  <copyright file="MustNotAwaitGracefulStopInsideReceiveAsyncFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotAwaitGracefulStopInsideReceiveAsyncAnalyzer>;

namespace Akka.Analyzers.Tests.Fixes.AK1000;

public class MustNotAwaitGracefulStopInsideReceiveAsyncFixerSpecs
{
    [Fact]
    public Task RemoveAwaitInsideReceiveAsyncMethod()
    {
        var before =
            """
            using System;
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAsync<string>(async str => {
                        await Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
                    });
                }
            }
            """;

        var after =
            """
            using System;
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAsync<string>(async str => {
                        _ = Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
                    });
                }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(10, 13, 10, 69)
            .WithArguments();

        return Verify.VerifyCodeFix(before, after, 
            MustNotAwaitGracefulStopInsideReceiveAsyncFixer.Key_FixAwaitGracefulStop, expectedDiagnostic);
    }
    
    [Fact]
    public Task RemoveAwaitInsideReceiveAnyAsyncMethod()
    {
        var before =
            """
            using System;
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAnyAsync(async obj => {
                        await Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
                    });
                }
            }
            """;

        var after =
            """
            using System;
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAnyAsync(async obj => {
                        _ = Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
                    });
                }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(10, 13, 10, 69)
            .WithArguments();

        return Verify.VerifyCodeFix(before, after, 
            MustNotAwaitGracefulStopInsideReceiveAsyncFixer.Key_FixAwaitGracefulStop, expectedDiagnostic);
    }
    
    [Fact]
    public Task RemoveAwaitInsideOneLinerReceiveAsyncMethod()
    {
        var before =
            """
            using System;
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAsync<string>(async str => await Context.Self.GracefulStop(TimeSpan.FromSeconds(3)));
                }
            }
            """;

        var after =
            """
            using System;
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAsync<string>(async str => _ = Context.Self.GracefulStop(TimeSpan.FromSeconds(3)));
                }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(9, 43, 9, 99);

        return Verify.VerifyCodeFix(before, after, 
            MustNotAwaitGracefulStopInsideReceiveAsyncFixer.Key_FixAwaitGracefulStop, expectedDiagnostic);
    }
    
    [Fact]
    public Task RemoveAwaitInsideOneLinerReceiveAnyAsyncMethod()
    {
        var before =
            """
            using System;
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAnyAsync(async obj => await Context.Self.GracefulStop(TimeSpan.FromSeconds(3)));
                }
            }
            """;

        var after =
            """
            using System;
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    ReceiveAnyAsync(async obj => _ = Context.Self.GracefulStop(TimeSpan.FromSeconds(3)));
                }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(9, 38, 9, 94);

        return Verify.VerifyCodeFix(before, after, 
            MustNotAwaitGracefulStopInsideReceiveAsyncFixer.Key_FixAwaitGracefulStop, expectedDiagnostic);
    }
}