using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustCloseOverSenderWhenUsingPipeToAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class MustCloseOverSenderWhenUsingPipeToSpecs
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
        }"
    };
    
    [Theory]
    [MemberData(nameof(SuccessCases))]
    public async Task SuccessCase(string testCode)
    {
        await Verify.VerifyAnalyzer(testCode).ConfigureAwait(true);
    }

    public static readonly TheoryData<string> FailureCases = new()
    {
        // Receive actor using PipeTo without a closure inside a Receive<T> block
        @"using Akka.Actor;
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
        }",
    };
    
    [Theory]
    [MemberData(nameof(FailureCases))]
    public async Task FailureCase(string testCode)
    {
        var expected = Verify.Diagnostic()
            .WithSpan(14, 37, 14, 43)
            .WithArguments("Sender")
            .WithSeverity(DiagnosticSeverity.Error);
        
        await Verify.VerifyAnalyzer(testCode, expected).ConfigureAwait(true);
    }
}