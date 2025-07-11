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
        // 01
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
        // 02
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
        // 03
        // Should not report diagnostic: using string as zero parameter
        new object[]{
            """
// 03
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregate("", (acc, item) => acc + item.ToString(), sys.Materializer());
    }
}
"""
        },
        // 04
        // Should not report diagnostic: using bool as zero parameter
        new object[]{
            """
// 04
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregate(false, (acc, item) => item > 5, sys.Materializer());
    }
}
"""
        },
        // 05
        // Should not report diagnostic: using ImmutableArray as zero parameter
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
        source.RunAggregate(ImmutableArray<int>.Empty, (acc, item) => acc.Add(item), sys.Materializer());
    }
}
"""
        },
        // 06
        // Should not report diagnostic: using ImmutableDictionary as zero parameter
        new object[]{
            """
// 06
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
        // 07
        // Should not report diagnostic: using ImmutableHashSet as zero parameter
        new object[]{
            """
// 07
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
        // 01a
        // Should not report diagnostic: using ImmutableList as zero parameter (async variant)
        new object[]{
            """
// 01a
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Immutable;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregateAsync(ImmutableList<int>.Empty, async (acc, item) => acc.Add(item), sys.Materializer());
    }
}
"""
        },
        // 02a
        // Should not report diagnostic: using int (non-enumerable) as zero parameter (async variant)
        new object[]{
            """
// 02a
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregateAsync(0, async (acc, item) => acc + item, sys.Materializer());
    }
}
"""
        },
        // 03a
        // Should not report diagnostic: using string as zero parameter (async variant)
        new object[]{
            """
// 03a
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregateAsync("", async (acc, item) => acc + item.ToString(), sys.Materializer());
    }
}
"""
        },
        // 04a
        // Should not report diagnostic: using bool as zero parameter (async variant)
        new object[]{
            """
// 04a
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregateAsync(false, async (acc, item) => item > 5, sys.Materializer());
    }
}
"""
        },
        // 05a
        // Should not report diagnostic: using ImmutableArray as zero parameter (async variant)
        new object[]{
            """
// 05a
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Immutable;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregateAsync(ImmutableArray<int>.Empty, async (acc, item) => acc.Add(item), sys.Materializer());
    }
}
"""
        },
        // 06a
        // Should not report diagnostic: using ImmutableDictionary as zero parameter (async variant)
        new object[]{
            """
// 06a
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Immutable;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregateAsync(ImmutableDictionary<string, int>.Empty, async (acc, item) => acc.Add(item.ToString(), item), sys.Materializer());
    }
}
"""
        },
        // 07a
        // Should not report diagnostic: using ImmutableHashSet as zero parameter (async variant)
        new object[]{
            """
// 07a
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Immutable;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregateAsync(ImmutableHashSet<int>.Empty, async (acc, item) => acc.Add(item), sys.Materializer());
    }
}
"""
        },
    };

    public static IEnumerable<object[]> FailureCases => new[]
    {
        // 01
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
        // 02
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
        // 03
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
        // 04
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
        // 05
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
        // 06
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
        // 07
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
        // 08
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
        // 01a
        // Should report diagnostic: using mutable List as zero parameter (async variant)
        new object[]{
            """
// 01a
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregateAsync(new List<int>(), async (acc, item) => { acc.Add(item); return acc; }, sys.Materializer());
    }
}
""",
            new[]{ (14, 9, 14, 123) }
        },
        // 02a
        // Should report diagnostic: using mutable Dictionary as zero parameter (async variant)
        new object[]{
            """
// 02a
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var sys = ActorSystem.Create("TestSystem");
        var source = Source.Single(1);
        source.RunAggregateAsync(new Dictionary<string, int>(), async (acc, item) => { acc[item.ToString()] = item; return acc; }, sys.Materializer());
    }
}
""",
            new[]{ (14, 9, 14, 151) }
        },
        // 03a
        // Should report diagnostic: Flow.AggregateAsync with mutable List
        new object[]{
            """
// 03a
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var flow = Flow.Create<int>();
        flow.AggregateAsync(new List<int>(), async (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (12, 9, 12, 98) }
        },
        // 04a
        // Should report diagnostic: Sink.AggregateAsync with mutable List
        new object[]{
            """
// 04a
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var sink = Sink.AggregateAsync<int, List<int>>(new List<int>(), async (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (11, 20, 11, 125) }
        },
        // 05a
        // Should report diagnostic: Source.AggregateAsync with mutable List
        new object[]{
            """
// 05a
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var source = Source.Single(1);
        source.AggregateAsync(new List<int>(), async (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (12, 9, 12, 100) }
        },
        // 06a
        // Should report diagnostic: SubFlow.AggregateAsync with mutable List
        new object[]{
            """
// 06a
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var flow = Flow.Create<int>();
        var subFlow = flow.GroupBy(1, x => x);
        subFlow.AggregateAsync(new List<int>(), async (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (13, 9, 13, 101) }
        },
        // 07a
        // Should report diagnostic: multiple static aggregate async extension methods with mutable List
        new object[]{
            """
// 07a
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var flow = Flow.Create<int>();
        // Test static FlowOperations.AggregateAsync method
        flow.AggregateAsync(new List<int>(), async (acc, item) => { acc.Add(item); return acc; });
      
        // Test static Sink.AggregateAsync method
        var sink = Sink.AggregateAsync<int, List<int>>(new List<int>(), async (acc, item) => { acc.Add(item); return acc; });
      
        // Test static SourceOperations.AggregateAsync method
        var source = Source.Single(1);
        source.AggregateAsync(new List<int>(), async (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (13, 9, 13, 98), (16, 20, 16, 125), (20, 9, 20, 100) }
        },
        // 08a
        // Should report diagnostic: multiple static aggregate async methods with mutable List
        new object[]{
            """
// 08a
using Akka.Streams;
using Akka.Streams.Dsl;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestClass
{
    public void TestMethod()
    {
        var flow = Flow.Create<int>();
        // Test static FlowOperations.AggregateAsync method
        FlowOperations.AggregateAsync(flow, new List<int>(), async (acc, item) => { acc.Add(item); return acc; });
        
        // Test static Sink.AggregateAsync method
        var sink = Sink.AggregateAsync<int, List<int>>(new List<int>(), async (acc, item) => { acc.Add(item); return acc; });
        
        // Test static SourceOperations.AggregateAsync method
        var source = Source.Single(1);
        SourceOperations.AggregateAsync(source, new List<int>(), async (acc, item) => { acc.Add(item); return acc; });
    }
}
""",
            new[]{ (13, 9, 13, 114), (16, 20, 16, 125), (20, 9, 20, 118) }
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
                .WithSpan(span.Item1, span.Item2, span.Item3, span.Item4)
        ).ToArray();
        await Verify.VerifyAnalyzer(source, expected);
    }
} 