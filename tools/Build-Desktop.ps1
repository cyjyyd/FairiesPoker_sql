param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::UTF8
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repositoryRoot "FairiesPoker.MG/FairiesPoker.MG.csproj"

& dotnet build $project --framework net9.0 --configuration $Configuration --nologo
if ($LASTEXITCODE -ne 0) { throw "Desktop build failed." }
