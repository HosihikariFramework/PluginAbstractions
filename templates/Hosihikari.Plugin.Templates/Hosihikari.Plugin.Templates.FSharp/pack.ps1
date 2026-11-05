<#
    Pack the plugin through Hosihikari.Plugin.Build's PackPlugin target.

    The manifest is generated from the project.
    Usage: pwsh ./pack.ps1 [-OutputRoot dist]
#>
param(
    [string]$OutputRoot = 'dist'
)

$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$name = 'Hosihikari.Plugin.Templates.FSharp'

$root = if ([IO.Path]::IsPathRooted($OutputRoot)) { $OutputRoot } else { Join-Path $here $OutputRoot }

& dotnet build (Join-Path $here "$name.fsproj") -c Release --nologo `
    -t:PackPlugin "-p:HosihikariPluginOutputRoot=$root"
if ($LASTEXITCODE -ne 0) { throw 'the plugin failed to pack' }
