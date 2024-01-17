// -----------------------------------------------------------------------
//  <copyright file="MustNotAwaitGracefulShutdownInsideReceiveAnalyzerSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotAwaitGracefulStopInsideReceiveAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class MustNotAwaitGracefulStopInsideReceiveAnalyzerSpec
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
        ReceiveAsync<string>(str => {
            // This is fine, detached task
            Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
            return Task.CompletedTask;
        });
    }
}
""",

        // ReceiveActor calling GracefulStop() as detached task inside a method that is invoked from inside ReceiveAsync<T> block 
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(str => {
            return Execute(str);
        });
    }

    private Task Execute(string str)
    {
        // This is fine, detached task
        Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
        return Task.CompletedTask;
    }
}
""",

    // ReceiveActor calling GracefulStop() as detached task inside a method that is invoked as a method group from inside ReceiveAsync<T> block
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(Execute);
    }

    private Task Execute(string str)
    {
        // This is fine, detached task
        Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
        return Task.CompletedTask;
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
        ReceiveAsync<string>(str => {
            Sender.Tell(str); // shouldn't flag this
            return Task.CompletedTask;
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
    }

    public IActorRef Self { get; }
    
    public async Task ReceiveAsync<T>(T message)
    {
        await Self.GracefulStop(TimeSpan.FromSeconds(3));
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
        ReceiveAsync<string>(async str => {
            await Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""", (10, 13, 10, 69)),

            // UntypedActor invoking await GracefulStop() inside a OnReceive block
            (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : UntypedActor
{
    protected override void OnReceive(object message)
    {
        var sender = Sender;
        LocalFunction().PipeTo(sender);
        return;

        async Task<int> LocalFunction()
        {
            await Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
            return message.ToString().Length;
        }
    }
}
""", (15, 13, 15, 69)),
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