// -----------------------------------------------------------------------
//  <copyright file="AkkaContext.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Akka.Analyzers;

/// <summary>
///     Provides information about the Akka.NET context (i.e. which libraries, which versions) in which the analyzer is
///     running.
/// </summary>
public sealed class AkkaContext
{
    private IAkkaCoreContext? _akkaCore;
    private IAkkaClusterContext? _akkaCluster;
    private IAkkaClusterShardingContext? _akkaClusterSharding;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AkkaContext" /> class.
    /// </summary>
    /// <param name="compilation">
    ///     The Roslyn compilation object used to look up types and
    ///     inspect references
    /// </param>
    public AkkaContext(Compilation compilation)
    {
        _akkaCore = AkkaCoreContext.Get(compilation);
        _akkaCluster = AkkaClusterContext.Get(compilation);
        _akkaClusterSharding = AkkaClusterShardingContext.Get(compilation);
    }

    private AkkaContext()
    {
    }

    /// <summary>
    ///     Data about the core Akka.NET library.
    /// </summary>
    public IAkkaCoreContext AkkaCore
    {
        get { return _akkaCore ??= EmptyCoreContext.Instance; }
    }

    /// <summary>
    ///     Does the current compilation context even have Akka.NET installed?
    /// </summary>
    public bool HasAkkaInstalled => AkkaCore != EmptyCoreContext.Instance;
    
    /// <summary>
    /// Symbol data and availability for Akka.Cluster.
    /// </summary>
    public IAkkaClusterContext AkkaCluster
    {
        get { return _akkaCluster ??= EmptyClusterContext.Instance; }
    }
    
    /// <summary>
    /// Does the current compilation context have Akka.Cluster installed?
    /// </summary>
    public bool HasAkkaClusterInstalled => AkkaCluster != EmptyClusterContext.Instance;
    
    /// <summary>
    /// Symbol data and availability for Akka.Cluster.Sharding.
    /// </summary>
    public IAkkaClusterShardingContext AkkaClusterSharding
    {
        get { return _akkaClusterSharding ??= EmptyAkkaClusterShardingContext.Instance; }
    }
    
    /// <summary>
    /// Does the current compilation context have Akka.Cluster.Sharding installed?
    /// </summary>
    public bool HasAkkaClusterShardingInstalled => AkkaClusterSharding != EmptyAkkaClusterShardingContext.Instance;
}