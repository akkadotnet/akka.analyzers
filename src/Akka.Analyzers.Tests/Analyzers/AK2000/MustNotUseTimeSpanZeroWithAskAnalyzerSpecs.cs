// -----------------------------------------------------------------------
//  <copyright file="MustNotUseTimeSpanZeroWithAskAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseTimeSpanZeroWithAskAnalyzer>;

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
}