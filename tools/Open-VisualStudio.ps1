$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::UTF8
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$locator = Join-Path ([Environment]::GetFolderPath("ProgramFilesX86")) "Microsoft Visual Studio/Installer/vswhere.exe"
if (-not (Test-Path -LiteralPath $locator)) { throw "Visual Studio Installer/vswhere.exe was not found." }
$ide = & $locator -latest -products "*" -requires Microsoft.Component.MSBuild -find "Common7/IDE/devenv.exe" -utf8
if (-not $ide -or -not (Test-Path -LiteralPath $ide)) { throw "No registered Visual Studio installation was found." }
# Explicitly opens an interactive IDE, avoiding stale file associations left by an old installation.
Start-Process -FilePath $ide -ArgumentList ('"' + (Join-Path $repositoryRoot "FairiesPoker.sln") + '"')
