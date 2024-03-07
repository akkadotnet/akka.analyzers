// -----------------------------------------------------------------------
//  <copyright file="ShouldUseIWithTimersInsteadOfScheduleTellAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.ShouldUseIWithTimersInsteadOfScheduleTellAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class ShouldUseIWithTimersInsteadOfScheduleTellAnalyzerSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
    {
        // ReceiveActor without ScheduleTellOnce() or ScheduleTellRepeatedly() at all
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str => { // shouldn't flag this
            Sender.Tell(str); 
        });
    }
}
""",

        // Non-Actor class that has ScheduleTellOnce() and/or ScheduleTellRepeatedly() methods, we're not responsible for this.
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor
{
    public MyActor()
    {
        ScheduleTellOnce();
        ScheduleTellRepeatedly();
    }

    public void ScheduleTellOnce() { }
    public void ScheduleTellRepeatedly() { }
}

public class MyOtherActor
{
    public MyOtherActor(MyActor actor)
    {
        actor.ScheduleTellOnce();
        actor.ScheduleTellRepeatedly();
    }
}
""",

        // Non-Actor class that uses ScheduleTellOnce() and/or ScheduleTellRepeatedly(),
        // we're only responsible for checking usage inside actor class.
"""
using System;
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor
{
    public MyActor(IScheduler scheduler, IActorRef from, IActorRef to)
    {
        scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), to, "test", from);
        scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), to, "test", from, null);
        scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), to, "test", from);
        scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), to, "test", from, null);
    }
}
""",
    };

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        FailureCases = new()
        {
            // Receive actor invoking ScheduleTellOnce() variant 1
            (
"""
// 01
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str => 
        {
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self);
        });
    }
}
""", (12, 13, 12, 99)),
            
            // Receive actor invoking ScheduleTellOnce() variant 2
            (
"""
// 02
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str =>
        {
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self, null);
        });
    }
}
""", (12, 13, 12, 105)),
            
            // Receive actor invoking ScheduleTellOnce() on scheduler stored inside a variable variant 1
            (
"""
// 03
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        var scheduler = Context.System.Scheduler;
        Receive<string>(str =>
        {
            scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self);
        });
    }
}
""", (13, 13, 13, 84)),
            
            // Receive actor invoking ScheduleTellOnce() on scheduler stored inside a variable variant 2
            (
"""
// 04
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        var scheduler = Context.System.Scheduler;
        Receive<string>(str =>
        {
            scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self, null);
        });
    }
}
""", (13, 13, 13, 90)),
            
            // Receive actor invoking ScheduleTellOnce() on scheduler stored inside a field variant 1
            (
"""
// 05
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly IScheduler _scheduler;
    
    public MyActor()
    {
        _scheduler = Context.System.Scheduler;
        
        Receive<string>(str =>
        {
            _scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self);
        });
    }
}
""", (16, 13, 16, 85)),
            
            // Receive actor invoking ScheduleTellOnce() on scheduler stored inside a field variant 2
            (
"""
// 06
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly IScheduler _scheduler;
    
    public MyActor()
    {
        _scheduler = Context.System.Scheduler;
        
        Receive<string>(str =>
        {
            _scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", null);
        });
    }
}
""", (16, 13, 16, 85)),
            
            // Receive actor invoking ScheduleTellOnce() on scheduler stored inside a property variant 1
            (
"""
// 07
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private IScheduler MyScheduler { get; }
    
    public MyActor()
    {
        MyScheduler = Context.System.Scheduler;
        
        Receive<string>(str =>
        {
            MyScheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self);
        });
    }
}
""", (16, 13, 16, 86)),
            
            // Receive actor invoking ScheduleTellOnce() on scheduler stored inside a property variant 2
            (
"""
// 08
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private IScheduler MyScheduler { get; }
    
    public MyActor()
    {
        MyScheduler = Context.System.Scheduler;
        
        Receive<string>(str =>
        {
            MyScheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self, null);
        });
    }
}
""", (16, 13, 16, 92)),

            // Receive actor invoking ScheduleTellOnce() on scheduler returned by a function variant 1
            (
"""
// 09
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private IScheduler MyScheduler() => Context.System.Scheduler;
    
    public MyActor()
    {
        Receive<string>(str =>
        {
            MyScheduler().ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self);
        });
    }
}
""", (14, 13, 14, 88)),
            
            // Receive actor invoking ScheduleTellOnce() on scheduler returned by a function variant 2
            (
"""
// 10
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private IScheduler MyScheduler() => Context.System.Scheduler;
    
    public MyActor()
    {
        Receive<string>(str =>
        {
            MyScheduler().ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self, null);
        });
    }
}
""", (14, 13, 14, 94)),
            
            // Receive actor invoking ScheduleTellOnce() on scheduler passed as function parameter variant 1
            (
"""
// 11
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str =>
        {
            void InnerLambda(IScheduler scheduler)
            {
                scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self);
            }
            
            InnerLambda(Context.System.Scheduler);
        });
    }
}
""", (14, 17, 14, 88)),
            
            // Receive actor invoking ScheduleTellOnce() on scheduler passed as function parameter variant 2
            (
"""
// 12
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str =>
        {
            void InnerLambda(IScheduler scheduler)
            {
                scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test", Self, null);
            }
            
            InnerLambda(Context.System.Scheduler);
        });
    }
}
""", (14, 17, 14, 94)),

            // Receive actor invoking ScheduleTellRepeatedly() variant 1
            (
"""
// 13
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str => 
        {
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self);
        });
    }
}
""", (12, 13, 12, 130)),
            
            // Receive actor invoking ScheduleTellRepeatedly() variant 2
            (
"""
// 14
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str =>
        {
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self, null);
        });
    }
}
""", (12, 13, 12, 136)),
            
            // Receive actor invoking ScheduleTellRepeatedly() on scheduler stored inside a variable variant 1
            (
"""
// 15
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        var scheduler = Context.System.Scheduler;
        Receive<string>(str =>
        {
            scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self);
        });
    }
}
""", (13, 13, 13, 115)),
            
            // Receive actor invoking ScheduleTellRepeatedly() on scheduler stored inside a variable variant 2
            (
"""
// 16
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        var scheduler = Context.System.Scheduler;
        Receive<string>(str =>
        {
            scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self, null);
        });
    }
}
""", (13, 13, 13, 121)),
            
            // Receive actor invoking ScheduleTellRepeatedly() on scheduler stored inside a field variant 1
            (
"""
// 17
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly IScheduler _scheduler;
    
    public MyActor()
    {
        _scheduler = Context.System.Scheduler;
        
        Receive<string>(str =>
        {
            _scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self);
        });
    }
}
""", (16, 13, 16, 116)),
            
            // Receive actor invoking ScheduleTellRepeatedly() on scheduler stored inside a field variant 2
            (
"""
// 18
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private readonly IScheduler _scheduler;
    
    public MyActor()
    {
        _scheduler = Context.System.Scheduler;
        
        Receive<string>(str =>
        {
            _scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", null);
        });
    }
}
""", (16, 13, 16, 116)),
            
            // Receive actor invoking ScheduleTellRepeatedly() on scheduler stored inside a property variant 1
            (
"""
// 19
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private IScheduler MyScheduler { get; }
    
    public MyActor()
    {
        MyScheduler = Context.System.Scheduler;
        
        Receive<string>(str =>
        {
            MyScheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self);
        });
    }
}
""", (16, 13, 16, 117)),
            
            // Receive actor invoking ScheduleTellRepeatedly() on scheduler stored inside a property variant 2
            (
"""
// 20
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private IScheduler MyScheduler { get; }
    
    public MyActor()
    {
        MyScheduler = Context.System.Scheduler;
        
        Receive<string>(str =>
        {
            MyScheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self, null);
        });
    }
}
""", (16, 13, 16, 123)),

            // Receive actor invoking ScheduleTellRepeatedly() on scheduler returned by a function variant 1
            (
"""
// 21
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private IScheduler MyScheduler() => Context.System.Scheduler;
    
    public MyActor()
    {
        Receive<string>(str =>
        {
            MyScheduler().ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self);
        });
    }
}
""", (14, 13, 14, 119)),
            
            // Receive actor invoking ScheduleTellRepeatedly() on scheduler returned by a function variant 2
            (
"""
// 22
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    private IScheduler MyScheduler() => Context.System.Scheduler;
    
    public MyActor()
    {
        Receive<string>(str =>
        {
            MyScheduler().ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self, null);
        });
    }
}
""", (14, 13, 14, 125)),
            
            // Receive actor invoking ScheduleTellRepeatedly() on scheduler passed as function parameter variant 1
            (
"""
// 23
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str =>
        {
            void InnerLambda(IScheduler scheduler)
            {
                scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self);
            }
            
            InnerLambda(Context.System.Scheduler);
        });
    }
}
""", (14, 17, 14, 119)),
            
            // Receive actor invoking ScheduleTellRepeatedly() on scheduler passed as function parameter variant 2
            (
"""
// 24
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str =>
        {
            void InnerLambda(IScheduler scheduler)
            {
                scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), Self, "test", Self, null);
            }
            
            InnerLambda(Context.System.Scheduler);
        });
    }
}
""", (14, 17, 14, 125)),
            
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
            .WithArguments("ScheduleTell invocation")
            .WithSeverity(DiagnosticSeverity.Warning);

        return Verify.VerifyAnalyzer(d.testCode, expected);
    }
}