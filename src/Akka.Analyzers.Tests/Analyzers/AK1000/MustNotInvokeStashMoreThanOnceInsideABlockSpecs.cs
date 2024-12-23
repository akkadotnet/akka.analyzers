// -----------------------------------------------------------------------
//  <copyright file="MustNotInvokeStashMoreThanOnceInsideABlockSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
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

        public class MyActor
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
        
        // Stash calls inside 2 different code branch (switch statements)
        """
        // 05
        using Akka.Actor;

        public sealed class MyActor : ReceiveActor, IWithStash
        {
            public MyActor(int n)
            {
                Receive<string>(str =>
                {
                    switch(str){
                        case null:
                            Stash!.Stash();
                            break;
                        default:
                            Stash!.Stash(); // should not flag this
                            break;
                    }
                });
            }
        
            public IStash Stash { get; set; } = null!;
        }
        """,
    };

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn)[] spanData)>
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
                """, [
                    (12, 13, 12, 26),
                    (13, 13, 13, 26)
                ]),

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
                """, [(12, 13, 12, 105),
                    (14, 13, 14, 26)]),

            // UntypedActor invoking Stash() twice without branching
            (
                """
                // 03
                using Akka.Actor;

                public class MyUntypedActor : UntypedActor, IWithStash
                {
                    protected override void OnReceive(object message)
                    {
                        Stash.Stash();
                        Stash.Stash(); // Error
                    }
                
                    public IStash Stash { get; set; } = null!;
                }
                """, [(8, 9, 8, 22),
                    (9, 9, 9, 22)]),
            // UntypedActor invoking Stash() twice with a switch, but one is outside
            (
                """
                // 04
                using Akka.Actor;

                public class MyUntypedActor : UntypedActor, IWithStash
                {
                    protected override void OnReceive(object message)
                    {
                        Stash.Stash();
                        
                        switch(message)
                        {
                            case string s:
                                Stash.Stash(); // Error
                                break;
                        }
                    }
                
                    public IStash Stash { get; set; } = null!;
                }
                """, [(8, 9, 8, 22),
                    (13, 17, 13, 30)]),
            
            // UntypedActor invoking Stash() twice with a switch, but one is outside
            (
                """
                // 05
                using Akka.Actor;

                public class MyUntypedActor : UntypedActor, IWithStash
                {
                    protected override void OnReceive(object message)
                    {
                        Stash.Stash();
                        
                        if(message is string s)
                        {
                            Stash.Stash(); // Error
                        }
                    }
                
                    public IStash Stash { get; set; } = null!;
                }
                """, [(8, 9, 8, 22),
                    (12, 17, 12, 30)]),
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
        (string testCode, (int startLine, int startColumn, int endLine, int endColumn)[] spanData) d)
    {
        List<DiagnosticResult> expectedResults = new();
        
        foreach(var (startLine, startColumn, endLine, endColumn) in d.spanData)
        {
            var expected = Verify.Diagnostic().WithSpan(startLine, startColumn, endLine, endColumn).WithSeverity(DiagnosticSeverity.Error);
            expectedResults.Add(expected);
        }

        return Verify.VerifyAnalyzer(d.testCode, expectedResults.ToArray());
    }
}