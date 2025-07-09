// -----------------------------------------------------------------------
//  <copyright file="MustNotHandleISystemMessageInsideTellInternalAnalyzerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis.Testing;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotHandleISystemMessageInsideTellInternalAnalyzer>;

namespace Akka.Analyzers.Tests.Analyzers.AK2000;

public class MustNotHandleISystemMessageInsideTellInternalAnalyzerSpecs
{
    public static readonly TheoryData<string> SuccessCases = new()
    {
        // No usage of ISystemMessage in TellInternal
"""
using Akka.Actor;
public class MyRef : ActorRefBase 
{
    public override ActorPath Path { get ; } = new RootActorPath(new Address("akka.tcp", "system", "127.0.0.1", 1337), "user");
    
    protected override void TellInternal(object message, IActorRef sender) {
        // safe
        var s = message.ToString();
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

    public static readonly TheoryData<(string testData, (int startLine, int startColumn, int endLine, int endColumn)[] spanData)> FailureCases = new()
    {
        // Explicit cast
        (
"""
// 01
using Akka.Actor;
using Akka.Dispatch.SysMsg;
public class MyRef : ActorRefBase 
{
    public override ActorPath Path { get ; } = new RootActorPath(new Address("akka.tcp", "system", "127.0.0.1", 1337), "user");

    protected override void TellInternal(object message, IActorRef sender) {
        var sysMsg = (ISystemMessage)message;
    }
}
""", [(9, 22, 9, 45)]),
        // is pattern
        (
"""
// 02
using Akka.Actor;
using Akka.Dispatch.SysMsg;
public class MyRef : ActorRefBase 
{
    public override ActorPath Path { get ; } = new RootActorPath(new Address("akka.tcp", "system", "127.0.0.1", 1337), "user");
    
    protected override void TellInternal(object message, IActorRef sender) {
        if (message is ISystemMessage sysMsg) { }
    }
}
""", [(9, 24, 9, 45)]),
        // as pattern
        (
"""
// 03
using Akka.Actor;
using Akka.Dispatch.SysMsg;
public class MyRef : ActorRefBase 
{
    public override ActorPath Path { get ; } = new RootActorPath(new Address("akka.tcp", "system", "127.0.0.1", 1337), "user");
    
    protected override void TellInternal(object message, IActorRef sender) {
        var sysMsg = message as ISystemMessage;
        if (sysMsg != null) { }
    }
}
""", [(9, 22, 9, 47)]),
        // switch/case
        (
"""
// 04
using Akka.Actor;
using Akka.Dispatch.SysMsg;
public class MyRef : ActorRefBase 
{
    public override ActorPath Path { get ; } = new RootActorPath(new Address("akka.tcp", "system", "127.0.0.1", 1337), "user");
    
    protected override void TellInternal(object message, IActorRef sender) {
        switch (message) {
            case ISystemMessage sysMsg:
                break;
        }
    }
}
""", [(10, 18, 10, 39)]),
        // Derived type (SystemMessage)
        (
"""
// 05
using Akka.Actor;
using Akka.Dispatch.SysMsg;
public class MyRef : ActorRefBase 
{
    public override ActorPath Path { get ; } = new RootActorPath(new Address("akka.tcp", "system", "127.0.0.1", 1337), "user");
    
    protected override void TellInternal(object message, IActorRef sender) {
        var sysMsg = (SystemMessage)message;
    }
}
""", [(9, 22, 9, 44)]),
        // Derived type (Escalate)
        (
"""
// 06
using Akka.Actor;
using Akka.Dispatch.SysMsg;
public class MyRef : ActorRefBase 
{
    public override ActorPath Path { get ; } = new RootActorPath(new Address("akka.tcp", "system", "127.0.0.1", 1337), "user");
    
    protected override void TellInternal(object message, IActorRef sender) {
        if (message is Escalate esc) { }
    }
}
""", [(9, 24, 9, 36)]),
        // Twice inherited
        (
"""
// 07
using Akka.Actor;
using Akka.Dispatch.SysMsg;
public class MyRefBase : ActorRefBase 
{
    public override ActorPath Path { get ; } = new RootActorPath(new Address("akka.tcp", "system", "127.0.0.1", 1337), "user");
    
    protected override void TellInternal(object message, IActorRef sender) {
    }
}
public class MyRef : MyRefBase 
{
    protected override void TellInternal(object message, IActorRef sender) {
        if (message is Escalate esc) { }
    }
}
""", [(14, 24, 14, 36)]),
        // Multiple offense, should list all
        (
"""
// 08
using Akka.Actor;
using Akka.Dispatch.SysMsg;
public class MyRef : ActorRefBase 
{
    public override ActorPath Path { get ; } = new RootActorPath(new Address("akka.tcp", "system", "127.0.0.1", 1337), "user");
    
    protected override void TellInternal(object message, IActorRef sender) {
        switch (message) {
            case Stop:
                break;
            case Escalate:
                break;
            case ISystemMessage:
                break;
        }
    }
}
""", [(10, 18, 10, 22), (12, 18, 12, 26), (14, 18, 14, 32)]),
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
        
        await Verify.VerifyAnalyzer(testData, expectedDiagnostics);
    }
} 