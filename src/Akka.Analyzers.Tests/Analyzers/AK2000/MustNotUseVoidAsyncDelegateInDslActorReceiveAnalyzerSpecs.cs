// -----------------------------------------------------------------------
//  <copyright file="MustNotUseVoidAsyncDelegateInReceiveOrCommandSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis.Testing;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseVoidAsyncDelegateInDslActorReceiveAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK2000;

public class MustNotUseVoidAsyncDelegateInDslActorReceiveAnalyzerSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
	{
        // Normal ReceiveActor and Dsl actor usage
"""
using Akka.Actor;
using Akka.Actor.Dsl;

public class MyActor
{
    public MyActor(ActorSystem sys, IActorRef sender)
    {
        sys.ActorOf(act =>
        {
            act.Receive<int>((msg, _) => sender.Tell(msg));
            act.Receive<int>(_ => true, (msg, _) => sender.Tell(msg));
            act.Receive<int>((msg, _) => sender.Tell(msg), _ => true);
            
            act.Receive<int>(MessageHandler);
            act.Receive<int>(_ => true, MessageHandler);
            act.Receive<int>(MessageHandler, _ => true);
        }, "dslActor");
    }

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

""", [(12, 30, 15, 14)]),
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

""", [(12, 41, 15, 14)]),
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

""", [(12, 30, 15, 14)]),
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
            act.Receive<int>(async (msg, _) => await MessageHandler(msg));
        }, "dslActor");
    }
    
    private async Task MessageHandler(int message)
    { }
}
""", [(12, 30, 12, 73)]),
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
            act.Receive<int>(_ => true, MessageHandler);
        }, "dslActor");
    }
    
    private async void MessageHandler(int message, IActorContext context)
    { }
}
""", [(12, 41, 12, 55)]),
            (
    // Dsl actor, receive delegate variant 7
"""
// 07
using Akka.Actor;
using Akka.Actor.Dsl;
using System.Threading.Tasks;

public class MyClass
{
    public MyClass(ActorSystem sys)
    {
        sys.ActorOf(act =>
        {
            act.Receive<int>(_ => true, async (msg, _) => MessageHandler(msg));
        }, "dslActor");
    }
    
    private async Task MessageHandler(int message)
    { }
}
""", [(12, 41, 12, 78)]),
            (
    // Dsl actor, receive delegate variant 8
"""
// 08
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
            (
    // Dsl actor, receive delegate variant 9
"""
// 09
using Akka.Actor;
using Akka.Actor.Dsl;
using System.Threading.Tasks;

public class MyClass
{
    public MyClass(ActorSystem sys)
    {
        sys.ActorOf(act =>
        {
            act.Receive<int>(async (msg, _) => await MessageHandler(msg), _ => true);
        }, "dslActor");
    }
    
    private async Task MessageHandler(int message)
    { }
}

""", [(12, 30, 12, 73)]),
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