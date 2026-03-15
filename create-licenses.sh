#!/usr/bin/env bash

export DOTNET_ROLL_FORWARD=Major

# sicherstellen, dass local tools installiert sind
dotnet tool restore

# scanne direkt das Hauptprojekt
dotnet tool run dotnet-project-licenses \
  -i src/Finance.NET.csproj \
  -o \
  --outfile THIRD_PARTY_LICENSES.txt
