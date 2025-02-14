// -----------------------------------------------------------------------
//  <copyright file="ShouldNotUseSystemToCreateChildActorsAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.ShouldNotUseSystemToCreateChildActorsAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class ShouldNotUseSystemToCreateChildActorsAnalyzerSpecs
{
        public static readonly TheoryData<string> SuccessCases = new()
    {
        // Non-actor class uses ActorSystem to create an actor
"""
using Akka.Actor;

public class SaveClass
{
    public SaveClass(ActorSystem system)
    {
        system.ActorOf(Props.Create(() => new ChildActor())); // Shouldn't flag this
        system.ActorOf<ChildActor>(); // Shouldn't flag this
    }
}

public class ChildActor : ReceiveActor
{
}
""",

        // Actors using Context to create child actors
"""
using Akka.Actor;

public class MyActor : ReceiveActor
{
    public MyActor()
    {
        Context.ActorOf(Props.Create(() => new ChildActor())); // Shouldn't flag this
        Context.ActorOf<ChildActor>(); // Shouldn't flag this
    }
}

public class ChildActor : ReceiveActor
{
}
""",
    };

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        FailureCases = new()
        {
            // ReceiveActor invoking ActorSystem.ActorOf() directly
            (
"""
// 01
using Akka.Actor;

public class MyActor : ReceiveActor
{
    public MyActor(ActorSystem system)
    {
        system.ActorOf(Props.Create(() => new ChildActor()));
    }
}

public class ChildActor : ReceiveActor
{
}
""", (8, 9, 8, 61)),
            
            // ReceiveActor invoking Context.System.ActorOf()
            (
"""
// 02
using Akka.Actor;

public class MyActor : ReceiveActor
{
    public MyActor()
    {
        Context.System.ActorOf(Props.Create(() => new ChildActor()));
    }
}

public class ChildActor : ReceiveActor
{
}
""", (8, 9, 8, 69)),
            
            // ReceiveActor invoking ActorOf<T>() extension on ActorSystem directly
            (
"""
// 03
using Akka.Actor;

public class MyActor : ReceiveActor
{
    public MyActor(ActorSystem system)
    {
        system.ActorOf<ChildActor>();
    }
}

public class ChildActor : ReceiveActor
{
}
""", (8, 9, 8, 37)),
            
            // ReceiveActor invoking ActorOf<T>() extension on Context.System
            (
"""
// 04
using Akka.Actor;

public class MyActor : ReceiveActor
{
    public MyActor()
    {
        Context.System.ActorOf<ChildActor>();
    }
}

public class ChildActor : ReceiveActor
{
}
""", (8, 9, 8, 45)),
            
            
            // UntypedActor invoking ActorSystem.ActorOf() directly
            (
"""
// 05
using Akka.Actor;

public class MyActor : UntypedActor
{
    public MyActor(ActorSystem system)
    {
        system.ActorOf(Props.Create(() => new ChildActor()));
    }

    protected override void OnReceive(object message)
    {
        throw new System.NotImplementedException();
    }
}

public class ChildActor : ReceiveActor
{
}
""", (8, 9, 8, 61)),
            
            // UntypedActor invoking Context.System.ActorOf()
            (
"""
// 06
using Akka.Actor;

public class MyActor : UntypedActor
{
    public MyActor()
    {
        Context.System.ActorOf(Props.Create(() => new ChildActor()));
    }

    protected override void OnReceive(object message)
    {
        throw new System.NotImplementedException();
    }
}

public class ChildActor : ReceiveActor
{
}
""", (8, 9, 8, 69)),
            
            // UntypedActor invoking ActorOf<T>() extension on ActorSystem directly
            (
"""
// 07
using Akka.Actor;

public class MyActor : UntypedActor
{
    public MyActor(ActorSystem system)
    {
        system.ActorOf<ChildActor>();
    }

    protected override void OnReceive(object message)
    {
        throw new System.NotImplementedException();
    }
}

public class ChildActor : ReceiveActor
{
}
""", (8, 9, 8, 37)),
            
            // UntypedActor invoking ActorOf<T>() extension on Context.System
            (
"""
// 08
using Akka.Actor;

public class MyActor : UntypedActor
{
    public MyActor()
    {
        Context.System.ActorOf<ChildActor>();
    }

    protected override void OnReceive(object message)
    {
        throw new System.NotImplementedException();
    }
}

public class ChildActor : ReceiveActor
{
}
""", (8, 9, 8, 45)),
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
            .WithSeverity(DiagnosticSeverity.Warning);

        return Verify.VerifyAnalyzer(d.testCode, expected);
    }

}