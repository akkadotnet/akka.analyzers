// -----------------------------------------------------------------------
//  <copyright file="ShouldUseImmutableEnumerableForStreamAggregateAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Xunit.Abstractions;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.ShouldUseImmutableEnumerableForStreamAggregateAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK2000;

public class ShouldUseImmutableEnumerableForStreamAggregateAnalyzerSpecs
{
    private readonly ITestOutputHelper Output;
    
    public ShouldUseImmutableEnumerableForStreamAggregateAnalyzerSpecs(ITestOutputHelper output)
    {
        Output = output;
    }

    public static IEnumerable<object[]> SuccessCases => new[]
    {
        // Should not report diagnostic: using ImmutableList as zero parameter
        new object[]{
            """
// 01
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Immutable;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregate(ImmutableList<int>.Empty, (acc, item) => acc.Add(item), sys.Materializer());
    }
}
"""
        },
        // Should not report diagnostic: using int (non-enumerable) as zero parameter
        new object[]{
            """
// 02
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregate(0, (acc, item) => acc + item, sys.Materializer());
    }
}
"""
        },
        // Should not report diagnostic: using ImmutableArray as zero parameter
        new object[]{
            """
// 03
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Immutable;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregate(ImmutableArray<int>.Empty, (acc, item) => acc.Add(item), sys.Materializer());
    }
}
"""
        },
        // Should not report diagnostic: using ImmutableDictionary as zero parameter
        new object[]{
            """
// 04
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Immutable;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregate(ImmutableDictionary<string, int>.Empty, (acc, item) => acc.Add(item.ToString(), item), sys.Materializer());
    }
}
"""
        },
        // Should not report diagnostic: using ImmutableHashSet as zero parameter
        new object[]{
            """
// 05
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Immutable;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregate(ImmutableHashSet<int>.Empty, (acc, item) => acc.Add(item), sys.Materializer());
    }
}
"""
        },
    };

    public static IEnumerable<object[]> FailureCases => new[]
    {
        // Should report diagnostic: using mutable List as zero parameter
        new object[]{
            """
// 01
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregate(new List<int>(), (acc, item) => { acc.Add(item); return acc; },  sys.Materializer());
    }
}
""",
            new[]{ (13, 9, 13, 113) }
        },
        // Should report diagnostic: using mutable Dictionary as zero parameter
        new object[]{
            """
// 02
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregate(new Dictionary<string, int>(), (acc, item) => { acc[item.ToString()] = item; return acc; }, sys.Materializer());
    }
}
""",
            new[]{ (13, 9, 13, 140) }
        },
        // Should report diagnostic: Flow.Aggregate with mutable List
        new object[]{
            """
// 03
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;

public class TestClass
{
    public void TestMethod()
    {
        var flow = Flow.Create<int>();
        flow.Aggregate(new List<int>(), (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (11, 9, 11, 87) }
        },
        // Should report diagnostic: Sink.Aggregate with mutable List
        new object[]{
            """
// 04
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;

public class TestClass
{
    public void TestMethod()
    {
        var sink = Sink.Aggregate<int, List<int>>(new List<int>(), (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (10, 20, 10, 114) }
        },
        // Should report diagnostic: Source.Aggregate with mutable List
        new object[]{
            """
// 05
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;

public class TestClass
{
    public void TestMethod()
    {
        var source = Source.Single(1);
        source.Aggregate(new List<int>(), (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (11, 9, 11, 89) }
        },
        // Should report diagnostic: SubFlow.Aggregate with mutable List
        new object[]{
            """
// 06
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;

public class TestClass
{
    public void TestMethod()
    {
        var flow = Flow.Create<int>();
        var subFlow = flow.GroupBy(1, x => x);
        subFlow.Aggregate(new List<int>(), (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (12, 9, 12, 90) }
        },
        // Should report diagnostic: multiple static aggregate extension methods with mutable List
        new object[]{
            """
// 07
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;

public class TestClass
{
    public void TestMethod()
    {
        var flow = Flow.Create<int>();
        // Test static FlowOperations.Aggregate method
        flow.Aggregate(new List<int>(), (acc, item) => { acc.Add(item); return acc; });
      
        // Test static Sink.Aggregate method
        var sink = Sink.Aggregate<int, List<int>>(new List<int>(), (acc, item) => { acc.Add(item); return acc; });
      
        // Test static SourceOperations.Aggregate method
        var source = Source.Single(1);
        source.Aggregate(new List<int>(), (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (12, 9, 12, 87), (15, 20, 15, 114), (19, 9, 19, 89) }
        },
        // Should report diagnostic: multiple static aggregate methods with mutable List
        new object[]{
            """
// 08
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;

public class TestClass
{
    public void TestMethod()
    {
        var flow = Flow.Create<int>();
        // Test static FlowOperations.Aggregate method
        FlowOperations.Aggregate(flow, new List<int>(), (acc, item) => { acc.Add(item); return acc; });
        
        // Test static Sink.Aggregate method
        var sink = Sink.Aggregate<int, List<int>>(new List<int>(), (acc, item) => { acc.Add(item); return acc; });
        
        // Test static SourceOperations.Aggregate method
        var source = Source.Single(1);
        SourceOperations.Aggregate(source, new List<int>(), (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (12, 9, 12, 103), (15, 20, 15, 114), (19, 9, 19, 107) }
        },
    };

    [Theory]
    [MemberData(nameof(SuccessCases))]
    public async Task SuccessCase(string source)
    {
        await Verify.VerifyAnalyzer(source);
    }

    [Theory]
    [MemberData(nameof(FailureCases))]
    public async Task FailureCase(string source, (int, int, int, int)[] spans)
    {
        var expected = spans.Select(span =>
            Verify.Diagnostic("AK2007")
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("The generic parameter used as the initial state (zero) for aggregate operations must be an immutable type when it is an enumerable. Consider using `System.Collections.Immutable` types instead of mutable collections.")
                .WithSpan(span.Item1, span.Item2, span.Item3, span.Item4)
        ).ToArray();
        await Verify.VerifyAnalyzer(source, expected);
    }
} 