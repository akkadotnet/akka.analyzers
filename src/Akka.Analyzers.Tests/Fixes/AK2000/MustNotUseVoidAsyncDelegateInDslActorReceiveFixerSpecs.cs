// -----------------------------------------------------------------------
//  <copyright file="MustNotUseVoidAsyncDelegateInReceiveFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Akka.Analyzers.Tests.Fixes.AK2000;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseVoidAsyncDelegateInDslActorReceiveAnalyzer>;

public class MustNotUseVoidAsyncDelegateInDslActorReceiveFixerSpecs
{
    public static readonly
        TheoryData<(string before, string after, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
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
""", 
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
            act.ReceiveAsync<int>(async (msg, _) =>
            {
                await Task.Yield();
            });
        }, "dslActor");
    }
}
""", 
(12, 30, 15, 14)),
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
""", 
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
            act.ReceiveAsync<int>(_ => true, async (msg, _) =>
            {
                await Task.Yield();
            });
        }, "dslActor");
    }
}
""", 
(12, 41, 15, 14)),
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

""", 
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
            act.ReceiveAsync<int>(async (msg, _) =>
            {
                await Task.Yield();
            }, _ => true);
        }, "dslActor");
    }
}

""", 
(12, 30, 15, 14)),
            
        };
    
    [Theory]
    [MemberData(nameof(DslActActorFailureCases))]
    public Task RenameDslActActorReceiveToReceiveAsync((string before, string after, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var (before, after, (startLine, startColumn, endLine, endColumn)) = d;
        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(startLine, startColumn, endLine, endColumn);

        return Verify.VerifyCodeFix(before, after, MustNotUseVoidAsyncDelegateInReceiveActorReceiveFixer.Key_FixReceiveWithVoidAsyncDelegate,
            expectedDiagnostic);
    }
    
    public static readonly
        TheoryData<(string data, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        DslActActorIgnoredFailureCases = new()
        {
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
""", 
(12, 30, 12, 44)),
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
""", 
(12, 41, 12, 55)),
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
""", 
(12, 30, 12, 44)),
        };
    
    [Theory]
    [MemberData(nameof(DslActActorIgnoredFailureCases))]
    public Task IgnoreComplexDslActActorReceive((string data, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var (data, (startLine, startColumn, endLine, endColumn)) = d;
        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(startLine, startColumn, endLine, endColumn);

        return Verify.VerifyCodeFix(data, data, MustNotUseVoidAsyncDelegateInReceiveActorReceiveFixer.Key_FixReceiveWithVoidAsyncDelegate,
            [expectedDiagnostic], [expectedDiagnostic]);
    }
    
}