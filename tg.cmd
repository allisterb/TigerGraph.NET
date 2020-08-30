@echo off
@setlocal
set ERROR_CODE=0

cd src\TigerGraph.CLI\bin\Debug\net461\
"TigerGraph.CLI.exe" %*
goto end

:end
cd ..\..\..\..\
exit /B %ERROR_CODE%