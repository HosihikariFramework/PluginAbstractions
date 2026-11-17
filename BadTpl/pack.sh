#!/usr/bin/env bash
# Pack the plugin through Hosihikari.Plugin.Build's PackPlugin target.
#
# The manifest is generated from the project.
# Usage: bash ./pack.sh [output-root]
set -euo pipefail

here=$(cd "$(dirname "$0")" && pwd)
name='BadTpl'
root=${1:-dist}
case "$root" in
    /*) ;;
    *) root="$here/$root" ;;
esac

dotnet build "$here/$name.csproj" -c Release --nologo \
    -t:PackPlugin "-p:HosihikariPluginOutputRoot=$root"
