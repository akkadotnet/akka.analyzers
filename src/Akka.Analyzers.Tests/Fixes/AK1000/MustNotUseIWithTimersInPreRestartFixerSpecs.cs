// -----------------------------------------------------------------------
//  <copyright file="MustNotUseIWithTimersInPreRestartFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseIWithTimersInPreRestartAnalyzer>;

namespace Akka.Analyzers.Tests.Fixes.AK1000;

public class MustNotUseIWithTimersInPreRestartFixerSpecs
{
    [Fact]
    public Task MoveStartSingleTimerFromPreRestartToNewPostRestart()
    {
        const string before = 
"""
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
""";

        const string after = 
"""
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    protected override void PreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
    }

    public ITimerScheduler Timers { get; set; }

    protected override void PostRestart(Exception reason)
    {
        base.PostRestart(reason);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }
}
""";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(9, 9, 9, 73)
            .WithArguments("StartSingleTimer", "PreRestart");

        return Verify.VerifyCodeFix(
            before: before,
            after: after, 
            fixerActionKey: MustNotUseIWithTimersInPreRestartFixer.Key_FixITimerScheduler, 
            diagnostics: expectedDiagnostic);
    }
    
    [Fact]
    public Task MoveStartSingleTimerFromPreRestartToExistingPostRestart()
    {
        const string before = 
"""
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    protected override void PreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }

    protected override void PostRestart(Exception reason)
    {
        base.PostRestart(reason);
    }

    public ITimerScheduler Timers { get; set; }
}
""";

        const string after = 
"""
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    protected override void PreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
    }

    protected override void PostRestart(Exception reason)
    {
        base.PostRestart(reason);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; }
}
""";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(9, 9, 9, 73)
            .WithArguments("StartSingleTimer", "PreRestart");

        return Verify.VerifyCodeFix(
            before: before,
            after: after, 
            fixerActionKey: MustNotUseIWithTimersInPreRestartFixer.Key_FixITimerScheduler, 
            diagnostics: expectedDiagnostic);
    }
    
    [Fact]
    public Task MoveStartSingleTimerFromAroundPreRestartToNewPostRestart()
    {
        const string before = 
"""
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    public override void AroundPreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; }
}
""";

        const string after = 
"""
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    public override void AroundPreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
    }

    public ITimerScheduler Timers { get; set; }

    protected override void PostRestart(Exception reason)
    {
        base.PostRestart(reason);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }
}
""";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(9, 9, 9, 73)
            .WithArguments("StartSingleTimer", "AroundPreRestart");

        return Verify.VerifyCodeFix(
            before: before,
            after: after, 
            fixerActionKey: MustNotUseIWithTimersInPreRestartFixer.Key_FixITimerScheduler, 
            diagnostics: expectedDiagnostic);
    }
    
    [Fact]
    public Task MoveStartSingleTimerFromAroundPreRestartToExistingPostRestart()
    {
        const string before = 
"""
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    public override void AroundPreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }

    protected override void PostRestart(Exception reason)
    {
        base.PostRestart(reason);
    }

    public ITimerScheduler Timers { get; set; }
}
""";

        const string after = 
"""
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    public override void AroundPreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
    }

    protected override void PostRestart(Exception reason)
    {
        base.PostRestart(reason);
        Timers.StartSingleTimer("test", "test", TimeSpan.FromMinutes(3));
    }

    public ITimerScheduler Timers { get; set; }
}
""";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(9, 9, 9, 73)
            .WithArguments("StartSingleTimer", "AroundPreRestart");

        return Verify.VerifyCodeFix(
            before: before,
            after: after, 
            fixerActionKey: MustNotUseIWithTimersInPreRestartFixer.Key_FixITimerScheduler, 
            diagnostics: expectedDiagnostic);
    }
    
    [Fact]
    public Task ImpossibleToFixCodes()
    {
        const string before = 
"""
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    public override void AroundPreRestart(Exception cause, object message)
    {
        base.AroundPreRestart(cause, message);
        var timerKey = "timer-key"; // calculated timer key
        var timerMessage = "timer-message"; // calculated timer message
        var timeout = TimeSpan.FromMinutes(3); // calculated timeout
        Timers.StartSingleTimer(timerKey, timerMessage, timeout);
    }

    public ITimerScheduler Timers { get; set; }
}
""";

        const string after = 
"""
using System;
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithTimers
{
    public override void AroundPreRestart(Exception reason, object message)
    {
        base.PreRestart(reason, message);
        var timerKey = "timer-key"; // calculated timer key
        var timerMessage = "timer-message"; // calculated timer message
        var timeout = TimeSpan.FromMinutes(3); // calculated timeout
    }

    public ITimerScheduler Timers { get; set; }

    protected override void PostRestart(Exception reason)
    {
        base.PostRestart(reason);
        Timers.StartSingleTimer(timerKey, timerMessage, timeout);
    }
}
""";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(12, 9, 12, 65)
            .WithArguments("StartSingleTimer", "AroundPreRestart");

        return Verify.VerifyCodeFix(
            before: before,
            after: after, 
            fixerActionKey: MustNotUseIWithTimersInPreRestartFixer.Key_FixITimerScheduler, 
            diagnostics: expectedDiagnostic);
    }
}