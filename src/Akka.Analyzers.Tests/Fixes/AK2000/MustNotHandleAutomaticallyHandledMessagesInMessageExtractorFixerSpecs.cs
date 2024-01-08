﻿// -----------------------------------------------------------------------
//  <copyright file="MustNotHandleAutomaticallyHandledMessagesInMessageExtractorFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorAnalyzer>;

namespace Akka.Analyzers.Tests.Fixes.AK2000;

public class MustNotHandleAutomaticallyHandledMessagesInMessageExtractorFixerSpecs
{
    [Fact]
    public Task RemoveIfStatementFromMessageExtractor()
    {
        var before =
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
""";

        var after =
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
            
        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(18, 24, 18, 42);

        return Verify.VerifyCodeFix(before, after, MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorFixer.Key_FixAutomaticallyHandledShardedMessage,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task RemoveElseIfStatementFromMessageExtractor()
    {
        var before =
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
                    else if (message is ShardingEnvelope e)
                    {
                        return e.EntityId;
                    }
                    
                    return null;
                }
            }
            """;

        var after =
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
            
        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(17, 29, 17, 47);

        return Verify.VerifyCodeFix(before, after, MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorFixer.Key_FixAutomaticallyHandledShardedMessage,
            expectedDiagnostic);
    }

    [Fact]
    public Task RemoveCaseStatementFromMessageExtractorWithSwitch()
    {
        var before =
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
""";

        var after =
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
            default:
                return null;
        }
    }
}
""";
            
        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(17, 18, 17, 36);

        return Verify.VerifyCodeFix(before, after, MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorFixer.Key_FixAutomaticallyHandledShardedMessage,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task RemoveSwitchExpressionArm()
    {
        var before =
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
            """;

        var after =
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
                        _ => null,
                    };
                }
            }
            """;
            
        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(16, 13, 16, 31);

        return Verify.VerifyCodeFix(before, after, MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorFixer.Key_FixAutomaticallyHandledShardedMessage,
            expectedDiagnostic);
    }
    
    [Fact]
    public Task RemoveTwoSwitchExpressionArms()
    {
        var before =
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
                        ShardRegion.StartEntity start => start.EntityId,
                        _ => null,
                    };
                }
            }
            """;

        var after =
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
                        _ => null,
                    };
                }
            }
            """;

        var expectedDiagnostics = new[]
        {
            Verify.Diagnostic()
                .WithSpan(16, 13, 16, 31),
            Verify.Diagnostic()
                .WithSpan(17, 13, 17, 42),
        };

        return Verify.VerifyCodeFix(before, after, MustNotUseAutomaticallyHandledMessagesInsideMessageExtractorFixer.Key_FixAutomaticallyHandledShardedMessage,
            expectedDiagnostics);
    }
}