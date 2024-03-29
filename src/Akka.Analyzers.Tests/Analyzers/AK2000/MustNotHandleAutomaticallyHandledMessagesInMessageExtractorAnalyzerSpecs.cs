﻿// -----------------------------------------------------------------------
//  <copyright file="MustNotHandleAutomaticallyHandledMessagesInMessageExtractorAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis.Testing;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK2000;

public class MustNotHandleAutomaticallyHandledMessagesInMessageExtractorAnalyzerSpecs
{
	public static readonly TheoryData<string> SuccessCases = new()
	{
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
""",
"""
using Akka.Cluster.Sharding;

public class MsgExtractorCreator{
    IMessageExtractor Create(){
       IMessageExtractor messageExtractor = HashCodeMessageExtractor.Create(100, msg =>
       {
        	if (msg is string s) {
        		return s;
        	}
        	else{
        		return null;
        	}
        });
    
        return messageExtractor;
    }
}
"""
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
// Simple message extractor edge case - using `if` statements
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
""", new[]{(18, 24, 18, 42)}),
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
        switch(message)
        {
            case string sharded:
                return sharded;
            case ShardingEnvelope e:
                return e.EntityId;
            default:
                return null;
        }
    }
}
""", new[]{(17, 18, 17, 36)}),
            
            // message extractor that uses a switch expression
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
        return message switch
        {
            string sharded => sharded,
            ShardingEnvelope e => e.EntityId,
            _ => null,
        };
    }
}
""", new[]{(16, 13, 16, 31)}),
            
        // multiple violations (one in each method)
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
		switch (message)
		{
			case string sharded:
				return sharded;
			case ShardingEnvelope e:
				return e.EntityId;
			default:
				return null;
		}
	}

	public override object EntityMessage(object message)
	{
		switch (message)
		{
			case string sharded:
				return sharded;
			case ShardRegion.StartEntity e:
				return e;
			default:
				return null;
		}
	}
}
""",
        new[]
        {
            (18, 9, 18, 27),  
            (31, 9, 31, 34)
        }),
        
        // combo mode - handle both types of forbidden messages in both methods
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
		switch (message)
		{
			case string sharded:
				return sharded;
			case ShardingEnvelope e:
				return e.EntityId;
			case ShardRegion.StartEntity start:
				return start.EntityId;
			default:
				return null;
		}
	}

	public override object EntityMessage(object message)
	{
		switch (message)
		{
			case string sharded:
				return sharded;
			case ShardingEnvelope e:
				return e.Message;
			case ShardRegion.StartEntity start:
				return start;
			default:
				return null;
		}
	}
}
""",
        new[]
        {
            (18, 9, 18, 27),  
            (20, 9, 20, 38),
            (33, 9, 33, 27),
            (35, 9, 35, 38),
        }),
        
        // custom IMessageExtractor implementation
        (
"""
using Akka.Cluster.Sharding;
using System;

public sealed class ShardMessageExtractor : IMessageExtractor
{
	/// <summary>
	/// We only ever run with a maximum of two nodes, so ~10 shards per node
	/// </summary>
	public ShardMessageExtractor()
	{
	}

	public string EntityId(object message)
	{
		switch (message)
		{
			case string sharded:
				return sharded;
			case ShardingEnvelope e:
				return e.EntityId;
			default:
				return null;
		}
	}

	public object EntityMessage(object message)
	{
		switch (message)
		{
			case string sharded:
				return sharded;
			case ShardingEnvelope e:
				return e.Message;
			default:
				return null;
		}
	}

	public string ShardId(object message)
	{
		return Random.Shared.Next(0,10).ToString();
	}
	
	public string ShardId(string entityId, object message)
	{
		return Random.Shared.Next(0,10).ToString();
	}
}
""", new[]
{
	(19, 9, 19, 27),  
	(32, 9, 32, 27)
}
	    ),
            
        // message extractor created by HashCode.MessageExtractor delegate
        (
"""
using Akka.Cluster.Sharding;

public class MsgExtractorCreator{
    IMessageExtractor Create(){
       IMessageExtractor messageExtractor = HashCodeMessageExtractor.Create(100, msg =>
       {
        	if (msg is string s) {
        		return s;
        	}
        	else if (msg is ShardingEnvelope shard) {
        		return shard.EntityId;
        	}
        	else{
        		return null;
        	}
        });
    
        return messageExtractor;
    }
}
""", new[]
{
    (10, 26, 10, 48)
})
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