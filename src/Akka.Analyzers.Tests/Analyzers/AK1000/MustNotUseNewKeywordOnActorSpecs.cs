// -----------------------------------------------------------------------
//  <copyright file="MustNotUseNewKeywordOnActorSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Tests.Utility;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using static Akka.Analyzers.RuleDescriptors;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseNewKeywordOnActorsAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class AkkaActorInstantiationAnalyzerTests
{
    [Fact]
    public async Task Analyzer_Should_Not_Report_Diagnostic_For_Valid_Usage()
    {
        var testCode = @"
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
        ActorSystem sys = ActorSystem.Create(""MySys"");
        Props props = Props.Create(() => new MyActor());
        IActorRef realActorInstance = sys.ActorOf(props);
    }
}";
        await Verify.VerifyAnalyzer(testCode).ConfigureAwait(true);
    }

    [Fact]
    public async Task Analyzer_Should_Report_Diagnostic_For_Invalid_Usage()
    {
        var testCode = @"
using Akka.Actor;

class MyActor : ActorBase { }

class Test
{
    void Method()
    {
        MyActor actorInstance = new MyActor();
    }
}";

        await new CSharpAnalyzerTest<MustNotUseNewKeywordOnActorsAnalyzer, DefaultVerifier>
        {
            TestCode = testCode,
            ExpectedDiagnostics =
            {
                // The diagnostic expected to be raised by the analyzer
                DiagnosticResult.CompilerError(Ak1000DoNotNewActors.Id).WithSpan(10, 31, 10, 45).WithArguments("MyActor"),
            },
            ReferenceAssemblies = ReferenceAssembliesHelper.CurrentAkka
        }.RunAsync().ConfigureAwait(false);
    }
}
