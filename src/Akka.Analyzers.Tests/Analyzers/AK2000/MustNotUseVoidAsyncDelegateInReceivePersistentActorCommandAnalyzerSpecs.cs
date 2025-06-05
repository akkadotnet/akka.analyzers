// -----------------------------------------------------------------------
//  <copyright file="MustNotUseVoidAsyncDelegateInReceivePersistentActorCommandAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis.Testing;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseVoidAsyncDelegateInReceivePersistentActorCommandAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK2000;

public class MustNotUseVoidAsyncDelegateInReceivePersistentActorCommandAnalyzerSpecs
{
        public static readonly TheoryData<string> SuccessCases = new()
	{
        // Normal ReceiveActor usage
"""
using Akka.Actor;
using Akka.Persistence;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command<int>(msg => Sender.Tell(msg));
        Command<int>(_ => true, msg => Sender.Tell(msg));
        Command<int>(msg => { // Impossible to pass in void delegate, no need to analyze/fix
            Sender.Tell(msg);
            return true;
        });
        Command(typeof(int), msg => Sender.Tell(msg));
        Command(typeof(int), _ => true, msg => Sender.Tell(msg));
        Command(typeof(int), msg => { // Impossible to pass in void delegate, no need to analyze/fix
            Sender.Tell(msg);
            return true;
        });
        
        Command<int>(MessageHandler);
        Command<int>(_ => true, MessageHandler);
        Command(typeof(int), MessageHandler);
        Command(typeof(int), _ => true, MessageHandler);
        
        Command<int>(MessageHandlerBool); // Impossible to pass in void delegate, no need to analyze/fix
        Command(typeof(int), MessageHandlerBool); // Impossible to pass in void delegate, no need to analyze/fix
        Command(MessageHandler);
    }
    
    public override string PersistenceId { get; }

    private void MessageHandler(object msg) { }
    private void MessageHandler(int msg) { }
    private bool MessageHandlerBool(object msg) => true;
    private bool MessageHandlerBool(int msg) => true;
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
        ReceiveActorFailureCases = new()
        {
            (
    // ReceivePersistentActor, command delegate variant 1
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
""", [(12, 22, 15, 10)]),
            (
    // ReceivePersistentActor, command delegate variant 2
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
""", [(12, 33, 15, 10)]),
            (
    // ReceivePersistentActor, command delegate variant 3
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
""", [(12, 30, 15, 10)]),
            (
    // ReceivePersistentActor, command delegate variant 4
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
""", [(12, 41, 15, 10)]),
            (
    // ReceivePersistentActor, command delegate variant 5
"""
// 05
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

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
""", [(12, 22, 12, 36)]),
            (
    // ReceivePersistentActor, command delegate variant 6
"""
// 06
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

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
""", [(12, 33, 12, 47)]),
            (
    // ReceivePersistentActor, command delegate variant 7
"""
// 07
using Akka.Actor;
using Akka.Persistence;
using System.Threading.Tasks;

public class MyActor: ReceivePersistentActor
{
    public MyActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Command(typeof(int), MessageHandler);
    }
    
    public override string PersistenceId { get; }

    private async void MessageHandler(object msg) { }
}
""", [(12, 30, 12, 44)]),
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
""", [(12, 41, 12, 55)]),
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
""", [(12, 17, 12, 31)]),
            
        };
    
    [Theory]
    [MemberData(nameof(ReceiveActorFailureCases))]
    public async Task ReceiveActorFailureCase((string testData, (int startLine, int startColumn, int endLine, int endColumn)[] spanData) d)
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