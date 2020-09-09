# About
This is a cross-platform CLI for TigerGraph written in C#. It talks directly to the REST++ and GSQL endpoints of your TigerGraph server and does not require the ``gsql_client.jar`` program.
This client also allows you to run GSQL queries remotely on free-tier TigerGraph instances. The ultimate goal is to match the capabilities of the Java-based GSQL client and also support all the publicly available REST++ and GSQL endpoints
on both free and paid-tier TigerGraph server instances. 

# Pre-requisites
You'll need the .NET Core 3.1 SDK [from](https://dotnet.microsoft.com/download) Microsoft.

# Usage

* Clone the TigerGraph.NET repo and run `build` or ``./build`` in the root project folder (or you can run `dotnet build` in the `TigerGraph.CLI` project folder). 
* Run ``tg`` or ``./tg`` from the root project folder to see what actions are available and the appropriate parameters.
* Although you can set the required parameters from the CLI when invoking ``tg`` each time, it's easier to set environment variables like `TG_TOKEN` and `TG_REST_SERVER_URL` once and these will persist for the remainder your shell session.
See `setvars.cmd` in the `scripts` folder for the variables you need to set for the `tg` program to access both REST++ and GSQL endpoints.
You can then run something like ``tg  query -s "INTERPRET QUERY (INT a) FOR GRAPH MyGraph{ PRINT a; }" -p a=99`` to run a parameterized query against the default graph.
