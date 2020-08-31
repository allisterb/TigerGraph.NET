@echo off
REM oc delete bc tgproxy
REM oc delete dc tgproxy
REM oc delete svc tgproxy

REM cd C:\Projects\TigerGraph.NET\src\TigerGraph.Proxy

REM dotnet publish -c Debug /p:MicrosoftNETPlatformLibrary=Microsoft.NETCore.App
REM oc new-build --name=tgproxy dotnet:3.1 --binary=true
REM oc start-build tgproxy --from-dir=bin\Debug\netcoreapp3.1\publish
REM oc new-app tgproxy -e TG_TOKEN=mytoken -e TG_SERVER_URL=myserverurl 
REM oc expose svc/tgproxy