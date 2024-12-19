// -----------------------------------------------------------------------
//  <copyright file="MustNotInvokeStashMoreThanOnceInsideABlockSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit.Abstractions;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotInvokeStashMoreThanOnceAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class MustNotInvokeStashMoreThanOnceInsideABlockSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
    {
        // ReceiveActor with single Stash() invocation
"""
// 01
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor, IWithStash
{
    public MyActor()
    {
        Receive<string>(str => {
            Sender.Tell(str);
            Stash.Stash(); // should not flag this
        });
    }
    
    public void Handler()
    {
        Stash.Stash();
    }

    public IStash Stash { get; set; }
}
""",

        // Non-Actor class that has Stash() methods, we're not responsible for this.
"""
// 02
public interface INonAkkaStash
{
    public void Stash();
}

public class NonAkkaStash : INonAkkaStash
{
    public void Stash() { }
}

public sealed class MyActor
{
    public MyActor()
    {
        Stash = new NonAkkaStash();
    }

    public void Test()
    {
        Stash.Stash();
        Stash.Stash(); // should not flag this
    }
    
    public INonAkkaStash Stash { get; }
}
""",

        // Non-Actor class that uses Stash(),
        // we're only responsible for checking usage inside ActorBase class and its descendants.
"""
// 03
using System;
using Akka.Actor;

public class MyActor: IWithStash
{
    public MyActor(IStash stash)
    {
        Stash = stash;
    }

    public void Test()
    {
        Stash.Stash();
        Stash.Stash(); // should not flag this
    }

    public IStash Stash { get; set; }
}
""",
        // Stash calls inside 2 different code branch
"""
// 04
using Akka.Actor;

public sealed class MyActor : ReceiveActor, IWithStash
{
    public MyActor(int n)
    {
        Receive<string>(str =>
        {
            if(n < 0)
            {
                Stash!.Stash();
            }
            else
            {
                Stash!.Stash(); // should not flag this
            }
        });
    }

    public IStash Stash { get; set; } = null!;
}
""",
    };

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        FailureCases = new()
        {
            // Receive actor invoking Stash()
            (
"""
// 01
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor, IWithStash
{
    public MyActor()
    {
        Receive<string>(str => 
        {
            Stash.Stash();
            Stash.Stash(); // Error
        });
    }

    public IStash Stash { get; set; } = null!;
}
""", (13, 13, 13, 26)),
            
            // Receive actor invoking Stash() inside and outside of a code branch
            (
"""
// 02
using System;
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor, IWithStash
{
    public MyActor(int n)
    {
        Receive<string>(str =>
        {
            if(n < 0)
            {
                Stash!.Stash();
            }
            
            Stash.Stash(); // Error
        });
    }

    public IStash Stash { get; set; } = null!;
}
""", (12, 13, 12, 105)),
            };
    
    private readonly ITestOutputHelper _output;
    
    public MustNotInvokeStashMoreThanOnceInsideABlockSpecs(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Theory]
    [MemberData(nameof(SuccessCases))]
    public Task SuccessCase(string testCode)
    {
        return Verify.VerifyAnalyzer(testCode);
    }

    [Theory]
    [MemberData(nameof(FailureCases))]
    public Task FailureCase(
        (string testCode, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var expected = Verify.Diagnostic()
            .WithSpan(d.spanData.startLine, d.spanData.startColumn, d.spanData.endLine, d.spanData.endColumn)
            .WithSeverity(DiagnosticSeverity.Error);

        return Verify.VerifyAnalyzer(d.testCode, expected);
    }

}

