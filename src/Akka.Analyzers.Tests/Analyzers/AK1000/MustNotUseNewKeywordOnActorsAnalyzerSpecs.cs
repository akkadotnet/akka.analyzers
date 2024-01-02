// -----------------------------------------------------------------------
//  <copyright file="MustNotUseNewKeywordOnActorsAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseNewKeywordOnActorsAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class MustNotUseNewKeywordOnActorsAnalyzerSpecs
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
        """,
        
        // ReceiveActor with CTOR arguments
        """
        using Akka.Actor;
        
        class MyActor : ReceiveActor {
            private readonly string _name;
            private readonly int _myVar;
        
            public MyActor(string name, int myVar){
                _name = name;
                _myVar = myVar;
                ReceiveAny(_ => {
                    Sender.Tell(_name + _myVar);
                });
            }
        }
        
        class Test
        {
            void Method()
            {
                ActorSystem sys = ActorSystem.Create("MySys");
                Props props = Props.Create(() => new MyActor("foo", 1));
                IActorRef realActorInstance = sys.ActorOf(props);
            }
        }
        """,
        
        // ReceiveActor with explicit Akka.Actor.Props invocation (found this in real-world use)
        """
        using Akka.Actor;

        class MyActor : ReceiveActor {
            private readonly string _name;
            private readonly int _myVar;
        
            public MyActor(string name, int myVar){
                _name = name;
                _myVar = myVar;
                ReceiveAny(_ => {
                    Sender.Tell(_name + _myVar);
                });
            }
        }

        class Test
        {
            void Method()
            {
                ActorSystem sys = ActorSystem.Create("MySys");
                Akka.Actor.Props props = Akka.Actor.Props.Create(() => new MyActor("foo", 1));
                IActorRef realActorInstance = sys.ActorOf(props);
            }
        }
        """,
        
        // New expression is _eventually_ passed into Props, but not immediately at the call site
        """
        using Akka.Actor;
        using System;
        using System.Linq.Expressions;
        
        class MyActor : ReceiveActor {
            private readonly string _name;
            private readonly int _myVar;
        
            public MyActor(string name, int myVar){
                _name = name;
                _myVar = myVar;
                ReceiveAny(_ => {
                    Sender.Tell(_name + _myVar);
                });
            }
        }
        
        class Test
        {
            public ActorSystem Sys { get; }
            
            public Test(){
                Sys = ActorSystem.Create("MySys");
            }
        
            IActorRef Method<TActor>(Expression<Func<TActor>> factory) where TActor : ActorBase
            {
                return Sys.ActorOf(Props.Create(factory));
            }
            
            public IActorRef StartMyActor(){
                return Method(() => new MyActor("foo", 1));
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