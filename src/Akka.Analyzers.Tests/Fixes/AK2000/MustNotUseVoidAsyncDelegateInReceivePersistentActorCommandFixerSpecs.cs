// -----------------------------------------------------------------------
//  <copyright file="MustNotUseVoidAsyncDelegateInReceiveFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;

namespace Akka.Analyzers.Tests.Fixes.AK2000;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseVoidAsyncDelegateInReceivePersistentActorCommandAnalyzer>;

public class MustNotUseVoidAsyncDelegateInReceivePersistentActorCommandFixerSpecs
{
    public static readonly
        TheoryData<(string before, string after, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        ReceiveActorFailureCases = new()
        {
            (
    // ReceiveActor, receive delegate variant 1
"""
// 01
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command<int>(async msg =>
        {
            await Task.Yield();
        });
    }
    
    public override string PersistenceId { get; }
}
""", 
"""
// 01
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        CommandAsync<int>(async msg =>
        {
            await Task.Yield();
        });
    }
    
    public override string PersistenceId { get; }
}
""", 
(12, 22, 15, 10)),
            (
    // ReceiveActor, receive delegate variant 2
"""
// 02
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command<int>(_ => true, async msg =>
        {
            await Task.Yield();
        });
    }
    
    public override string PersistenceId { get; }
}
""", 
"""
// 02
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        CommandAsync<int>(_ => true, async msg =>
        {
            await Task.Yield();
        });
    }
    
    public override string PersistenceId { get; }
}
""", 
(12, 33, 15, 10)),
            (
    // ReceiveActor, receive delegate variant 3
"""
// 03
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command(typeof(int), async msg =>
        {
            await Task.Yield();
        });
    }
    
    public override string PersistenceId { get; }
}
""", 
"""
// 03
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        CommandAsync(typeof(int), async msg =>
        {
            await Task.Yield();
        });
    }
    
    public override string PersistenceId { get; }
}
""", 
(12, 30, 15, 10)),
            (
    // ReceiveActor, receive delegate variant 4
"""
// 04
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command(typeof(int), _ => true, async msg =>
        {
            await Task.Yield();
        });
    }
    
    public override string PersistenceId { get; }
}
""", 
"""
// 04
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        CommandAsync(typeof(int), _ => true, async msg =>
        {
            await Task.Yield();
        });
    }
    
    public override string PersistenceId { get; }
}
""", 
(12, 41, 15, 10)),

        };
    
    [Theory]
    [MemberData(nameof(ReceiveActorFailureCases))]
    public Task RenameReceiveActorReceiveToReceiveAsync((string before, string after, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var (before, after, (startLine, startColumn, endLine, endColumn)) = d;
        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(startLine, startColumn, endLine, endColumn);

        return Verify.VerifyCodeFix(before, after, MustNotUseVoidAsyncDelegateInReceivePersistentActorCommandFixer.Key_FixCommandWithVoidAsyncDelegate,
            expectedDiagnostic);
    }

    public static readonly
        TheoryData<(string data, (int startLine, int startColumn, int endLine, int endColumn) spanData)>
        ReceiveActorIgnoredFailureCases = new()
        {
            (
    // ReceiveActor, receive delegate variant 5
"""
// 05
using Akka.Actor;
using Akka.Persistence;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command<int>(MessageHandler);
    }
    
    public override string PersistenceId { get; }

    private async void MessageHandler(int msg) { }
}
""", 
(11, 22, 11, 36)),
            (
    // ReceiveActor, receive delegate variant 6
"""
// 06
using Akka.Actor;
using Akka.Persistence;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command<int>(_ => true, MessageHandler);
    }
    
    public override string PersistenceId { get; }

    private async void MessageHandler(int msg) { }
}
""", 
(11, 33, 11, 47)),
            (
    // ReceiveActor, receive delegate variant 7
"""
// 07
using Akka.Actor;
using Akka.Persistence;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command(typeof(int), _ => true, MessageHandler);
    }
    
    public override string PersistenceId { get; }

    private async void MessageHandler(object msg) { }
}
""", 
(11, 41, 11, 55)),
            (
    // ReceivePersistentActor, command delegate variant 8
"""
// 08
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command(typeof(int), _ => true, MessageHandler);
    }
    
    public override string PersistenceId { get; }

    private async void MessageHandler(object msg) { }
}
""", 
(12, 41, 12, 55)),
            (
    // ReceivePersistentActor, command delegate variant 9
"""
// 09
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command(MessageHandler);
    }
    
    public override string PersistenceId { get; }

    private async void MessageHandler(object msg) { }
}
""", 
(12, 17, 12, 31)),

        };    
    [Theory]
    [MemberData(nameof(ReceiveActorIgnoredFailureCases))]
    public Task IgnoreComplexReceiveActorReceive((string data, (int startLine, int startColumn, int endLine, int endColumn) spanData) d)
    {
        var (data, (startLine, startColumn, endLine, endColumn)) = d;
        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(startLine, startColumn, endLine, endColumn);

        return Verify.VerifyCodeFix(data, data, MustNotUseVoidAsyncDelegateInReceivePersistentActorCommandFixer.Key_FixCommandWithVoidAsyncDelegate, 
            [expectedDiagnostic], [expectedDiagnostic]);
    }
    
}