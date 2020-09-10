## Inspiration
.NET is one of the most popular enterprise developer technologies and as graph databases become more mainstream, vendors like  [Neo4j](https://neo4j.com/developer/dotnet/) and [DGraph](https://github.com/dgraph-io/dgraph.net) already ship .NET native libraries for their products which allow .NET developers to program graph databases in their own language and connect their existing apps and data sources without relying on manual HTTP calls to an API.

At the same time .NET, C# and F# have unique strengths as a platform and languages for developers, which make .NET an appealing choice for building graph-powered apps that can run both as traditional client-server apps and also as HTML-only SPA apps, together with interactive data-analysis notebooks in Jupyter [using](https://github.com/dotnet/interactive) C# and F#. Frameworks like [Blazor](https://devblogs.microsoft.com/aspnet/blazor-webassembly-3-2-0-now-available/) which can compile .NET code to WebAssembly can simplify development of multi-target applications by allowing code to be reused across a solution 

Right now the first-choice client library for TigerGraph is [pyTigerGraph](https://github.com/pyTigerGraph/pyTigerGraph) which is widelt used across the TigerGraph ecosystem.
However I wanted to build a .NET native client library for TigerGraph using the following requirements:

* Cross-platform
* No external dependencies
* The same library can be used in CLI and server apps and also target JavaScript and WebAssembly
* Connect to Windows data sources like the Windows Event Log

In addition I wanted to build components that implement common design patterns and practices to solve challenges developers commonly face using the TigerGraph server. My own interest in graph databases is for security and endpoint protection and I want to build open-source apps that can ingest data from a wide set of sources on Windows and Linux and be deployed quickly and easily.

## What it does
TigerGraph.NET is a set of libraries, tools and components for building multi-target graph-powered applications using C# and F#. There are several sub-projects under the TG.NET umbrella:

### CLI
The [CLI](https://github.com/allisterb/TigerGraph.NET/tree/master/src/TigerGraph.Proxy) project provides a cross-platform client for querying and monitoring TigerGraph servers including free-tier server instances. It talks to the REST++ and GSQL endpoints does not rely on the Java based GSQL client.  

### Proxy
The [Proxy](https://github.com/allisterb/TigerGraph.NET/tree/master/src/TigerGraph.Proxy) project is a proxy server for TigerGraph that provides common app services like caching and mitigates some of the limitations of using free-tier TigerGraph instances for browser-based apps. The proxy is a small .NET Core app that can run on most Linux environments like on a AWS micro-instance or as a container on Redhat OpenShift and provides a transparent proxy for REST++ and GSQL API requests from client-side code with the following features.

* Authentication: You can set environment variables for your  `TG_TOKEN`, `TG_USER` and `TG_PASS` credentials on the server so you don't have to expose these in your client app code.

* CORS: The server supports CORS headers and CORS pre-flighting requests so you can make calls to your TigerGraph server API from your JS browser code. Normally you would have to configure the TigerGraph Nginx server using `gadmin` to enable this support but this isn't available for free-tier instances

* Keep-alive: By default the proxy server pings the `echo` endpoint of the backing TG server every 15 minutes. By default free-tier instances shutdown after about 90 minutes of inactivity and there is no way of restarting them automatically.

* Cachiing: The proxy server implements a simple memory-cache which caches graph data requests using the URL requests as cache keys. Apps that use graph data can avoid hitting the TG server on every request. More sophisticated caches and schemes can be implemented pretty easily using the ASP.NET Core libraries and middleware.

### Deployment
Deployment is a common task of developers of web application, but can be a complex fragilr process fraught with errors. One wa
TigerGraph.NET comes with a set of deploy scripts for deploying

## How I built it

## Challenges I ran into

## Accomplishments that I'm proud of

## What I learned

## What's next for TigerGraph.NET: Libraries and building blocks for graph apps

https://deck.net/e219ed9b62eda88647f0eac92be4bc9b