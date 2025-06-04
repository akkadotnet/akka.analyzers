// -----------------------------------------------------------------------
//  <copyright file="MustNotUseVoidAsyncDelegateInReceiveOrCommandSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis.Testing;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseVoidAsyncDelegateInReceiveActorReceiveAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK2000;

public class MustNotUseVoidAsyncDelegateInReceiveActorReceiveAnalyzerSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
	{
        // Normal ReceiveActor usage
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
""", [(9, 22, 12, 10)]),
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
""", [(9, 33, 12, 10)]),
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
""", [(9, 41, 12, 10)]),
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
        Receive<int>(async msg => await MessageHandler(msg));
    }

    private async Task MessageHandler(int msg) { }
}
""", [(9, 22, 9, 60)]),
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
        Receive<int>(_ => true, MessageHandler);
    }

    private async void MessageHandler(int msg) { }
}
""", [(9, 33, 9, 47)]),
            (
    // ReceiveActor, receive delegate variant 7
"""
// 07
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        Receive<int>(_ => true, async msg => await MessageHandler(msg));
    }

    private async Task MessageHandler(int msg) { }
}
""", [(9, 33, 9, 71)]),
            (
    // ReceiveActor, receive delegate variant 8
"""
// 08
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
            (
    // ReceiveActor, receive delegate variant 8
"""
// 09
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        Receive(typeof(int), _ => true, async msg => await MessageHandler(msg));
    }

    private async Task MessageHandler(object msg) { }
}
""", [(9, 41, 9, 79)]),
            
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
}