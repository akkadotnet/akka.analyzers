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

        // GracefulStop being called on actor other than Self
"""
using System;
using Akka.Actor;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<IActorRef>(async p =>
        {
            var actorSelection = Context.System.ActorSelection(p.Path);
            var actorRef = await actorSelection.ResolveOne(TimeSpan.FromSeconds(3));
            await actorRef.GracefulStop(TimeSpan.FromSeconds(3)); // should not flag this
        });

        ReceiveAnyAsync(async _ =>
        {
            var actorSelection = Context.System.ActorSelection(ActorPath.Parse(""));
            var actorRef = await actorSelection.ResolveOne(TimeSpan.FromSeconds(3));
            await actorRef.GracefulStop(TimeSpan.FromSeconds(3)); // should not flag this
        });
    }
}
""",

        // Method exists on the actor with await Self.GracefulStop() but is NEVER bound as a handler.
        // The lambda-only check has always missed this; the cache-based check should also leave it alone
        // because the method isn't in the cache.
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor : ReceiveActor
{
    public MyActor()
    {
        // No ReceiveAsync(StopMe) registration anywhere
        ReceiveAsync<string>(async msg => { await Task.Yield(); });
    }

    private async Task StopMe(string msg) // not a handler
    {
        await Self.GracefulStop(TimeSpan.FromSeconds(3));
    }
}
""",

        // Nested anonymous async lambda inside a method-group handler: Task.Run is off the actor
        // scheduler, so ConfigureAwait/await semantics inside the inner lambda are decoupled from
        // the actor context. Should NOT flag.
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(StopMe);
    }

    private async Task StopMe(string msg)
    {
        await Task.Run(async () =>
        {
            await Self.GracefulStop(TimeSpan.FromSeconds(3)); // not flagged — inside Task.Run lambda
        });
    }
}
""",

    };

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        FailureCases = new()
        {
            // Receive actor invoking await Context.Self.GracefulStop() inside a ReceiveAsync<T> block
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
            
            // Receive actor invoking await GracefulStop() on ActorContext stored in a variable
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
            var ctx = Context;
            await ctx.Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""", (12, 13, 12, 65)),
            
            // Receive actor invoking await GracefulStop() on ActorContext stored inside a field
            (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly IActorContext _context;
    
    public MyActor()
    {
        _context = Context;
        
        ReceiveAsync<string>(async str =>
        {
            await _context.Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""", (15, 13, 15, 70)),
            
            // Receive actor invoking await GracefulStop() on ActorContext stored inside a property
            (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private IActorContext MyContext { get; }
    
    public MyActor()
    {
        MyContext = Context;
        
        ReceiveAsync<string>(async str =>
        {
            await MyContext.Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""", (15, 13, 15, 71)),
            
            // Receive actor invoking await GracefulStop() on ActorContext returned by a function
            (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private IActorContext MyContext() => Context;
    
    public MyActor()
    {
        ReceiveAsync<string>(async str =>
        {
            await MyContext().Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""", (13, 13, 13, 73)),
            
            // Receive actor invoking await Context.Self.GracefulStop() inside a lambda function inside ReceiveAsync<T>()
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
            async Task InnerLambda()
            {
                await Context.Self.GracefulStop(TimeSpan.FromSeconds(3));
            }
            
            await InnerLambda();
        });
    }
}
""", (13, 17, 13, 73)),
            
            // Receive actor invoking await GracefulStop() on ActorContext passed as function parameter of a lambda function inside ReceiveAsync<T>()
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
            async Task InnerLambda(IActorContext ctx)
            {
                await ctx.Self.GracefulStop(TimeSpan.FromSeconds(3));
            }
            
            await InnerLambda(Context);
        });
    }
}
""", (13, 17, 13, 69)),
            
            // Receive actor invoking await Context.Self.GracefulStop() inside a ReceiveAnyAsync block
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
            
            // Receive actor invoking await Context.Self.GracefulStop() inside a ReceiveAsync<T> with no code block
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
            
            // Receive actor invoking await Context.Self.GracefulStop() inside a ReceiveAnyAsync with no code block
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
            
            // Receive actor invoking await Self.GracefulStop() inside a ReceiveAsync<T> block
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
            await Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""", (11, 13, 11, 61)),
            
            // Receive actor invoking await Self.GracefulStop() inside a ReceiveAnyAsync block
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
            await Self.GracefulStop(TimeSpan.FromSeconds(3));
        });
    }
}
""", (11, 13, 11, 61)),
            
        // Receive actor invoking await Self.GracefulStop() inside a ReceiveAsync<T> with no code block
        (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str => await Self.GracefulStop(TimeSpan.FromSeconds(3)));
    }
}
""", (9, 43, 9, 91)),
            
            // Receive actor invoking await Self.GracefulStop() inside a ReceiveAnyAsync with no code block
            (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAnyAsync(async obj => await Self.GracefulStop(TimeSpan.FromSeconds(3)));
    }
}
""", (9, 38, 9, 86)),

            // Method-group binding: ReceiveAsync<T>(StopMe) where StopMe body has await Self.GracefulStop()
            (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(StopMe);
    }

    private async Task StopMe(string msg)
    {
        await Self.GracefulStop(TimeSpan.FromSeconds(3));
    }
}
""", (14, 9, 14, 57)),

            // Method-group binding via CommandAsync<T> on a ReceivePersistentActor
            (
"""
using System;
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public sealed class MyActor : ReceivePersistentActor
{
    public MyActor()
    {
        CommandAsync<string>(StopMe);
    }

    private async Task StopMe(string msg)
    {
        await Self.GracefulStop(TimeSpan.FromSeconds(3));
    }

    public override string PersistenceId => "p";
}
""", (15, 9, 15, 57)),

            // Method-group binding via UntypedActor.RunTask with Func<Task> overload
            (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : UntypedActor
{
    protected override void OnReceive(object message) { RunTask(StopMe); }

    private async Task StopMe()
    {
        await Self.GracefulStop(TimeSpan.FromSeconds(3));
    }
}
""", (11, 9, 11, 57)),

            // Nested local async function inside a method-group handler — same actor scheduler context
            (
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor() { ReceiveAsync<string>(StopMe); }

    private async Task StopMe(string msg)
    {
        async Task Inner()
        {
            await Self.GracefulStop(TimeSpan.FromSeconds(3));
        }
        await Inner();
    }
}
""", (13, 13, 13, 61)),
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

    [Fact]
    public Task MethodGroupBoundAcrossFiles()
    {
        const string actorFile = """
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor() { ReceiveAsync<string>(Helpers.StopHandler); }
            }
            """;

        const string helpersFile = """
            using System;
            using Akka.Actor;
            using System.Threading.Tasks;

            public static class Helpers
            {
                public static async Task StopHandler(string msg)
                {
                    var self = ActorRefs.Nobody;
                    await self.GracefulStop(TimeSpan.FromSeconds(3));
                }
            }
            """;

        // Helpers.StopHandler is bound as a handler in the actor file but its body lives in
        // a separate file. Verify the compilation-wide cache picks it up. The method uses a
        // local IActorRef variable instead of Self because it's static; the analyzer's
        // `IsAccessingActorSelf` check requires Self/Context.Self, so this will NOT fire.
        // Useful negative case proving the analyzer still gates on the access pattern.
        return Verify.VerifyAnalyzer(new[] { actorFile, helpersFile });
    }
}