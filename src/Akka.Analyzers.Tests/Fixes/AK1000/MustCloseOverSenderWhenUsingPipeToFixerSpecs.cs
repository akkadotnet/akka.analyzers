// -----------------------------------------------------------------------
//  <copyright file="MustCloseOverSenderWhenUsingPipeToFixerSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2024 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------

using Akka.Analyzers.Fixes;
using Microsoft.CodeAnalysis;
using Verify = Akka.Analyzers.Tests.Utility.AkkaVerifier<Akka.Analyzers.MustCloseOverSenderWhenUsingPipeToAnalyzer>;

namespace Akka.Analyzers.Tests.Fixes.AK1000;

public class MustCloseOverSenderWhenUsingPipeToFixerSpecs
{
    [Fact]
    public Task AddClosureInsideReceiveActor()
    {
        var before =
            @"using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor{

    public MyActor(){
        Receive<string>(str => {
            async Task<int> LocalFunction(){
                await Task.Delay(10);
                return str.Length;
            }

            // incorrect use of closure
            LocalFunction().PipeTo(Sender); 
        });
    }
}";

        var after =
            @"using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor{

    public MyActor(){
        Receive<string>(str => {
            async Task<int> LocalFunction(){
                await Task.Delay(10);
                return str.Length;
            }
            var sender = this.Sender;

            // incorrect use of closure
            LocalFunction().PipeTo(sender); 
        });
    }
}";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(14, 29, 14, 35)
            .WithArguments("Sender");

        return Verify.VerifyCodeFix(before, after, MustCloseOverSenderWhenUsingPipeToFixer.Key_FixPipeToSender,
            expectedDiagnostic);
    }

    [Fact]
    public Task AddClosureInsideOneLinerReceiveMethod()
    {
        var before =
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str => MessageHandler(str).PipeTo(Sender));
    }

    private Task<int> MessageHandler(string str)
    {
        return Task.FromResult(str.Length);
    }
}
""";

        var after =
"""
using Akka.Actor;
using System.Threading.Tasks;

public sealed class MyActor : ReceiveActor
{
    public MyActor()
    {
        Receive<string>(str =>
        {
            var sender = this.Sender;
            MessageHandler(str).PipeTo(sender);
        });
    }

    private Task<int> MessageHandler(string str)
    {
        return Task.FromResult(str.Length);
    }
}
""";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(8, 52, 8, 58)
            .WithArguments("Sender");

        return Verify.VerifyCodeFix(before, after, MustCloseOverSenderWhenUsingPipeToFixer.Key_FixPipeToSender,
            expectedDiagnostic);
    }

    [Fact]
    public Task AddClosureInsideUntypedActor()
    {
        var before =
            @"using Akka.Actor;
using System.Threading.Tasks;
using System;

public sealed class MyActor : UntypedActor{

    protected override void OnReceive(object message){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return message.ToString().Length;
        }

        Console.WriteLine(Sender);
        // incorrect use of closure
        LocalFunction().PipeTo(Sender); 
    }
}";

        var after =
            @"using Akka.Actor;
using System.Threading.Tasks;
using System;

public sealed class MyActor : UntypedActor{

    protected override void OnReceive(object message){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return message.ToString().Length;
        }

        Console.WriteLine(Sender);
        var sender = this.Sender;
        // incorrect use of closure
        LocalFunction().PipeTo(sender); 
    }
}";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(15, 25, 15, 31)
            .WithArguments("Sender");

        return Verify.VerifyCodeFix(before, after, MustCloseOverSenderWhenUsingPipeToFixer.Key_FixPipeToSender,
            expectedDiagnostic);
    }

    [Fact]
    public Task AddClosureWithoutErasingOtherPipeToArguments()
    {
        var before =
            @"using Akka.Actor;
using System.Threading.Tasks;
using System;

public sealed class MyActor : UntypedActor{

    protected override void OnReceive(object message){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return message.ToString().Length;
        }

        Console.WriteLine(Sender);
        // incorrect use of closure
        LocalFunction().PipeTo(Sender, success: r => 1); 
    }
}";

        var after =
            @"using Akka.Actor;
using System.Threading.Tasks;
using System;

public sealed class MyActor : UntypedActor{

    protected override void OnReceive(object message){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return message.ToString().Length;
        }

        Console.WriteLine(Sender);
        var sender = this.Sender;
        // incorrect use of closure
        LocalFunction().PipeTo(sender, success: r => 1); 
    }
}";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(15, 25, 15, 31)
            .WithArguments("Sender");

        return Verify.VerifyCodeFix(before, after, MustCloseOverSenderWhenUsingPipeToFixer.Key_FixPipeToSender,
            expectedDiagnostic);
    }

    [Fact]
    public Task MustOnlyReplaceSenderArgumentWhenUsed()
    {
        var before =
            @"using Akka.Actor;
using System.Threading.Tasks;
using System;

public sealed class MyActor : UntypedActor{

    protected override void OnReceive(object message){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return message.ToString().Length;
        }

        Console.WriteLine(Sender);
        // incorrect use of closure
        LocalFunction().PipeTo(Self, Sender); 
    }
}";

        var after =
            @"using Akka.Actor;
using System.Threading.Tasks;
using System;

public sealed class MyActor : UntypedActor{

    protected override void OnReceive(object message){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return message.ToString().Length;
        }

        Console.WriteLine(Sender);
        var sender = this.Sender;
        // incorrect use of closure
        LocalFunction().PipeTo(Self, sender); 
    }
}";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(15, 25, 15, 31)
            .WithArguments("Sender");

        return Verify.VerifyCodeFix(before, after, MustCloseOverSenderWhenUsingPipeToFixer.Key_FixPipeToSender,
            expectedDiagnostic);
    }

    [Fact]
    public Task ReplaceSenderWhenUsedForBothRecipientAndSender()
    {
        var before =
            @"using Akka.Actor;
using System.Threading.Tasks;
using System;

public sealed class MyActor : UntypedActor{

    protected override void OnReceive(object message){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return message.ToString().Length;
        }

        Console.WriteLine(Sender);
        // incorrect use of closure
        LocalFunction().PipeTo(this.Sender, Sender); 
    }
}";

        var after =
            @"using Akka.Actor;
using System.Threading.Tasks;
using System;

public sealed class MyActor : UntypedActor{

    protected override void OnReceive(object message){
        async Task<int> LocalFunction(){
            await Task.Delay(10);
            return message.ToString().Length;
        }

        Console.WriteLine(Sender);
        var sender = this.Sender;
        // incorrect use of closure
        LocalFunction().PipeTo(sender, sender); 
    }
}";

        var expectedDiagnostic = Verify.Diagnostic()
            .WithSpan(15, 25, 15, 31)
            .WithArguments("Sender");

        return Verify.VerifyCodeFix(before, after, MustCloseOverSenderWhenUsingPipeToFixer.Key_FixPipeToSender,
            expectedDiagnostic);
    }
    
    [Fact(DisplayName = "Should fix missing closure when using Context.Sender instead of this.Sender")]
    public Task FailureCaseWithContextSender()
    {
        var before = """
                   using Akka.Actor;
                   using System.Threading.Tasks;

                   public sealed class MyActor : UntypedActor{
                   
                       protected override void OnReceive(object message){
                           async Task<int> LocalFunction(){
                               await Task.Delay(10);
                               return message.ToString().Length;
                           }
                   
                           // incorrect use of closures
                           LocalFunction().PipeTo(Context.Sender);
                       }
                   }
                   """;
        
        var after = """
                     using Akka.Actor;
                     using System.Threading.Tasks;

                     public sealed class MyActor : UntypedActor{
                     
                         protected override void OnReceive(object message){
                             async Task<int> LocalFunction(){
                                 await Task.Delay(10);
                                 return message.ToString().Length;
                             }
                             var sender = this.Sender;
                     
                             // incorrect use of closures
                             LocalFunction().PipeTo(sender);
                         }
                     }
                     """;
        
        var expected = Verify.Diagnostic()
            .WithSpan(13, 25, 13, 31)
            .WithArguments("Context.Sender")
            .WithSeverity(DiagnosticSeverity.Error);
        
        return Verify.VerifyCodeFix(before, after, MustCloseOverSenderWhenUsingPipeToFixer.Key_FixPipeToSender,
            expected);
    }
}