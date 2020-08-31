# About
This is a small .NET Core proxy server for dealing with some of the limitations of writing client-side apps that talk to a free-tier TigerGraph server instance:
* Use environment variables defined on the server for the TigerGraph access token and url so you don't have to expose these in your client code.
* Send back to the client the correct CORS headers like `Access-Control-Allow-Origin` together with the TigerGraph JSON data so you can make cross-origin calls to the server from your browser app.
* Ping the TigerGraph server every 15mins since free-tier servers shutdown after a period of inactivity and there's no way to auto-restart them.

# Pre-requisites
You'll need the .NET Core 3.1 SDK [from](https://dotnet.microsoft.com/download) Microsoft.

# Usage

* Clone the TigerGraph.NET repo and run `dotnet build` in the TigerGraph.Proxy sub-directory. 
* Set the `TG_TOKEN` and `TG_SERVER_URL` variables.
* From the proxy sub-directory run `dotnet run bin\Debug\netcoreapp3.1\TigerGraph.Proxy.exe --urls=http://localhost:5001/`.
Change the port number if required. Press Ctrl-C to stop the server.
* You can then run something like `curl -i --location --request GET "http://localhost:5001/p/echo"` and the server with return the JSON data from the TG server at `TG_SERVER_URL_` together with CORS headers. It will also make `echo` calls to the TG server every 15mins.
