#### 0.2.0 January 8th 2024 ####

* [Added Uris for all error messages flagged by Akka.Analyzers](https://github.com/akkadotnet/akka.analyzers/issues/6)
* [Implemented `AK2001`: detect when automatically handled messages are being handled inside `MessageExtractor` / `IMessageExtractor` (Cluster.Sharding)](https://github.com/akkadotnet/akka.analyzers/issues/42)

#### 0.1.2 January 3rd 2024 ####

* [Resolved issues with `AK1001` Code Fix overwriting other `PipeTo` arguments](https://github.com/akkadotnet/akka.analyzers/issues/32)
* Updated `AK1001` to also check if `Sender` is being used as the `PipeTo` `IActorRef sender` argument, which is now also handled by both the analyzer and the Code Fix.
* Corrected casing on all issue numbers.

#### 0.1.1 January 2nd 2024 ####

Fixed Roslyn NuGet package format for analyzers and code fixes per https://learn.microsoft.com/en-us/nuget/guides/analyzers-conventions

#### 0.1.0 January 2nd 2024 ####

Added first set of Roslyn Analyzers and fixes for Akka.NET:

* [AK1000: must not `new` up actor types outside of `Props.Create`](https://github.com/Aaronontheweb/akka.analyzers/pull/3)
* [AK1001: must close over `Sender` when using `PipeTo`](https://github.com/Aaronontheweb/akka.analyzers/pull/5)
* [AK2000: `Ask` should never have `TimeSpan.Zero`](https://github.com/Aaronontheweb/akka.analyzers/issues/11)

This is a pre-release package - we will work on incorporating this directly into the core [Akka.NET NuGet packages](https://www.nuget.org/packages/Akka).