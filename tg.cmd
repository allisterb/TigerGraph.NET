@echo off
@setlocal
set ERROR_CODE=0

src\TigerGraph.CLI\bin\Debug\net461\TigerGraph.CLI.exe %*

:end
cd ..\..\..\..\
exit /B %ERROR_CODE%