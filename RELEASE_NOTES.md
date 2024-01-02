#### 0.1.1 January 2nd 2024 ####

Fixed Roslyn NuGet package format for analyzers only.

#### 0.1.0 January 2nd 2024 ####

Added first set of Roslyn Analyzers and fixes for Akka.NET:

* [AK1000: must not `new` up actor types outside of `Props.Create`](https://github.com/Aaronontheweb/akka.analyzers/pull/3)
* [AK1001: must close over `Sender` when using `PipeTo`](https://github.com/Aaronontheweb/akka.analyzers/pull/5)
* [AK2000: `Ask` should never have `TimeSpan.Zero`](https://github.com/Aaronontheweb/akka.analyzers/issues/11)

This is a pre-release package - we will work on incorporating this directly into the core [Akka.NET NuGet packages](https://www.nuget.org/packages/Akka).