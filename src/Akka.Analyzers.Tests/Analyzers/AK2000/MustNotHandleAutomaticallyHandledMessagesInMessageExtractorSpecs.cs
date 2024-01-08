// -----------------------------------------------------------------------
//  <copyright file="MustNotHandleAutomaticallyHandledMessagesInMessageExtractorSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseAutomaticallyHandledMessagesInsideMessageExtractor>;

namespace Akka.Analyzers.Tests.Analyzers.AK2000;

public class MustNotHandleAutomaticallyHandledMessagesInMessageExtractorSpecs
{
    [Fact]
    public Task SuccessCase()
    {
        var code = 
"""
using Akka.Cluster.Sharding;

public sealed class ShardMessageExtractor : HashCodeMessageExtractor
{
    /// <summary>
    /// We only ever run with a maximum of two nodes, so ~10 shards per node
    /// </summary>
    public ShardMessageExtractor(int shardCount = 20) : base(shardCount)
    {
    }

    public override string EntityId(object message)
    {
        if(message is string sharded)
        {
            return sharded;
        }
        
        return null;
    }
}
""";
        
        return Verify.VerifyAnalyzer(code);
    }

    public static readonly
        TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        FailureCases = new()
        {
            (
"""
using Akka.Cluster.Sharding;
public sealed class ShardMessageExtractor : HashCodeMessageExtractor
{
    /// <summary>
    /// We only ever run with a maximum of two nodes, so ~10 shards per node
    /// </summary>
    public ShardMessageExtractor(int shardCount = 20) : base(shardCount)
    {
    }

    public override string EntityId(object message)
    {
        if(message is string sharded)
        {
            return sharded;
        }

        if (message is ShardingEnvelope e)
        {
            return e.EntityId;
        }

        return null;
    }
}
""", (13, 23, 13, 37)),
        };
    
    [Theory]
    [MemberData(nameof(FailureCases))]
    public async Task FailureCase((string testData, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var (testData, spanData) = d;
        var expectedDiagnostic = Verify.Diagnostic().WithSpan(spanData.startLine, spanData.startColumn, spanData.endLine, spanData.endColumn);
        await Verify.VerifyAnalyzer(testData, expectedDiagnostic).ConfigureAwait(true);
    }
}