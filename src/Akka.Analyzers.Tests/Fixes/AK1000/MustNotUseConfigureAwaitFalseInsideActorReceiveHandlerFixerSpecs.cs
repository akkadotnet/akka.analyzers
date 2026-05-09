// -----------------------------------------------------------------------
//  <copyright file="MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2026 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerAnalyzer>;

namespace Akka.Analyzers.Tests.Fixes.AK1000;

public class MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerFixerSpecs
{
    [Fact]
    public Task RemoveConfigureAwaitFalse_InsideReceiveAsync()
    {
        const string before =
            """
            using System.Net.Http;
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                private readonly HttpClient _client = new();

                public MyActor()
                {
                    ReceiveAsync<string>(async url =>
                    {
                        var result = await _client.GetAsync(url).ConfigureAwait(false);
                        Sender.Tell(result);
                    });
                }
            }
            """;

        const string after =
            """
            using System.Net.Http;
            using Akka.Actor;
            using System.Threading.Tasks;

            public sealed class MyActor : ReceiveActor
            {
                private readonly HttpClient _client = new();

                public MyActor()
                {
                    ReceiveAsync<string>(async url =>
                    {
                        var result = await _client.GetAsync(url);
                        Sender.Tell(result);
                    });
                }
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(13, 32, 13, 75)
            .WithArguments();

        return Verify.VerifyCodeFix(before, after,
            MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerFixer.Key_RemoveConfigureAwaitFalse,
            expectedDiagnostic);
    }

    [Fact]
    public Task RemoveConfigureAwaitFalse_InsideCommandAsync()
    {
        const string before =
            """
            using System.Net.Http;
            using Akka.Persistence;
            using System.Threading.Tasks;

            public sealed class MyPersistentActor : ReceivePersistentActor
            {
                private readonly HttpClient _client = new();

                public MyPersistentActor()
                {
                    CommandAsync<string>(async url =>
                    {
                        var result = await _client.GetAsync(url).ConfigureAwait(false);
                    });
                }

                public override string PersistenceId => "p1";
            }
            """;

        const string after =
            """
            using System.Net.Http;
            using Akka.Persistence;
            using System.Threading.Tasks;

            public sealed class MyPersistentActor : ReceivePersistentActor
            {
                private readonly HttpClient _client = new();

                public MyPersistentActor()
                {
                    CommandAsync<string>(async url =>
                    {
                        var result = await _client.GetAsync(url);
                    });
                }

                public override string PersistenceId => "p1";
            }
            """;

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(13, 32, 13, 75)
            .WithArguments();

        return Verify.VerifyCodeFix(before, after,
            MustNotUseConfigureAwaitFalseInsideActorReceiveHandlerFixer.Key_RemoveConfigureAwaitFalse,
            expectedDiagnostic);
    }
}
