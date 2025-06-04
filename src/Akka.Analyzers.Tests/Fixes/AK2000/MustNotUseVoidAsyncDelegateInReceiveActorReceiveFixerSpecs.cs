// -----------------------------------------------------------------------
//  <copyright file="MustNotUseVoidAsyncDelegateInReceiveFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Akka.Analyzers.Tests.Fixes.AK2000;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseVoidAsyncDelegateInReceiveActorReceiveAnalyzer>;

public class MustNotUseVoidAsyncDelegateInReceiveActorReceiveFixerSpecs
{
    public static readonly
        TheoryData<(string before, string after, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
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
""", 
"""
// 01
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<int>(async msg =>
        {
            await Task.Yield();
        });
    }
}
""", 
(9, 22, 12, 10)),
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
""", 
"""
// 02
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<int>(_ => true, async msg =>
        {
            await Task.Yield();
        });
    }
}
""", 
(9, 33, 12, 10)),
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
""", 
"""
// 03
using Akka.Actor;
using System.Threading.Tasks;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync(typeof(int), _ => true, async msg =>
        {
            await Task.Yield();
        });
    }
}
""", 
(9, 41, 12, 10)),

        };
    
    [Theory]
    [MemberData(nameof(ReceiveActorFailureCases))]
    public Task RenameReceiveActorReceiveToReceiveAsync((string before, string after, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var (before, after, (startLine, startColumn, endLine, endColumn)) = d;
        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(startLine, startColumn, endLine, endColumn);

        return Verify.VerifyCodeFix(before, after, MustNotUseVoidAsyncDelegateInReceiveActorReceiveFixer.Key_FixReceiveWithVoidAsyncDelegate,
            expectedDiagnostic);
    }

    public static readonly
        TheoryData<(string data, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        ReceiveActorIgnoredFailureCases = new()
        {
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
""", 
(9, 22, 9, 36)),
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
""", 
(9, 33, 9, 47)),
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
""", 
(9, 41, 9, 55)),
            
        };    
    [Theory]
    [MemberData(nameof(ReceiveActorIgnoredFailureCases))]
    public Task IgnoreComplexReceiveActorReceive((string data, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var (data, (startLine, startColumn, endLine, endColumn)) = d;
        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(startLine, startColumn, endLine, endColumn);

        return Verify.VerifyCodeFix(data, data, MustNotUseVoidAsyncDelegateInReceiveActorReceiveFixer.Key_FixReceiveWithVoidAsyncDelegate, 
            [expectedDiagnostic], [expectedDiagnostic]);
    }
    
}