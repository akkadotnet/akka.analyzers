// -----------------------------------------------------------------------
//  <copyright file="ShouldNotUseSystemToCreateChildActorsFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.ShouldNotUseSystemToCreateChildActorsAnalyzer>;

namespace Akka.Analyzers.Tests.Fixes.AK1000;

public class ShouldNotUseSystemToCreateChildActorsFixerSpecs
{
    [Fact]
    public Task RemoveSystemMemberAccessAsync()
    {
        const string before =
            """
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
            """;

        const string  after =
            """
            using Akka.Actor;
            
            public class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    Context.ActorOf<ChildActor>();
                }
            }
            
            public class ChildActor : ReceiveActor
            {
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(7, 9, 7, 45);

        return Verify.VerifyCodeFix(before, after, ShouldNotUseSystemToCreateChildActorsFixer.Key_FixActorSystemActorOf,
            expectedDiagnostic);
    }

    [Fact]
    public Task RemoveSystemMemberAccessVariantAsync()
    {
        const string before =
            """
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
            """;

        const string  after =
            """
            using Akka.Actor;

            public class MyActor : ReceiveActor
            {
                public MyActor()
                {
                    Context.ActorOf(Props.Create(() => new ChildActor()));
                }
            }

            public class ChildActor : ReceiveActor
            {
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(7, 9, 7, 69);

        return Verify.VerifyCodeFix(before, after, ShouldNotUseSystemToCreateChildActorsFixer.Key_FixActorSystemActorOf,
            expectedDiagnostic);
    }

    [Fact]
    public Task ChangeActorSystemMemberAccessToContextAsync()
    {
        const string before =
            """
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
            """;

        const string  after =
            """
            using Akka.Actor;

            public class MyActor : ReceiveActor
            {
                public MyActor(ActorSystem system)
                {
                    Context.ActorOf<ChildActor>();
                }
            }

            public class ChildActor : ReceiveActor
            {
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(7, 9, 7, 37);

        return Verify.VerifyCodeFix(before, after, ShouldNotUseSystemToCreateChildActorsFixer.Key_FixActorSystemActorOf,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task ChangeActorSystemMemberAccessToContextVariantAsync()
    {
        const string before =
            """
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
            """;

        const string  after =
            """
            using Akka.Actor;
            
            public class MyActor : ReceiveActor
            {
                public MyActor(ActorSystem system)
                {
                    Context.ActorOf(Props.Create(() => new ChildActor()));
                }
            }
            
            public class ChildActor : ReceiveActor
            {
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(7, 9, 7, 61);

        return Verify.VerifyCodeFix(before, after, ShouldNotUseSystemToCreateChildActorsFixer.Key_FixActorSystemActorOf,
            expectedDiagnostic);
    }
}