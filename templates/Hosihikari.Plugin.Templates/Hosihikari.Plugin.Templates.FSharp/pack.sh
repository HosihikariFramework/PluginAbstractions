#!/usr/bin/env bash
# Pack the plugin through Hosihikari.Plugin.Build's PackPlugin target.
#
# The manifest is generated from the project.
# Usage: bash ./pack.sh [output-root]
set -euo pipefail

here=$(cd "$(dirname "$0")" && pwd)
name='Hosihikari.Plugin.Templates.FSharp'
root=${1:-dist}
case "$root" in
    /*) ;;
    *) root="$here/$root" ;;
esac

dotnet build "$here/$name.fsproj" -c Release --nologo \
    -t:PackPlugin "-p:HosihikariPluginOutputRoot=$root"
