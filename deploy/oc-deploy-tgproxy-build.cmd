@echo off
@setlocal
set ERROR_CODE=0

pushd C:\Projects\TigerGraph.NET\src\TigerGraph.Proxy
if not %ERRORLEVEL%==0  (
    echo Could not cd to C:\Projects\TigerGraph.NET\src\TigerGraph.Proxy
    set ERROR_CODE=1
    goto End
)
dotnet publish -c Debug /p:MicrosoftNETPlatformLibrary=Microsoft.NETCore.App
if not %ERRORLEVEL%==0  (
    echo Could not build project at C:\Projects\TigerGraph.NET\src\TigerGraph.Proxy
    set ERROR_CODE=1
    popd
    goto End
)
oc start-build tgproxy --from-dir=bin\Debug\netcoreapp3.1\publish
if not %ERRORLEVEL%==0  (
    echo Could not start build on OpenShift.
    set ERROR_CODE=1
    popd
    goto End
)
echo Deploy succeded.
popd

:End
@endlocal
exit /B %ERROR_CODE%
