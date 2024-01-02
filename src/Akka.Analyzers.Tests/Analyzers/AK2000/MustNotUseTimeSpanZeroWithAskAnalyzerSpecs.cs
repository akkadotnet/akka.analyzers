// -----------------------------------------------------------------------
//  <copyright file="MustNotUseTimeSpanZeroWithAskAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.AK2000.MustNotUseTimeSpanZeroWithAskAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK2000;

public class MustNotUseTimeSpanZeroWithAskAnalyzerSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
    {
        // Explicit non-zero TimeSpan
        @"using Akka.Actor;
        using System.Threading.Tasks;
        using System;

        public static class MyActorCaller{
            public static Task<string> Call(IActorRef actor){
                return actor.Ask<string>(""hello"", TimeSpan.FromSeconds(1));
            }
        }",

        // Implicit timeout - handled by Akka.NET default timeout
        @"using Akka.Actor;
        using System.Threading.Tasks;
        using System;

        public static class MyActorCaller{
            public static Task<string> Call(IActorRef actor){
                return actor.Ask<string>(""hello"");
            }
        }"
    };

    [Theory]
    [MemberData(nameof(SuccessCases))]
    public Task SuccessCase(string code)
    {
        return Verify.VerifyAnalyzer(code);
    }

    [Fact]
    public Task FailureCaseExplicitZeroTimeSpanGeneric()
    {
        var code =
            @"using Akka.Actor;
 using System.Threading.Tasks;
 using System;

 public static class MyActorCaller{
     public static Task<string> Call(IActorRef actor){
         return actor.Ask<string>(""hello"", TimeSpan.Zero);
     }
 }";
        var expectedDiagnostic = Verify.Diagnostic().WithSpan(7, 44, 7, 57); // Adjust the line and character positions
        return Verify.VerifyAnalyzer(code, expectedDiagnostic);
    }

    [Fact]
    public Task FailureCaseExplicitZeroTimeSpanNonGeneric()
    {
        var code =
            @"using Akka.Actor;
 using System.Threading.Tasks;
 using System;

 public static class MyActorCaller{
     public static Task Call(IActorRef actor){
         return actor.Ask(""hello"", TimeSpan.Zero);
     }
 }";
        var expectedDiagnostic = Verify.Diagnostic().WithSpan(7, 36, 7, 49); // Adjust the line and character positions
        return Verify.VerifyAnalyzer(code, expectedDiagnostic);
    }


    [Fact]
    public Task FailureCaseExplicitZeroTimeSpanWithNamedArgumentAndOtherArguments()
    {
        var code =
            @"using Akka.Actor;
 using System.Threading.Tasks;
 using System;

 public static class MyActorCaller{
    public static Task<string> Call(IActorRef actor, string message){
         return actor.Ask<string>(message, timeout: TimeSpan.Zero, cancellationToken: default);
    }
}";
        var expectedDiagnostic = Verify.Diagnostic().WithSpan(7, 44, 7, 66);
        return Verify.VerifyAnalyzer(code, expectedDiagnostic);
    }

    [Fact(Skip = "Variable analysis not yet implemented")]
    public Task FailureCaseTimeSpanVariableSetToDefault()
    {
        var code =
            @"using Akka.Actor;
using System.Threading.Tasks;
using System;

public static class MyActorCaller{
 public static Task<string> Call(IActorRef actor){
     TimeSpan myTs = default;
     return actor.Ask<string>(""hello"", timeout: myTs);
 }
}";
        var expectedDiagnostic = Verify.Diagnostic().WithLocation(7, 19); // Adjust accordingly
        return Verify.VerifyAnalyzer(code, expectedDiagnostic);
    }

    [Fact(Skip = "Variable analysis not yet implemented")]
    public Task FailureCaseTimeSpanVariableSetToTimeSpanZero()
    {
        var code =
            @"using Akka.Actor;
using System.Threading.Tasks;
using System;

public static class MyActorCaller{
 public static Task<string> Call(IActorRef actor){
     TimeSpan myTs = TimeSpan.Zero;
     return actor.Ask<string>(""hello"", timeout: myTs);
 }
}";
        var expectedDiagnostic = Verify.Diagnostic().WithLocation(7, 19); // Adjust accordingly
        return Verify.VerifyAnalyzer(code, expectedDiagnostic);
    }
}