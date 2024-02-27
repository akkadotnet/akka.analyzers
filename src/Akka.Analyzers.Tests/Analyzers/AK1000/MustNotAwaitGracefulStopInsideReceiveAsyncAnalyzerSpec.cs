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