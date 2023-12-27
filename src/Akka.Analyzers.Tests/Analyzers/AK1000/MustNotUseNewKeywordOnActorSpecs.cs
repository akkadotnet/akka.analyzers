// -----------------------------------------------------------------------
//  <copyright file="MustNotUseNewKeywordOnActorSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Akka.Analyzers.Tests.Analyzers.AK1000;

public class AkkaActorInstantiationAnalyzerTests
{
    [Fact]
    public async Task Analyzer_Should_Not_Report_Diagnostic_For_Valid_Usage()
    {
        var testCode = @"
using Akka.Actor;

class MyActor : ActorBase { }

class Test
{
    void Method()
    {
        ActorSystem sys = ActorSystem.Create(""MySys"");
        Props props = Props.Create(() => new MyActor());
        IActorRef realActorInstance = sys.ActorOf(props);
    }
}";
        await new CSharpAnalyzerTest<MustNotUseNewKeywordOnActorsAnalyzer, XUnitVerifier>
        {
            TestCode = testCode,
        }.RunAsync().ConfigureAwait(true);
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

        await new CSharpAnalyzerTest<MustNotUseNewKeywordOnActorsAnalyzer, XUnitVerifier>
        {
            TestCode = testCode,
            ExpectedDiagnostics =
            {
                // The diagnostic expected to be raised by the analyzer
                DiagnosticResult.CompilerError("AkkaActorInstantiation").WithSpan(10, 31, 10, 45).WithArguments("MyActor"),
            },
        }.RunAsync().ConfigureAwait(false);
    }
}
