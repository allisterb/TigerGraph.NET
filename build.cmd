@echo off
@setlocal
set ERROR_CODE=0

cd src\TigerGraph.CLI\
dotnet build /p:Configuration=Debug %*

:end
cd ..\..
exit /B %ERROR_CODE%