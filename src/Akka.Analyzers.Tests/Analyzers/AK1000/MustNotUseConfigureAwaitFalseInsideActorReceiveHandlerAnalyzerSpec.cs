// -----------------------------------------------------------------------
//  <copyright file="MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerAnalyzerSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerAnalyzerSpec
{
    public static readonly TheoryData<string> SuccessCases = new()
    {
        // Plain await without ConfigureAwait inside ReceiveAsync<T> is fine
"""
using System.Net.Http;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly HttpClient _client = new();

    public MyActor()
    {
        ReceiveAsync<string>(async url =>
        {
            var result = await _client.GetAsync(url);
            Sender.Tell(result);
        });
    }
}
""",

        // ConfigureAwait(true) inside ReceiveAsync<T> is fine (does not bypass scheduler)
"""
using System.Net.Http;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly HttpClient _client = new();

    public MyActor()
    {
        ReceiveAsync<string>(async url =>
        {
            var result = await _client.GetAsync(url).ConfigureAwait(true);
            Sender.Tell(result);
        });
    }
}
""",

        // ConfigureAwait(false) outside any actor handler is fine
"""
using System.Net.Http;
using System.Threading.Tasks;

public class NotAnActor
{
    private readonly HttpClient _client = new();

    public async Task DoWorkAsync(string url)
    {
        var result = await _client.GetAsync(url).ConfigureAwait(false);
    }
}
""",

        // ConfigureAwait(false) in a non-handler method on an actor class is fine —
        // outside the ReceiveAsync lambda the ActorTaskScheduler is not in play.
"""
using System.Net.Http;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly HttpClient _client = new();

    public MyActor()
    {
        ReceiveAsync<string>(async url =>
        {
            await DoWorkAsync(url);
        });
    }

    private async Task DoWorkAsync(string url)
    {
        await _client.GetAsync(url).ConfigureAwait(false);
    }
}
""",

        // ConfigureAwait(false) in a static helper called from a Receive lambda is fine
"""
using System.Net.Http;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async url =>
        {
            await Helpers.FetchAsync(url);
        });
    }
}

public static class Helpers
{
    private static readonly HttpClient Client = new();

    public static async Task FetchAsync(string url)
    {
        await Client.GetAsync(url).ConfigureAwait(false);
    }
}
""",

        // ConfigureAwait(false) inside Task.Run nested in ReceiveAsync is fine — Task.Run is already off the actor scheduler
"""
using System.Net.Http;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly HttpClient _client = new();

    public MyActor()
    {
        ReceiveAsync<string>(async url =>
        {
            await Task.Run(async () =>
            {
                await _client.GetAsync(url).ConfigureAwait(false);
            });
        });
    }
}
""",

        // User defined `ConfigureAwait` method on a non-Task type — we shouldn't flag this
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str =>
        {
            await new MyAwaitable().ConfigureAwait(false);
        });
    }
}

public class MyAwaitable
{
    public Task ConfigureAwait(bool flag) => Task.CompletedTask;
}
""",

        // ActorTaskScheduler.RunTask without ConfigureAwait(false) inside should pass cleanly.
"""
using Akka.Actor;
using Akka.Dispatch;
using System.Threading.Tasks;

public sealed class MyActor : UntypedActor
{
    protected override void OnReceive(object message)
    {
        if (message is string str)
        {
            ActorTaskScheduler.RunTask(async () =>
            {
                await Task.Delay(10);
                Sender.Tell(str);
            });
        }
    }
}
""",
    };

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        FailureCases = new()
        {
            // ConfigureAwait(false) inside ReceiveAsync<T>
            (
"""
using System.Net.Http;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly HttpClient _client = new();

    public MyActor()
    {
        ReceiveAsync<string>(async url =>
        {
            var result = await _client.GetAsync(url).ConfigureAwait(false);
            Sender.Tell(result);
        });
    }
}
""", (13, 54, 13, 75)),

            // ConfigureAwait(false) inside ReceiveAnyAsync
            (
"""
using System.Net.Http;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly HttpClient _client = new();

    public MyActor()
    {
        ReceiveAnyAsync(async msg =>
        {
            var result = await _client.GetAsync(msg.ToString()).ConfigureAwait(false);
            Sender.Tell(result);
        });
    }
}
""", (13, 65, 13, 86)),

            // ConfigureAwait(false) inside CommandAsync<T> on ReceivePersistentActor
            (
"""
using System.Net.Http;
using Akka.Persistence;
using System.Threading.Tasks;

public sealed class MyPersistentActor : ReceivePersistentActor
{
    private readonly HttpClient _client = new();

    public MyPersistentActor()
    {
        CommandAsync<string>(async url =>
        {
            var result = await _client.GetAsync(url).ConfigureAwait(false);
        });
    }

    public override string PersistenceId => "p1";
}
""", (13, 54, 13, 75)),

            // ConfigureAwait(false) inside CommandAnyAsync on ReceivePersistentActor
            (
"""
using System.Net.Http;
using Akka.Persistence;
using System.Threading.Tasks;

public sealed class MyPersistentActor : ReceivePersistentActor
{
    private readonly HttpClient _client = new();

    public MyPersistentActor()
    {
        CommandAnyAsync(async msg =>
        {
            var result = await _client.GetAsync(msg.ToString()).ConfigureAwait(false);
        });
    }

    public override string PersistenceId => "p1";
}
""", (13, 65, 13, 86)),

            // ConfigureAwait(false) on a Task<T>-returning expression
            (
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str =>
        {
            await Task.FromResult(0).ConfigureAwait(false);
        });
    }
}
""", (10, 38, 10, 59)),

            // ConfigureAwait(false) on a ValueTask
            (
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str =>
        {
            await new ValueTask().ConfigureAwait(false);
        });
    }
}
""", (10, 35, 10, 56)),

            // ConfigureAwait(false) inside a nested local async function inside ReceiveAsync<T>
            (
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str =>
        {
            async Task Inner()
            {
                await Task.FromResult(0).ConfigureAwait(false);
            }

            await Inner();
        });
    }
}
""", (12, 42, 12, 63)),

            // ConfigureAwait(false) inside ActorTaskScheduler.RunTask on an UntypedActor — RunTask schedules
            // the lambda's continuation back onto the ActorTaskScheduler, same as ReceiveAsync.
            (
"""
using Akka.Actor;
using Akka.Dispatch;
using System.Threading.Tasks;

public sealed class MyActor : UntypedActor
{
    protected override void OnReceive(object message)
    {
        ActorTaskScheduler.RunTask(async () =>
        {
            await Task.FromResult(0).ConfigureAwait(false);
        });
    }
}
""", (11, 38, 11, 59)),
        };

    [Theory]
    [MemberData(nameof(SuccessCases))]
    public Task SuccessCase(string testCode)
        => Verify.VerifyAnalyzer(testCode);

    [Theory]
    [MemberData(nameof(FailureCases))]
    public Task FailureCase(
        (string testCode, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var expected = Verify.Diagnostic()
            .WithSpan(d.spanData.startLine, d.spanData.startColumn, d.spanData.endLine, d.spanData.endColumn)
            .WithSeverity(DiagnosticSeverity.Warning);

        return Verify.VerifyAnalyzer(d.testCode, expected);
    }
}
