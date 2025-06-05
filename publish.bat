@echo off
setlocal enabledelayedexpansion

if "%~1"=="" (
    echo ERROR: Please specify the version number as the first parameter.
    echo "Usage: publish.bat <version> <nuget-api-key>"
    goto :eof
)

if "%~2"=="" (
    echo ERROR: Please specify the NuGet API key as the second parameter.
    echo "Usage: publish.bat <version> <nuget-api-key>"
    goto :eof
)

set VERSION=%1
set NUGET_API_KEY=%2
set CONFIGURATION=Release

echo Cleaning up...
for /r src %%f in (*.nupkg) do (
    del /q "%%f"
)

echo Restoring packages...
dotnet restore
if errorlevel 1 (
    echo Restore failed.
    goto :eof
)

echo Building solution with version %VERSION%...
dotnet build Doulex.DomainDriven.sln -c %CONFIGURATION%
if errorlevel 1 (
    echo Build failed.
    goto :eof
)

echo Packing projects...
dotnet pack .\src\Doulex.DomainDriven -c %CONFIGURATION% -p:VersionPrefix=%VERSION%
if errorlevel 1 (
    echo Pack Doulex.DomainDriven failed.
    goto :eof
)

dotnet pack .\src\Doulex.DomainDriven.Repo.EFCore -c %CONFIGURATION% -p:VersionPrefix=%VERSION%
if errorlevel 1 (
    echo Pack Doulex.DomainDriven.Repo.EFCore failed.
    goto :eof
)

dotnet pack .\src\Doulex.DomainDriven.Repo.FileSystem -c %CONFIGURATION% -p:VersionPrefix=%VERSION%
if errorlevel 1 (
    echo Pack Doulex.DomainDriven.Repo.FileSystem failed.
    goto :eof
)

echo Pushing packages to NuGet...
dotnet nuget push src\**\*.nupkg -k %NUGET_API_KEY% -s https://api.nuget.org/v3/index.json --skip-duplicate
if errorlevel 1 (
    echo Push failed.
    goto :eof
)

echo All done.

endlocal
