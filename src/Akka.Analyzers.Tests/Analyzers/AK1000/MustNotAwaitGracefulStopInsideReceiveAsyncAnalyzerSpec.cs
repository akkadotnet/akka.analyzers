// -----------------------------------------------------------------------
//  <copyright file="MustNotAwaitGracefulShutdownInsideReceiveAnalyzerSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotAwaitGracefulStopInsideReceiveAsyncAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class MustNotAwaitGracefulStopInsideReceiveAsyncAnalyzerSpec
{
        public static readonly TheoryData<string> SuccessCases = new()
    {
        // ReceiveActor calling GracefulStop() as detached task inside ReceiveAsync<T> block
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        // This is fine, detached task
        ReceiveAsync<string>(async str => {
            Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""",

        // ReceiveActor calling GracefulStop() as detached task inside ReceiveAnyAsync block
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        // This is fine, detached task
        ReceiveAnyAsync(async obj => {
            Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""",

        // ReceiveActor using ReceiveAsync<T> without GracefulStop() at all
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str => { // shouldn't flag this
            Sender.Tell(str); 
        });
    }
}
""",

        // ReceiveActor using ReceiveAnyAsync without GracefulStop() at all
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAnyAsync(async obj => { // shouldn't flag this
            Sender.Tell(obj);
        });
    }
}
""",

        // Non-Actor class that has an IActorRef Self property
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor
{
    public MyActor(IActorRef self)
    {
        Self = self;
        ReceiveAsync<string>(async str =>
        {
            await Self.GracefulStop(TimeSpan.FromSeconds(3));
        });

        ReceiveAnyAsync(async _ =>
        {
            await Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }

    public IActorRef Self { get; }
    
    public Task ReceiveAsync<T>(Func<string, Task> func)
    {
        return func("test");
    }
    
    public Task ReceiveAnyAsync(Func<object, Task> func)
    {
        return func("test");
    }
}
""",

        // User defined `GracefulStop()` method, we're not responsible for this.
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str =>
        {
            await GracefulStop(TimeSpan.FromSeconds(3));
        });

        ReceiveAnyAsync(async _ =>
        {
            await GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
    
    public Task GracefulStop(TimeSpan timeout)
    {
        return Task.CompletedTask;
    }
}
""",
    };

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        FailureCases = new()
        {
            // Receive actor invoking await GracefulStop() inside a ReceiveAsync<T> block
            (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str => 
        {
            await Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""", (11, 13, 11, 69)),
            
            // Receive actor invoking await GracefulStop() inside a ReceiveAnyAsync block
            (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAnyAsync(async obj =>
        {
            await Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""", (11, 13, 11, 69)),
            
            // Receive actor invoking await GracefulStop() inside a ReceiveAsync<T> with no code block
            (
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
""", (9, 43, 9, 99)),
            
            // Receive actor invoking await GracefulStop() inside a ReceiveAnyAsync with no code block
            (
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
""", (9, 38, 9, 94)),
        };

    [Theory]
    [MemberData(nameof(SuccessCases))]
    public async Task SuccessCase(string testCode)
    {
        await Verify.VerifyAnalyzer(testCode).ConfigureAwait(true);
    }

    [Theory]
    [MemberData(nameof(FailureCases))]
    public Task FailureCase(
        (string testCode, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var expected = Verify.Diagnostic()
            .WithSpan(d.spanData.startLine, d.spanData.startColumn, d.spanData.endLine, d.spanData.endColumn)
            .WithSeverity(DiagnosticSeverity.Error);

        return Verify.VerifyAnalyzer(d.testCode, expected);
    }
}