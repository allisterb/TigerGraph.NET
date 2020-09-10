# About
This is a small .NET Core proxy server that provides common services for writing client-side apps that talk to a TigerGraph server instance:
* Authentication: You can set environment variables for your TG_TOKEN, TG_USER and TG_PASS credentials on the server so you don't have to expose these in your client app code.
* CORS: The server supports CORS headers and CORS pre-flighting requests so you can make calls to your TigerGraph server API from your JS browser code. Normally you would have to configure the TigerGraph Nginx server using gadmin to enable this support but this isn't available for free-tier instances
* Keep-alive: By default the proxy server pings the echo endpoint of the backing TG server every 15 minutes. By default free-tier instances shutdown after about 90 minutes of inactivity and there is no way of restarting them automatically.
* Caching: The proxy server implements a simple memory-cache which caches graph data requests using the URL requests as cache keys. Apps that use graph data can avoid hitting the TG server on every request. More sophisticated caches and schemes can be implemented pretty easily using the ASP.NET Core libraries and middleware.
# Pre-requisites
You'll need the .NET Core 3.1 SDK [from](https://dotnet.microsoft.com/download) Microsoft.

# Usage

* Clone the TigerGraph.NET repo and run `build` in the root project folder. 
* Set the `TG_TOKEN`, `TG_REST_SERVER_URL`, `TG_GSQL_SERVER_URL`, `TG_USER` and `TG_PASS` environment variables.
* Run `tg-proxy http://localhost:5001/` to start the server on port 5001. Press Ctrl-C to stop the server.

You can then run something like `curl -i --location --request GET "http://localhost:5001/p/echo"` and the server with return the JSON data from the TG server at `TG_REST_SERVER_URL`. It will also make `echo` calls to the TG server every 15mins.
