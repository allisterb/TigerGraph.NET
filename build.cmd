@echo off
@setlocal
set ERROR_CODE=0

echo Building CLI...
cd src\TigerGraph.CLI\
dotnet build /p:Configuration=Debug %*

echo Building proxy...
cd ..\TigerGraph.CLI\
dotnet build /p:Configuration=Debug %*

:end
cd ..\..
exit /B %ERROR_CODE%