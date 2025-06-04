// -----------------------------------------------------------------------
//  <copyright file="MustNotUseVoidAsyncDelegateInReceiveOrCommandSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis.Testing;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseVoidAsyncDelegateInReceiveAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK2000;

public class MustNotUseVoidAsyncDelegateInReceiveSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
	{
        // Normal ReceiveActor and Dsl actor usage
"""
using Akka.Actor;
using Akka.Actor.Dsl;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        Receive<int>(msg => Sender.Tell(msg));
        Receive<int>(_ => true, msg => Sender.Tell(msg));
        Receive(typeof(int), msg => Sender.Tell(msg));
        
        Receive<int>(MessageHandler);
        Receive<int>(_ => true, MessageHandler);
        Receive(typeof(int), MessageHandler);

        Context.ActorOf(act =>
        {
            act.Receive<int>((msg, _) => Sender.Tell(msg));
            act.Receive<int>(_ => true, (msg, _) => Sender.Tell(msg));
            act.Receive<int>((msg, _) => Sender.Tell(msg), _ => true);
            
            act.Receive<int>(MessageHandler);
            act.Receive<int>(_ => true, MessageHandler);
            act.Receive<int>(MessageHandler, _ => true);
        }, "dslActor");
    }

    private void MessageHandler(object msg) { }
    private void MessageHandler(int msg) { }
    private void MessageHandler(int msg, IActorContext context) { }
}
""",
	};
	
    [Theory]
    [MemberData(nameof(SuccessCases))]
    public Task SuccessCase(string code)
    {
        return Verify.VerifyAnalyzer(code);
    }

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn)[] spanData)>
        ReceiveActorFailureCases = new()
        {
            (
    // ReceiveActor, receive delegate variant 1
"""
// 01
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        Receive<int>(async msg =>
        {
            await Task.Yield();
        });
    }
}
""", [(9, 22, 9, 27)]),
            (
    // ReceiveActor, receive delegate variant 2
"""
// 02
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        Receive<int>(_ => true, async msg =>
        {
            await Task.Yield();
        });
    }
}
""", [(9, 33, 9, 38)]),
            (
    // ReceiveActor, receive delegate variant 3
"""
// 03
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        Receive(typeof(int), _ => true, async msg =>
        {
            await Task.Yield();
        });
    }
}
""", [(9, 41, 9, 46)]),
            (
    // ReceiveActor, receive delegate variant 4
"""
// 04
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        Receive<int>(MessageHandler);
    }

    private async void MessageHandler(int msg) { }
}
""", [(9, 22, 9, 36)]),
            (
    // ReceiveActor, receive delegate variant 5
"""
// 05
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        Receive<int>(_ => true, MessageHandler);
    }

    private async void MessageHandler(int msg) { }
}
""", [(9, 33, 9, 47)]),
            (
    // ReceiveActor, receive delegate variant 6
"""
// 06
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        Receive(typeof(int), _ => true, MessageHandler);
    }

    private async void MessageHandler(object msg) { }
}
""", [(9, 41, 9, 55)]),
            
        };
    
    [Theory]
    [MemberData(nameof(ReceiveActorFailureCases))]
    public async Task ReceiveActorFailureCase((string testData, (int startLine, int startColumn, int endLine, int endColumn)[] spanData) d)
    {
        var (testData, spanData) = d;
        var expectedDiagnostics = new DiagnosticResult[spanData.Length];
        var currentDiagnosticIndex = 0;
            
        // there can be multiple violations per test case
        foreach (var (startLine, startColumn, endLine, endColumn) in spanData)
        {
            expectedDiagnostics[currentDiagnosticIndex++] = Verify.Diagnostic().WithSpan(startLine, startColumn, endLine, endColumn);
        }
            
        await Verify.VerifyAnalyzer(testData, expectedDiagnostics).ConfigureAwait(true);
    }

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn)[] spanData)>
        DslActActorFailureCases = new()
        {
            (
    // Dsl actor, receive delegate variant 1
"""
// 01
using Akka.Actor;
using Akka.Actor.Dsl;
using System.Threading.Tasks;

public class MyClass
{
    public MyClass(ActorSystem sys)
    {
        sys.ActorOf(act =>
        {
            act.Receive<int>(async (msg, _) =>
            {
                await Task.Yield();
            });
        }, "dslActor");
    }
}

""", [(12, 30, 12, 35)]),
            (
    // Dsl actor, receive delegate variant 2
"""
// 02
using Akka.Actor;
using Akka.Actor.Dsl;
using System.Threading.Tasks;

public class MyClass
{
    public MyClass(ActorSystem sys)
    {
        sys.ActorOf(act =>
        {
            act.Receive<int>(_ => true, async (msg, _) =>
            {
                await Task.Yield();
            });
        }, "dslActor");
    }
}

""", [(12, 41, 12, 46)]),
            (
    // Dsl actor, receive delegate variant 3
"""
// 03
using Akka.Actor;
using Akka.Actor.Dsl;
using System.Threading.Tasks;

public class MyClass
{
    public MyClass(ActorSystem sys)
    {
        sys.ActorOf(act =>
        {
            act.Receive<int>(async (msg, _) =>
            {
                await Task.Yield();
            }, _ => true);
        }, "dslActor");
    }
}

""", [(12, 30, 12, 35)]),
            (
    // Dsl actor, receive delegate variant 4
"""
// 04
using Akka.Actor;
using Akka.Actor.Dsl;
using System.Threading.Tasks;

public class MyClass
{
    public MyClass(ActorSystem sys)
    {
        sys.ActorOf(act =>
        {
            act.Receive<int>(MessageHandler);
        }, "dslActor");
    }
    
    private async void MessageHandler(int message, IActorContext context)
    { }
}

""", [(12, 30, 12, 44)]),
            (
    // Dsl actor, receive delegate variant 5
"""
// 05
using Akka.Actor;
using Akka.Actor.Dsl;
using System.Threading.Tasks;

public class MyClass
{
    public MyClass(ActorSystem sys)
    {
        sys.ActorOf(act =>
        {
            act.Receive<int>(_ => true, MessageHandler);
        }, "dslActor");
    }
    
    private async void MessageHandler(int message, IActorContext context)
    { }
}

""", [(12, 41, 12, 55)]),
            (
    // Dsl actor, receive delegate variant 6
"""
// 06
using Akka.Actor;
using Akka.Actor.Dsl;
using System.Threading.Tasks;

public class MyClass
{
    public MyClass(ActorSystem sys)
    {
        sys.ActorOf(act =>
        {
            act.Receive<int>(MessageHandler, _ => true);
        }, "dslActor");
    }
    
    private async void MessageHandler(int message, IActorContext context)
    { }
}

""", [(12, 30, 12, 44)]),
        };
    
    [Theory]
    [MemberData(nameof(DslActActorFailureCases))]
    public async Task DslActActorFailureCase((string testData, (int startLine, int startColumn, int endLine, int endColumn)[] spanData) d)
    {
        var (testData, spanData) = d;
        var expectedDiagnostics = new DiagnosticResult[spanData.Length];
        var currentDiagnosticIndex = 0;
            
        // there can be multiple violations per test case
        foreach (var (startLine, startColumn, endLine, endColumn) in spanData)
        {
            expectedDiagnostics[currentDiagnosticIndex++] = Verify.Diagnostic().WithSpan(startLine, startColumn, endLine, endColumn);
        }
            
        await Verify.VerifyAnalyzer(testData, expectedDiagnostics).ConfigureAwait(true);
    }
    
}