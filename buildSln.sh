#!/bin/bash
rm -f *.sln
dotnet new sln
dotnet sln add **/*.csproj