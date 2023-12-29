using Akka.Analyzers.Fixes.AK1000;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustCloseOverSenderWhenUsingPipeToAnalyzer>;

namespace Akka.Analyzers.Tests.Fixes.AK1000;

public class MustCloseOverSenderWhenUsingPipeToFixerSpecs
{
    [Fact]
    public Task AddClosureInsideReceiveActor()
    {
        var before = 
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
}";
        
        var after = 
@"using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor{

    public MyActor(){
        Receive<string>(str => {
            async Task<int> LocalFunction(){
                await Task.Delay(10);
                return str.Length;
            }
            var sender = this.Sender;

            // incorrect use of closure
            LocalFunction().PipeTo(sender); 
        });
    }
}";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(14, 29, 14, 35)
            .WithArguments("Sender");
        
        return Verify.VerifyCodeFix(before, after, MustCloseOverSenderWhenUsingPipeToFixer.Key_FixPipeToSender, expectedDiagnostic);
        
        //return Verify.VerifyCodeFix(before, after, MustCloseOverSenderWhenUsingPipeToFixer.Key_FixPipeToSender);
    }
}