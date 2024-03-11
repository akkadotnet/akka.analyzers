// -----------------------------------------------------------------------
//  <copyright file="ShouldUseIWithTimersInsteadOfScheduleTellFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Xunit.Abstractions;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.ShouldUseIWithTimersInsteadOfScheduleTellAnalyzer>;

namespace Akka.Analyzers.Tests.Fixes.AK1000;

public class ShouldUseIWithTimersInsteadOfScheduleTellFixerSpecs
{
    public ShouldUseIWithTimersInsteadOfScheduleTellFixerSpecs(ITestOutputHelper output)
    {
        ShouldUseIWithTimersInsteadOfScheduleTellFixer.Output = output;
    }
    
    [Fact]
    public Task ReplaceScheduleTellOnceWithStartSingleTimer()
    {
        const string before =
            """
            using System;
            using Akka.Actor;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test-message", Context.Self);
                }
            }
            """;

        const string after =
            """
            using System;
            using Akka.Actor;

            public sealed class MyActor : ReceiveActor, IWithTimers
            {
                private const string TimerKey_3b718060 = "3b718060 - PLEASE REFACTOR THIS";
                public MyActor()
                {
                    Timers.StartSingleTimer(TimerKey_3b718060, "test-message", TimeSpan.FromSeconds(3));
                }
                public ITimerScheduler Timers { get; set; }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(8, 9, 8, 111)
            .WithArguments("ScheduleTell invocation");

        return Verify.VerifyCodeFix(before, after, ShouldUseIWithTimersInsteadOfScheduleTellFixer.Key_ScheduleTell,
            expectedDiagnostic);
    }

    [Fact]
    public Task ReplaceScheduleTellOnceWithStartSingleTimerButLeaveInterfaceAlone()
    {
        const string before =
            """
            using System;
            using Akka.Actor;

            public sealed class MyActor : ReceiveActor, IWithTimers
            {
                public MyActor()
                {
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test-message", Context.Self);
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
                private const string TimerKey_3b718060 = "3b718060 - PLEASE REFACTOR THIS";
                public MyActor()
                {
                    Timers.StartSingleTimer(TimerKey_3b718060, "test-message", TimeSpan.FromSeconds(3));
                }
            
                public ITimerScheduler Timers { get; set; }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(8, 9, 8, 111)
            .WithArguments("ScheduleTell invocation");

        return Verify.VerifyCodeFix(before, after, ShouldUseIWithTimersInsteadOfScheduleTellFixer.Key_ScheduleTell,
            expectedDiagnostic);
    }

    [Fact]
    public Task ReplaceScheduleTellRepeatedlyWithStartPeriodicTimer()
    {
        const string before =
            """
            using System;
            using Akka.Actor;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), Self, "test-message", Context.Self);
                }
            }
            """;

        const string after =
            """
            using System;
            using Akka.Actor;

            public sealed class MyActor : ReceiveActor, IWithTimers
            {
                private const string TimerKey_ed720b1d = "ed720b1d - PLEASE REFACTOR THIS";
                public MyActor()
                {
                    Timers.StartPeriodicTimer(TimerKey_ed720b1d, "test-message", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
                }
                public ITimerScheduler Timers { get; set; }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(8, 9, 8, 142)
            .WithArguments("ScheduleTell invocation");

        return Verify.VerifyCodeFix(before, after, ShouldUseIWithTimersInsteadOfScheduleTellFixer.Key_ScheduleTell,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task ShouldNotClobberExistingTimerKeyLikeIdentifiers()
    {
        const string before =
            """
            using System;
            using Akka.Actor;

            public sealed class MyActor : ReceiveActor
            {
                private const int TimerKey_1 = 999;
                
                public int TimerKey_2 => 999;
                
                public MyActor()
                {
                    Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), Self, "test-message", Context.Self);
                }
                
                public void TimerKey_3() { }
            }
            """;

        const string after =
            """
            using System;
            using Akka.Actor;

            public sealed class MyActor : ReceiveActor, IWithTimers
            {
                private const string TimerKey_ed720b1d = "ed720b1d - PLEASE REFACTOR THIS";
                private const int TimerKey_1 = 999;
                
                public int TimerKey_2 => 999;
                
                public MyActor()
                {
                    Timers.StartPeriodicTimer(TimerKey_ed720b1d, "test-message", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
                }
                
                public void TimerKey_3() { }
                public ITimerScheduler Timers { get; set; }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(12, 9, 12, 142)
            .WithArguments("ScheduleTell invocation");

        return Verify.VerifyCodeFix(before, after, ShouldUseIWithTimersInsteadOfScheduleTellFixer.Key_ScheduleTell,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task ShouldBeAbleToFixMultipleOccurence()
    {
        const string before =
            """
            using System;
            using Akka.Actor;

            public sealed class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), Self, "test-message1", Context.Self);
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test-message2", Context.Self);
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(4), Self, "test-message3", Context.Self);
                }
            }
            """;

        var after = new[]
        {
            """
            using System;
            using Akka.Actor;

            public sealed class MyActor : ReceiveActor, IWithTimers
            {
                private const string TimerKey_68ecbee5 = "68ecbee5 - PLEASE REFACTOR THIS";
                public MyActor()
                {
                    Timers.StartPeriodicTimer(TimerKey_68ecbee5, "test-message1", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self, "test-message2", Context.Self);
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(4), Self, "test-message3", Context.Self);
                }
                public ITimerScheduler Timers { get; set; }
            }
            """,
        };
        
        const string afterAll =
            """
            using System;
            using Akka.Actor;

            public sealed class MyActor : ReceiveActor, IWithTimers
            {
                private const string TimerKey_c9025011 = "c9025011 - PLEASE REFACTOR THIS";
                private const string TimerKey_547b7de9 = "547b7de9 - PLEASE REFACTOR THIS";
                private const string TimerKey_68ecbee5 = "68ecbee5 - PLEASE REFACTOR THIS";
                public MyActor()
                {
                    Timers.StartPeriodicTimer(TimerKey_68ecbee5, "test-message1", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
                    Timers.StartSingleTimer(TimerKey_547b7de9, "test-message2", TimeSpan.FromSeconds(3));
                    Timers.StartSingleTimer(TimerKey_c9025011, "test-message3", TimeSpan.FromSeconds(4));
                }
                public ITimerScheduler Timers { get; set; }
            }
            """;

        var expectedDiagnostics = new[] {
            Verify.Diagnostic()
                .WithSpan(9, 9, 9, 112)
                .WithArguments("ScheduleTell invocation"),
            Verify.Diagnostic()
                .WithSpan(8, 9, 8, 143)
                .WithArguments("ScheduleTell invocation"),
            Verify.Diagnostic()
                .WithSpan(10, 9, 10, 112)
                .WithArguments("ScheduleTell invocation"),
        };

        return Verify.VerifyCodeFix(before, afterAll, ShouldUseIWithTimersInsteadOfScheduleTellFixer.Key_ScheduleTell, expectedDiagnostics);
    }
}