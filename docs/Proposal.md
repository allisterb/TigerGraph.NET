## Inspiration
.NET is one of the most popular enterprise developer technologies and as graph databases become more mainstream, vendors like  [Neo4j](https://neo4j.com/developer/dotnet/) and [DGraph](https://github.com/dgraph-io/dgraph.net) already ship .NET native libraries for their products which allow .NET developers to program graph databases in their own language and connect their existing apps and data sources without relying on manual HTTP calls to an API.

At the same time .NET, C# and F# have unique strengths as platforms and languages for developers, which make .NET an appealing choice for building graph-powered apps that can run both as traditional client-server apps and also as HTML-only SPA apps, together with interactive data-analysis notebooks in Jupyter [using](https://github.com/dotnet/interactive) C# and F#. Language features like static typing and LINQ and language-level 
async support.
Right now the first-choice client library for TigerGraph is pyTigerGrap
However I wanted to build a .NET client library using the following requirements

* Cross-platform and runs on Windows and Linux
* Can be used both in traditional apps and also compiled to JavaScript


### Proxy
The [TigerGraph.Proxy](https://github.com/allisterb/TigerGraph.NET/tree/master/src/TigerGraph.Proxy) project is a proxy server for TigerGraph that provides common app services like caching and mitigates some of the limitations of using free-tier TigerGraph instances for browser-based apps. The proxy is a small .NET Core app that can run on most Linux environments like on a AWS micro-instance or as a container on Redhat OpenShift and provides a transparent proxy for REST++ and GSQL API requests from client-side code with the following features.

* Authentication: You can set environment variables for your  `TG_TOKEN`, `TG_USER` and `TG_PASS` credentials on the server so you don't have to expose these in your client app code.

* CORS: The server supports CORS headers and CORS pre-flighting requests so you can make calls to your TigerGraph server API from your JS browser code. Normally you would have to configure the TigerGraph Nginx server using `gadmin` to enable this support but this isn't available for free-tier instances


implements common app services like caching

//[Cayley](https://neo4j.com/developer/dotnet/) 
## What it does

## How I built it

## Challenges I ran into

## Accomplishments that I'm proud of

## What I learned

## What's next for TigerGraph.NET: Libraries and building blocks for graph apps
