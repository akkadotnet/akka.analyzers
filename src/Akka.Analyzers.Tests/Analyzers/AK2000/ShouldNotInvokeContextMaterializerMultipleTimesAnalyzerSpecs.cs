// -----------------------------------------------------------------------
//  <copyright file="ShouldNotInvokeContextMaterializerMultipleTimesAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis.Testing;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.ShouldNotInvokeContextMaterializerMultipleTimesAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK2000;

public class ShouldNotInvokeContextMaterializerMultipleTimesAnalyzerSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
	{
        // Using ActorSystem.Materializer from a non-actor class should not trigger any warnings
"""
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyClass
{
    public MyClass(ActorSystem system)
    {
        var mat1 = system.Materializer();
        var mat2 = system.Materializer();

        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), system.Materializer());
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), system.Materializer());
    }
}
""",
    // Using ActorSystem.Materializer from an actor class should not trigger any warnings
"""
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        var mat1 = Context.System.Materializer();
        var mat2 = Context.System.Materializer();

        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.System.Materializer());
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.System.Materializer());
    }
}
""",
    // Using ActorSystem.Materializer from an actor class should not trigger any warnings
"""
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        var system = Context.System;
        var mat1 = system.Materializer();
        var mat2 = system.Materializer();

        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), system.Materializer());
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), system.Materializer());
    }
}
""",
// Using ActorMaterializerExtensions.Materializer() with ActorSystem argument from an actor class should not trigger any warnings
"""
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        var system = Context.System;
        var mat1 = ActorMaterializerExtensions.Materializer(system);
        var mat2 = ActorMaterializerExtensions.Materializer(system);

        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), ActorMaterializerExtensions.Materializer(system));
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), ActorMaterializerExtensions.Materializer(system));
    }
}
""",
    // Cached Context.Materializer should not trigger any warnings
"""
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        var mat = Context.Materializer();

        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), mat);
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), mat);
    }
}
""",
    // Cached Context.Materializer should not trigger any warnings
"""
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyActor: ReceiveActor
{
    private readonly ActorMaterializer _materializer = Context.Materializer();
    public MyActor()
    {
        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), _materializer);
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), _materializer);
    }
}
""",
    // Cached Context.Materializer should not trigger any warnings
"""
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyActor: ReceiveActor
{
    private ActorMaterializer Materializer { get; } = Context.Materializer();
    public MyActor()
    {
        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Materializer);
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Materializer);
    }
}
""",
	};
	
    [Theory]
    [MemberData(nameof(SuccessCases))]
    public Task SuccessCase(string code)
    {
        return Verify.VerifyAnalyzer(code);
    }

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn)[] spanData)>
        FailureCases = new()
        {
            (
    // Context.Materializer invoked multiple times
"""
// 01
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.Materializer());
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.Materializer());
        var source3 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.Materializer());
    }
}
""", new[]{(14, 42, 14, 64), (16, 42, 16, 64)}),
            (
    // Context.Materializer invoked multiple times
"""
// 02
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyActor: ReceiveActor
{
    private ActorMaterializer Materializer { get; } = Context.Materializer();
    public MyActor()
    {
        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Materializer);
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.Materializer());
        var source3 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.Materializer());
    }
}
""", new[]{(15, 42, 15, 64), (17, 42, 17, 64)}),
            (
    // Context.Materializer invoked multiple times, mixed with ActorSystem.Materializer()
    // Should not emit warning on Context.System.Materializer()
"""
// 03
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.Materializer());
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.System.Materializer());
        var source3 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.Materializer());
    }
}
""", new[]{(16, 42, 16, 64)}),
            (
    // ActorMaterializerExtensions.Materializer() invoked multiple times
"""
// 04
using System.Linq;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class MyActor: ReceiveActor
{
    public MyActor()
    {
        var source1 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), ActorMaterializerExtensions.Materializer(Context));
        var source2 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), Context.Materializer());
        var source3 = Source.From(Enumerable.Range(0, 100))
            .RunWith(Sink.Ignore<int>(), ActorMaterializerExtensions.Materializer(Context));
    }
}
""", new[]{(14, 42, 14, 64), (16, 42, 16, 91)}),
        };
    
    [Theory]
    [MemberData(nameof(FailureCases))]
    public async Task FailureCase((string testData, (int startLine, int startColumn, int endLine, int endColumn)[] spanData) d)
    {
        var (testData, spanData) = d;
        var expectedDiagnostics = new DiagnosticResult[spanData.Length];
        var currentDiagnosticIndex = 0;
            
        // there can be multiple violations per test case
        foreach (var (startLine, startColumn, endLine, endColumn) in spanData)
        {
            expectedDiagnostics[currentDiagnosticIndex++] = Verify.Diagnostic().WithSpan(startLine, startColumn, endLine, endColumn);
        }
            
        await Verify.VerifyAnalyzer(testData, expectedDiagnostics).ConfigureAwait(true);
    }
}