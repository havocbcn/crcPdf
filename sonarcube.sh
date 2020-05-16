#!/bin/bash
set -e

#dotnet tool install --global dotnet-sonarscanner
#dotnet tool install --global coverlet.console
#dotnet tool install -g dotnet-reportgenerator-globaltool

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

dotnet test /p:CollectCoverage="true" /p:CoverletOutputFormat="opencover" /p:CoverletOutput="$DIR/" /p:Exclude="[xunit.*]*"

reportgenerator -reports:./coverage.opencover.xml  -targetdir:./cover/

#dotnet-sonarscanner begin /k:"SharpPDF" /d:sonar.host.url="http://192.168.1.100:32768" /d:sonar.cs.opencover.reportsPaths="coverage.opencover.xml" 
#dotnet build
#dotnet-sonarscanner end 

