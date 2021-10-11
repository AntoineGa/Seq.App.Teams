# This script originally (c) 2016 Serilog Contributors - license Apache 2.0

echo "build: Build started"

Push-Location $PSScriptRoot

if(Test-Path .\artifacts) {
	echo "build: Cleaning .\artifacts"
	Remove-Item .\artifacts -Force -Recurse
}

& dotnet restore --no-cache
if($LASTEXITCODE -ne 0) { exit 1 }    

$version = @{ $true = $env:APPVEYOR_BUILD_VERSION; $false = "0.0.0" }[$env:APPVEYOR_BUILD_VERSION -ne $NULL];

echo "build: Version is $version"

Push-Location Seq.App.Teams

& dotnet publish -c Release -o ./obj/publish /p:Version=$version
if($LASTEXITCODE -ne 0) { exit 2 }

& dotnet pack -c Release -o ..\Artifacts --no-build /p:Version=$version
if($LASTEXITCODE -ne 0) { exit 3 }

Pop-Location
Push-Location Seq.App.Teams.Tests

echo "build: Testing"

& dotnet test -c Release
if($LASTEXITCODE -ne 0) { exit 4 }

Pop-Location
Pop-Location
