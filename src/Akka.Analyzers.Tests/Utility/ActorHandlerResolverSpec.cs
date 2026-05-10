// -----------------------------------------------------------------------
//  <copyright file="ActorHandlerResolverSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers;
using Microsoft.CodeAnalysis;

namespace Akka.Analyzers.Tests.Utility;

public class ActorHandlerResolverSpec
{
    private static async Task<List<ActorHandlerBinding>> ResolveAsync(string source, string methodName)
    {
        var (_, tree, model, akkaContext) = await ResolverTestHarness.CompileAsync(source);
        var inv = ResolverTestHarness.FindInvocation(tree, methodName);
        return ActorHandlerResolver.Resolve(inv, model, akkaContext).ToList();
    }

    // ------------- Family 1: ReceiveActor handlers -------------

    [Fact]
    public async Task ReceiveAsync_AsyncLambda_BlockBody()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(async msg => { await Task.Yield(); }); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.Lambda, bindings[0].Kind);
        Assert.NotNull(bindings[0].Lambda);
    }

    [Fact]
    public async Task ReceiveAsync_AsyncLambda_ExpressionBody()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(async msg => Task.Delay(1)); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.Lambda, bindings[0].Kind);
    }

    [Fact]
    public async Task ReceiveAsync_NonAsyncLambdaReturningTask()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(msg => Task.CompletedTask); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.Lambda, bindings[0].Kind);
    }

    [Fact]
    public async Task ReceiveAsync_AnonymousMethod()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(async delegate(string m) { await Task.Yield(); }); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.AnonymousMethod, bindings[0].Kind);
        Assert.NotNull(bindings[0].AnonymousMethod);
    }

    [Fact]
    public async Task ReceiveAsync_MethodGroup_InstanceMethod()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(Handle); }
                private async Task Handle(string m) { await Task.Yield(); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
        Assert.Equal("Handle", bindings[0].MethodGroup!.Name);
    }

    [Fact]
    public async Task ReceiveAsync_MethodGroup_ExplicitThis()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(this.Handle); }
                private async Task Handle(string m) { await Task.Yield(); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
        Assert.Equal("Handle", bindings[0].MethodGroup!.Name);
    }

    [Fact]
    public async Task ReceiveAsync_MethodGroup_StaticOnHelper()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class Helper { public static Task Handle(string m) => Task.CompletedTask; }
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(Helper.Handle); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
        Assert.True(bindings[0].MethodGroup!.IsStatic);
    }

    [Fact]
    public async Task ReceiveAsync_MethodGroup_OnFieldReceiver()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class Helper { public Task Handle(string m) => Task.CompletedTask; }
            public class A : ReceiveActor
            {
                private readonly Helper _helper = new();
                public A() { ReceiveAsync<string>(_helper.Handle); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
        Assert.Equal("Handle", bindings[0].MethodGroup!.Name);
    }

    [Fact]
    public async Task ReceiveAsync_MethodGroup_LocalFunction()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A()
                {
                    async Task Local(string m) { await Task.Yield(); }
                    ReceiveAsync<string>(Local);
                }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
        Assert.Equal("Local", bindings[0].MethodGroup!.Name);
    }

    [Fact]
    public async Task ReceiveAsync_Wrapped_Parens()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>((Handle)); }
                private async Task Handle(string m) { await Task.Yield(); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
    }

    [Fact]
    public async Task ReceiveAsync_Wrapped_Cast()
    {
        var src = """
            using Akka.Actor;
            using System;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>((Func<string, Task>)Handle); }
                private async Task Handle(string m) { await Task.Yield(); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
    }

    [Fact]
    public async Task ReceiveAsync_Wrapped_ExplicitDelegateCtor()
    {
        var src = """
            using Akka.Actor;
            using System;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(new Func<string, Task>(Handle)); }
                private async Task Handle(string m) { await Task.Yield(); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
    }

    [Fact]
    public async Task ReceiveAsync_PredicateAndHandler_BothBindings()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(async m => { await Task.Yield(); }, m => true); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(2, bindings.Count);
        Assert.Equal(ActorHandlerBindingKind.Lambda, bindings[0].Kind);
        Assert.Equal(ActorHandlerBindingKind.Lambda, bindings[1].Kind);
    }

    [Fact]
    public async Task Receive_Sync_MethodGroup()
    {
        var src = """
            using Akka.Actor;
            public class A : ReceiveActor
            {
                public A() { Receive<string>(Handle); }
                private void Handle(string m) { }
            }
            """;
        var bindings = await ResolveAsync(src, "Receive");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
    }

    [Fact]
    public async Task ReceiveAny_Sync_Lambda()
    {
        var src = """
            using Akka.Actor;
            public class A : ReceiveActor
            {
                public A() { ReceiveAny(o => { }); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAny");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.Lambda, bindings[0].Kind);
    }

    [Fact]
    public async Task ReceiveAnyAsync_Lambda()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAnyAsync(async o => { await Task.Yield(); }); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAnyAsync");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.Lambda, bindings[0].Kind);
    }

    // ------------- Family 1: ReceivePersistentActor handlers -------------

    [Fact]
    public async Task CommandAsync_MethodGroup()
    {
        var src = """
            using Akka.Persistence;
            using System.Threading.Tasks;
            public class P : ReceivePersistentActor
            {
                public P() { CommandAsync<string>(Handle); }
                private async Task Handle(string m) { await Task.Yield(); }
                public override string PersistenceId => "p";
            }
            """;
        var bindings = await ResolveAsync(src, "CommandAsync");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
    }

    [Fact]
    public async Task CommandAnyAsync_Lambda()
    {
        var src = """
            using Akka.Persistence;
            using System.Threading.Tasks;
            public class P : ReceivePersistentActor
            {
                public P() { CommandAnyAsync(async o => { await Task.Yield(); }); }
                public override string PersistenceId => "p";
            }
            """;
        var bindings = await ResolveAsync(src, "CommandAnyAsync");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.Lambda, bindings[0].Kind);
    }

    [Fact]
    public async Task Recover_MethodGroup()
    {
        var src = """
            using Akka.Persistence;
            public class P : ReceivePersistentActor
            {
                public P() { Recover<string>(Replay); }
                private void Replay(string m) { }
                public override string PersistenceId => "p";
            }
            """;
        var bindings = await ResolveAsync(src, "Recover");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
    }

    [Fact]
    public async Task RecoverAny_Lambda()
    {
        var src = """
            using Akka.Persistence;
            public class P : ReceivePersistentActor
            {
                public P() { RecoverAny(o => { }); }
                public override string PersistenceId => "p";
            }
            """;
        var bindings = await ResolveAsync(src, "RecoverAny");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.Lambda, bindings[0].Kind);
    }

    // ------------- Family 3: RunTask -------------

    [Fact]
    public async Task RunTask_FuncTaskLambda()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : UntypedActor
            {
                protected override void OnReceive(object msg)
                {
                    RunTask(async () => { await Task.Yield(); });
                }
            }
            """;
        var bindings = await ResolveAsync(src, "RunTask");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.Lambda, bindings[0].Kind);
    }

    [Fact]
    public async Task RunTask_ActionMethodGroup()
    {
        var src = """
            using Akka.Actor;
            public class A : UntypedActor
            {
                protected override void OnReceive(object msg) { RunTask(DoWork); }
                private void DoWork() { }
            }
            """;
        var bindings = await ResolveAsync(src, "RunTask");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.MethodGroup, bindings[0].Kind);
        Assert.Equal("DoWork", bindings[0].MethodGroup!.Name);
    }

    // ------------- Unresolvable shapes (Unknown) -------------

    [Fact]
    public async Task ReceiveAsync_DelegateField_ReturnsUnknown()
    {
        var src = """
            using Akka.Actor;
            using System;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                private Func<string, Task> _handler = m => Task.CompletedTask;
                public A() { ReceiveAsync<string>(_handler); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Single(bindings);
        Assert.Equal(ActorHandlerBindingKind.Unknown, bindings[0].Kind);
    }

    [Fact]
    public async Task ReceiveAsync_LocalDelegateVariable_ReturnsUnknown()
    {
        var src = """
            using Akka.Actor;
            using System;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A()
                {
                    Func<string, Task> h = m => Task.CompletedTask;
                    ReceiveAsync<string>(h);
                }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(ActorHandlerBindingKind.Unknown, bindings[0].Kind);
    }

    [Fact]
    public async Task ReceiveAsync_MethodCallResult_ReturnsUnknown()
    {
        var src = """
            using Akka.Actor;
            using System;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(GetHandler()); }
                private Func<string, Task> GetHandler() => m => Task.CompletedTask;
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(ActorHandlerBindingKind.Unknown, bindings[0].Kind);
    }

    [Fact]
    public async Task ReceiveAsync_Conditional_ReturnsUnknown()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(true ? A : B); }
                private async Task A(string m) { await Task.Yield(); }
                private async Task B(string m) { await Task.Yield(); }
            }
            """;
        var bindings = await ResolveAsync(src, "ReceiveAsync");
        Assert.Equal(ActorHandlerBindingKind.Unknown, bindings[0].Kind);
    }

    // ------------- Negatives: not a handler-registration call -------------

    [Fact]
    public async Task NotAHandlerRegistration_ReturnsEmpty()
    {
        var src = """
            using Akka.Actor;
            public class A : ReceiveActor
            {
                public A() { Self.Tell("hi"); }
            }
            """;
        var bindings = await ResolveAsync(src, "Tell");
        Assert.Empty(bindings);
    }
}
