// -----------------------------------------------------------------------
//  <copyright file="ShouldNotUseReceiveAsyncWithoutAsyncLambdaExpressionSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.ShouldNotUseReceiveAsyncWithoutAsyncLambdaAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class ShouldNotUseReceiveAsyncWithoutAsyncLambdaAnalyzerSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
    {
        // ReceiveActor using ReceiveAsync with async keyword on the async lambda expression
"""
// 1
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str => {
            var sender = Sender;
            sender.Tell(await LocalFunction());
            return;
            
            async Task<int> LocalFunction()
            {
                await Task.Delay(10);
                return str.Length;
            }
        });
    }
}
""",

// ReceiveActor using ReceiveAsync with async keyword on the async lambda expression, alternate version
"""
// 2
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(str => true, async str => {
            var sender = Sender;
            sender.Tell(await LocalFunction());
            return;
            
            async Task<int> LocalFunction()
            {
                await Task.Delay(10);
                return str.Length;
            }
        });
    }
}
""",

        // ReceiveActor using ReceiveAsync with async keyword on the async lambda expression
"""
// 3
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor(){
        ReceiveAsync<string>(async str => {
            await Execute(str, Sender);
        });
    }

    private async Task Execute(string str, IActorRef sender){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return str.Length;
        }

        sender.Tell(await LocalFunction());
    }
}
""",

        // ReceiveActor using ReceiveAsync with async keyword on the async lambda expression, alternate version
"""
// 4
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor(){
        ReceiveAsync<string>(str => true, async str => {
            await Execute(str, Sender);
        });
    }

    private async Task Execute(string str, IActorRef sender){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return str.Length;
        }

        sender.Tell(await LocalFunction());
    }
}
""",

        // ReceiveActor using ReceiveAsync with async keyword on the one-liner async lambda expression
"""
// 5
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str => await Execute(str, Sender));
    }

    private async Task Execute(string str, IActorRef sender){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return str.Length;
        }

        sender.Tell(await LocalFunction());
    }
}
""",

        // ReceiveActor using ReceiveAsync with async keyword on the one-liner async lambda expression, alternate version
"""
// 6
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(str => true, async str => await Execute(str, Sender));
    }

    private async Task Execute(string str, IActorRef sender){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return str.Length;
        }

        sender.Tell(await LocalFunction());
    }
}
""",

        // ReceiveActor using ReceiveAsync with async keyword on the one-liner async lambda expression
"""
// 7
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str => Sender.Tell(await Execute(str)));
    }

    private async Task<int> Execute(string str){
        return str.Length;
    }
}
""",

        // ReceiveActor using ReceiveAsync with async keyword on the one-liner async lambda expression, alternate version
"""
// 8
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(str => true, async str => Sender.Tell(await Execute(str)));
    }

    private async Task<int> Execute(string str){
        return str.Length;
    }
}
""",

        // Identical ReceiveAsync and ReceiveAnyAsync method fingerprint in non-ReceiveActor class 
"""
// 9
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : UntypedActor
{
    public MyActor()
    {
        ReceiveAnyAsync(async o => Self.Tell(o));
        ReceiveAsync<string>(async s => Self.Tell(s));
    }

    protected override void OnReceive(object message) { }
    
    protected void ReceiveAsync<T>(Func<T, Task> handler, Predicate<T>? shouldHandle = null) { }
    protected void ReceiveAnyAsync(Func<object, Task> handler) { }
}
""",

        // ReceiveActor using ReceiveAsync with async method delegate, this needs to be handled by a different analyzer
"""
// 10
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(Handler);
    }
    
    private async Task Handler(string s) { }
}
"""
    };

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        FailureCases = new()
        {
            // ReceiveActor using ReceiveAsync without async keyword on the async lambda expression
            (
"""
// 1
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(str =>
        {
            Sender.Tell(str);
            return Task.CompletedTask;
        });
    }
}
""", (9, 9, 13, 11)),
            
            // ReceiveActor using ReceiveAsync without async keyword on the async lambda expression, alternate version
            (
"""
// 2
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(str => true, str =>
        {
            Sender.Tell(str);
            return Task.CompletedTask;
        });
    }
}
""", (9, 9, 13, 11)),
            
            // ReceiveActor using ReceiveAsync with async keyword on the async lambda expression
            // but without any awaited code
            (
"""
// 3
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(async str =>
        {
            Sender.Tell(str);
        });
    }
}
""", (9, 9, 12, 11)),
            
            // ReceiveActor using ReceiveAsync with async keyword on the async lambda expression
            // but without any awaited code, alternate version
            (
"""
// 4
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        ReceiveAsync<string>(str => true, async str =>
        {
            Sender.Tell(str);
        });
    }
}
""", (9, 9, 12, 11)),
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
            .WithArguments("lambda expression")
            .WithSeverity(DiagnosticSeverity.Warning);

        return Verify.VerifyAnalyzer(d.testCode, expected);
    }
}