@echo off
oc delete bc tgproxy
oc delete dc tgproxy
oc delete svc tgproxy

cd C:\Projects\TigerGraph.NET\src\TigerGraph.Proxy

dotnet publish -c Debug /p:MicrosoftNETPlatformLibrary=Microsoft.NETCore.App
oc new-build --name=tgproxy dotnet:3.1 --binary=true
oc start-build tgproxy --from-dir=bin\Debug\netcoreapp3.1\publish
oc new-app tgproxy -e TG_TOKEN=%TG_TOKEN% -e TG_REST_SERVER_URL=%TG_REST_SERVER_URL% -e TG_GSQL_SERVER_URL=%TG_GSQL_SERVER_URL% -e TG_USER=%TG_USER% -e TG_PASS=%TG_PASS%
REM oc expose svc/tgproxy