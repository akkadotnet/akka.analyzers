// -----------------------------------------------------------------------
//  <copyright file="MustCloseOverSenderWhenUsingPipeToAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustCloseOverSenderWhenUsingPipeToAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class MustCloseOverSenderWhenUsingPipeToAnalyzerSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
    {
        // ReceiveActor using PipeTo with a closure inside a Receive<string> block
        @"using Akka.Actor;
        using System.Threading.Tasks;

        public sealed class MyActor : ReceiveActor{

        public MyActor(){
            Receive<string>(str => {
             async Task<int> LocalFunction(){
                  await Task.Delay(10);
                  return str.Length;
             }

             // correct use of closure
             var sender = Sender;
             LocalFunction().PipeTo(sender); 
         });
        }
      }",

        // ReceiveActor using PipeTo calling a method from within a Receive<T> block
        @"using Akka.Actor;
        using System.Threading.Tasks;

        public sealed class MyActor : ReceiveActor{

        public MyActor(){
            Receive<string>(str => {
                Execute(str);
            });
        }

        private void Execute(string str){
           async Task<int> LocalFunction(){
                  await Task.Delay(10);
                  return str.Length;
             }

             // correct use of closure
             var sender = Sender;
             LocalFunction().PipeTo(sender); 
        }
    };",

        // Actor doing message handling in a Receive<T> block without PipeTo at all
        @"using Akka.Actor;
        
        public sealed class MyActor : ReceiveActor{

            public MyActor(){
                Receive<string>(str => {
                    Sender.Tell(str); // shouldn't flag this
                });
            }
        }",

        // Non-Actor class that has an IActorRef Sender property
        """
        using Akka.Actor;
        using System.Threading.Tasks;

        public class MyActor
        {
            public MyActor(IActorRef sender)
            {
                Sender = sender;
            }
        
            public IActorRef Sender { get; }
            
            public void Method()
            {
                async Task<int> LocalFunction(){
                               await Task.Delay(10);
                               return 11;
                           }
        
                // Sender is immutable on this custom non-Actor class, so shouldn't flag this
                LocalFunction().PipeTo(Sender);
            }
        }
        """
    };

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        FailureCases = new()
        {
            // Receive actor using PipeTo without a closure inside a Receive<T> block
            (@"using Akka.Actor;
        using System.Threading.Tasks;
        
        public sealed class MyActor : ReceiveActor{

            public MyActor(){
                Receive<string>(str => {
                    async Task<int> LocalFunction(){
                        await Task.Delay(10);
                        return str.Length;
                    }

                    // incorrect use of closure
                    LocalFunction().PipeTo(Sender); 
                });
            }
        }", (14, 37, 14, 43)),

            // UntypedActor using PipeTo without a closure inside a OnReceive block
            (@"using Akka.Actor;
        using System.Threading.Tasks;

        public sealed class MyActor : UntypedActor{

            protected override void OnReceive(object message){
                async Task<int> LocalFunction(){
                    await Task.Delay(10);
                    return message.ToString().Length;
                }

                // incorrect use of closure
                LocalFunction().PipeTo(Sender); 
            }
        }", (13, 33, 13, 39)),

            // Actor is using Sender as the "sender" property on PipeTo, rather than the recipient
            (@"using Akka.Actor;
        using System.Threading.Tasks;

        public sealed class MyActor : UntypedActor{

            protected override void OnReceive(object message){
                async Task<int> LocalFunction(){
                    await Task.Delay(10);
                    return message.ToString().Length;
                }

                // incorrect use of closure
                LocalFunction().PipeTo(Self, Sender); 
            }
        }", (13, 33, 13, 39)),

            // Actor is using Sender as the "sender" property on PipeTo, rather than the recipient
            ( // Actor is using this.Sender rather than just "Sender"
                """
                using Akka.Actor;
                using System.Threading.Tasks;
                
                public sealed class MyActor : UntypedActor{
                
                    protected override void OnReceive(object message){
                        async Task<int> LocalFunction(){
                            await Task.Delay(10);
                            return message.ToString().Length;
                        }
                
                        // incorrect use of closure
                        LocalFunction().PipeTo(this.Sender); 
                    }
                }
                """, (13, 25, 13, 31))
        };

    [Theory]
    [MemberData(nameof(SuccessCases))]
    public async Task SuccessCase(string testCode)
    {
        await Verify.VerifyAnalyzer(testCode).ConfigureAwait(true);
    }

    [Theory]
    [MemberData(nameof(FailureCases))]
    public async Task FailureCase(
        (string testCode, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var expected = Verify.Diagnostic()
            .WithSpan(d.spanData.startLine, d.spanData.startColumn, d.spanData.endLine, d.spanData.endColumn)
            .WithArguments("Sender")
            .WithSeverity(DiagnosticSeverity.Error);

        await Verify.VerifyAnalyzer(d.testCode, expected).ConfigureAwait(true);
    }
}