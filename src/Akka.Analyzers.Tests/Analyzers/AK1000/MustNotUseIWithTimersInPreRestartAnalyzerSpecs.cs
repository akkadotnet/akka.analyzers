// -----------------------------------------------------------------------
//  <copyright file="MustNotUseIWithTimersInPreRestartAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseIWithTimersInPreRestartAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class MustNotUseIWithTimersInPreRestartAnalyzerSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
    {
        // ReceiveActor calling ITimerScheduler methods outside of AroundPreRestart() and PreRestart()
"""
// 01
using System;
using Akka.Actor;

public class MyActor: ReceiveActor, IWithTimers
{
    public MyActor()
    {
        ReceiveAny(_ =>
        {
            Timers!.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
            Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
            Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
            LocalFunction();
            NonOverrideMethod();
        });
        
        Timers!.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
        
        return;

        void LocalFunction()
        {
            Timers!.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
            Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
            Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
        }
    }

    private void NonOverrideMethod()
    {
        Timers!.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }
    
    public override void AroundPostRestart(Exception cause, object message)
    {
        base.AroundPostRestart(cause, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    protected override void PostRestart(Exception reason)
    {
        base.PostRestart(reason);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    public override void AroundPostStop()
    {
        base.AroundPostStop();
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    protected override void PostStop()
    {
        base.PostStop();
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    public override void AroundPreStart()
    {
        base.AroundPreStart();
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    protected override void PreStart()
    {
        base.PreStart();
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; } = null!;
}
""",

    // UntypedActor calling ITimerScheduler methods outside of AroundPreRestart() and PreRestart()
"""
// 02
using System;
using Akka.Actor;

public class MyActor: UntypedActor, IWithTimers
{
    protected override void OnReceive(object message)
    {
        Timers!.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
        LocalFunction();
        NonOverrideMethod();
        
        return;

        void LocalFunction()
        {
            Timers!.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
            Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
            Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
        }
    }
    
    private void NonOverrideMethod()
    {
        Timers!.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }
    
    public override void AroundPostRestart(Exception cause, object message)
    {
        base.AroundPostRestart(cause, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    protected override void PostRestart(Exception reason)
    {
        base.PostRestart(reason);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    public override void AroundPostStop()
    {
        base.AroundPostStop();
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    protected override void PostStop()
    {
        base.PostStop();
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    public override void AroundPreStart()
    {
        base.AroundPreStart();
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    protected override void PreStart()
    {
        base.PreStart();
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; } = null!;
}
""",

        // ReceiveActor without ITimerScheduler method calls
"""
// 03
using Akka.Actor;

public class MyActor: ReceiveActor, IWithTimers
{
    public MyActor() { }

    public ITimerScheduler Timers { get; set; } = null!;
}

""",

        // UntypedActor without ITimerScheduler method calls
"""
// 04
using Akka.Actor;

public class MyActor: UntypedActor, IWithTimers
{
    public MyActor() { }

    protected override void OnReceive(object message) { }
    
    public ITimerScheduler Timers { get; set; } = null!;
}
""",

        // Non-Actor class that implements IWithTimers and have the same `AroundPreRestart()` and `PreRestart()`
        // methods fingerprints, we're not responsible for this.
"""
// 05
using System;
using Akka.Actor;

public class MyNonActorClass: MyNonActorBaseClass, IWithTimers
{
    public MyNonActorClass(ITimerScheduler scheduler)
    {
        Timers = scheduler;
    }

    public override void AroundPreRestart(Exception cause, object message)
    {
        base.AroundPreRestart(cause, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    protected override void PreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; }
}

public abstract class MyNonActorBaseClass
{
    public virtual void AroundPreRestart(Exception cause, object message) { }

    protected virtual void PreRestart(Exception reason, object message) { }
}
""",
    };

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData, object[] arguments)>
        FailureCases = new()
        {
            // ReceiveActor with ITimerScheduler.StartSingleTimer call inside AroundPreStart
            (
"""
// 01
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    public override void AroundPreRestart(Exception cause, object message)
    {
        base.AroundPreRestart(cause, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 73), ["StartSingleTimer", "AroundPreRestart"]),
            
            // ReceiveActor with ITimerScheduler.StartPeriodicTimer call inside AroundPreStart, variant 1
            (
"""
// 02
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    public override void AroundPreRestart(Exception cause, object message)
    {
        base.AroundPreRestart(cause, message);
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 75), ["StartPeriodicTimer", "AroundPreRestart"]),
            
            // ReceiveActor with ITimerScheduler.StartPeriodicTimer call inside AroundPreStart, variant 2
            (
"""
// 03
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    public override void AroundPreRestart(Exception cause, object message)
    {
        base.AroundPreRestart(cause, message);
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 100), ["StartPeriodicTimer", "AroundPreRestart"]),
            
            // ReceiveActor with ITimerScheduler.StartSingleTimer call inside PreStart
            (
"""
// 04
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    protected override void PreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 73), ["StartSingleTimer", "PreRestart"]),
            
            // ReceiveActor with ITimerScheduler.StartPeriodicTimer call inside PreStart, variant 1
            (
"""
// 05
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    protected override void PreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 75), ["StartPeriodicTimer", "PreRestart"]),
            
            // ReceiveActor with ITimerScheduler.StartPeriodicTimer call inside PreStart, variant 2
            (
"""
// 06
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    protected override void PreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 100), ["StartPeriodicTimer", "PreRestart"]),
          
            // UntypedActor with ITimerScheduler.StartSingleTimer call inside AroundPreStart
            (
"""
// 07
using System;
using Akka.Actor;

public sealed class MyActor : UntypedActor, IWithTimers
{
    public override void AroundPreRestart(Exception cause, object message)
    {
        base.AroundPreRestart(cause, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }
    
    protected override void OnReceive(object message) { }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 73), ["StartSingleTimer", "AroundPreRestart"]),
            
            // UntypedActor with ITimerScheduler.StartPeriodicTimer call inside AroundPreStart, variant 1
            (
"""
// 08
using System;
using Akka.Actor;

public sealed class MyActor : UntypedActor, IWithTimers
{
    public override void AroundPreRestart(Exception cause, object message)
    {
        base.AroundPreRestart(cause, message);
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
    }
    
    protected override void OnReceive(object message) { }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 75), ["StartPeriodicTimer", "AroundPreRestart"]),
            
            // UntypedActor with ITimerScheduler.StartPeriodicTimer call inside AroundPreStart, variant 2
            (
"""
// 09
using System;
using Akka.Actor;

public sealed class MyActor : UntypedActor, IWithTimers
{
    public override void AroundPreRestart(Exception cause, object message)
    {
        base.AroundPreRestart(cause, message);
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }
    
    protected override void OnReceive(object message) { }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 100), ["StartPeriodicTimer", "AroundPreRestart"]),
            
            // UntypedActor with ITimerScheduler.StartSingleTimer call inside PreStart
            (
"""
// 10
using System;
using Akka.Actor;

public sealed class MyActor : UntypedActor, IWithTimers
{
    protected override void PreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }
    
    protected override void OnReceive(object message) { }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 73), ["StartSingleTimer", "PreRestart"]),
            
            // UntypedActor with ITimerScheduler.StartPeriodicTimer call inside PreStart, variant 1
            (
"""
// 11
using System;
using Akka.Actor;

public sealed class MyActor : UntypedActor, IWithTimers
{
    protected override void PreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3));
    }
    
    protected override void OnReceive(object message) { }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 75), ["StartPeriodicTimer", "PreRestart"]),
            
            // UntypedActor with ITimerScheduler.StartPeriodicTimer call inside PreStart, variant 2
            (
"""
// 12
using System;
using Akka.Actor;

public sealed class MyActor : UntypedActor, IWithTimers
{
    protected override void PreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        Timers.StartPeriodicTimer("test", "test", TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }
    
    protected override void OnReceive(object message) { }

    public ITimerScheduler Timers { get; set; }
}
""", (10, 9, 10, 100), ["StartPeriodicTimer", "PreRestart"]),            
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
        (string testCode, (int startLine, int startColumn, int endLine, int endColumn) spanData, object[] arguments) d)
    {
        var expected = Verify.Diagnostic()
            .WithSpan(d.spanData.startLine, d.spanData.startColumn, d.spanData.endLine, d.spanData.endColumn)
            .WithSeverity(DiagnosticSeverity.Error)
            .WithArguments(d.arguments);

        return Verify.VerifyAnalyzer(d.testCode, expected);
    }

}