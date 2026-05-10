// -----------------------------------------------------------------------
//  <copyright file="ActorHandlerMethodCacheSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Akka.Analyzers.Tests.Utility;

public class ActorHandlerMethodCacheSpec
{
    private static async Task<(ActorHandlerMethodCache Cache, SyntaxTree Tree, SemanticModel Model)> BuildAsync(string source)
    {
        var (compilation, tree, model, akkaContext) = await ResolverTestHarness.CompileAsync(source);
        return (ActorHandlerMethodCache.Create(compilation, akkaContext), tree, model);
    }

    private static IMethodSymbol FindMethod(SyntaxTree tree, SemanticModel model, string name)
    {
        var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.Text == name);
        return (IMethodSymbol)model.GetDeclaredSymbol(node)!;
    }

    [Fact]
    public async Task Contains_MethodGroupBoundToReceiveAsync()
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

        var (cache, tree, model) = await BuildAsync(src);
        var handle = FindMethod(tree, model, "Handle");

        Assert.True(cache.Contains(handle));
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public async Task DoesNotContain_LambdaBindings()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(async m => { await Task.Yield(); }); }
            }
            """;

        var (cache, _, _) = await BuildAsync(src);
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public async Task DoesNotContain_NonHandlerMethods()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(Handle); }
                private async Task Handle(string m) { Helper(); await Task.Yield(); }
                private void Helper() { }
            }
            """;

        var (cache, tree, model) = await BuildAsync(src);
        var helper = FindMethod(tree, model, "Helper");

        Assert.False(cache.Contains(helper));
    }

    [Fact]
    public async Task Contains_AcrossMultipleHandlerKinds()
    {
        var src = """
            using Akka.Actor;
            using Akka.Persistence;
            using System.Threading.Tasks;
            public class P : ReceivePersistentActor
            {
                public P()
                {
                    CommandAsync<string>(Cmd);
                    Recover<int>(Replay);
                }
                private async Task Cmd(string m) { await Task.Yield(); }
                private void Replay(int e) { }
                public override string PersistenceId => "p";
            }
            public class A : UntypedActor
            {
                protected override void OnReceive(object msg) { RunTask(DoWork); }
                private void DoWork() { }
            }
            """;

        var (cache, tree, model) = await BuildAsync(src);
        var cmd = FindMethod(tree, model, "Cmd");
        var replay = FindMethod(tree, model, "Replay");
        var doWork = FindMethod(tree, model, "DoWork");

        Assert.True(cache.Contains(cmd));
        Assert.True(cache.Contains(replay));
        Assert.True(cache.Contains(doWork));
        Assert.Equal(3, cache.Count);
    }

    [Fact]
    public async Task NormalizesGenericMethodToOriginalDefinition()
    {
        var src = """
            using Akka.Actor;
            using System.Threading.Tasks;
            public class A : ReceiveActor
            {
                public A() { ReceiveAsync<string>(Generic<string>); }
                private async Task Generic<T>(T m) { await Task.Yield(); }
            }
            """;

        var (cache, tree, model) = await BuildAsync(src);
        var generic = FindMethod(tree, model, "Generic");

        // GetDeclaredSymbol returns the open generic; the binding goes through OriginalDefinition.
        Assert.True(cache.Contains(generic));
    }

    [Fact]
    public async Task EmptyCompilation_ReturnsEmptyCache()
    {
        var src = """
            using Akka.Actor;
            public class NotAnActor { }
            """;

        var (cache, _, _) = await BuildAsync(src);
        Assert.Equal(0, cache.Count);
    }
}
