#!/bin/bash

set -e 

cd src/TigerGraph.CLI
dotnet build /p:Configuration=RuntimeDebug $*
cd ../../
