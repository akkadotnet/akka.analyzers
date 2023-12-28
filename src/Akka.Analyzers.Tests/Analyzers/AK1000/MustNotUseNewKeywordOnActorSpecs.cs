// -----------------------------------------------------------------------
//  <copyright file="MustNotUseNewKeywordOnActorSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Tests.Utility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using static Akka.Analyzers.RuleDescriptors;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseNewKeywordOnActorsAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class AkkaActorInstantiationAnalyzerTests
{
    public static readonly TheoryData<string> SuccessCases = new()
    {
        // ActorBase
        """
        using Akka.Actor;

        class MyActor : ActorBase {
            protected override bool Receive(object message) {
                return true;
            }
        }

        class Test
        {
            void Method()
            {
                ActorSystem sys = ActorSystem.Create("MySys");
                Props props = Props.Create(() => new MyActor());
                IActorRef realActorInstance = sys.ActorOf(props);
            }
        }
        """,
        
        // UntypedActor
        """
        using Akka.Actor;

        class MyActor : UntypedActor {
            protected override void OnReceive(object message) {
            }
        }

        class Test
        {
            void Method()
            {
                ActorSystem sys = ActorSystem.Create("MySys");
                Props props = Props.Create(() => new MyActor());
                IActorRef realActorInstance = sys.ActorOf(props);
            }
        }
        """,
        
        // ReceiveActor
        """
        using Akka.Actor;

        class MyActor : ReceiveActor {
            public MyActor(){
                ReceiveAny(_ => { });
            }
        }

        class Test
        {
            void Method()
            {
                ActorSystem sys = ActorSystem.Create("MySys");
                Props props = Props.Create(() => new MyActor());
                IActorRef realActorInstance = sys.ActorOf(props);
            }
        }
        """
    };
    
    [Theory]
    [MemberData(nameof(SuccessCases))]
    public async Task SuccessCase(string testCode)
    {
        await Verify.VerifyAnalyzer(testCode).ConfigureAwait(true);
    }

    [Fact]
    public async Task FailureCase()
    {
        const string testCode = """
                                using Akka.Actor;

                                class MyActor : ActorBase {
                                    protected override bool Receive(object message) {
                                        return true;
                                    }
                                }

                                class Test
                                {
                                    void Method()
                                    {
                                        MyActor actorInstance = new MyActor();
                                    }
                                }
                                """;
        
        var expected = Verify.Diagnostic()
            .WithSpan(13, 33, 13, 46)
            .WithArguments("MyActor")
            .WithSeverity(DiagnosticSeverity.Error);
        
        await Verify.VerifyAnalyzer(testCode, expected).ConfigureAwait(true);
    }
}
