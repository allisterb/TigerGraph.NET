@echo off
@setlocal

cd src\TigerGraph.Proxy
dotnet run bin\Debug\netcoreapp3.1\TigerGraph.Proxy.exe --urls=%1
cd ..\..