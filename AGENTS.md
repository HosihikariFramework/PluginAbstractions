# AGENTS.md

The public plugin contract for the Hosihikari Framework, plus the precompiled C# and F# `dotnet new`
templates.

## Repository map

- `src/` — `IPlugin`, contexts, manifest types, contract version, loader diagnostics.
- `templates/Hosihikari.Plugin.Templates/` — C# and F# template sources and packaging project.
- `.github/workflows/build.yml` — contract build.
- `.github/workflows/template-verify.yml` — packs the contract and templates, installs them, and
  builds generated C# and F# projects.
- `.github/workflows/publish.yml` — NuGet publishing.

## Build and verify

```bash
dotnet build src/Hosihikari.Plugin.Abstractions.csproj -c Release
dotnet pack src/Hosihikari.Plugin.Abstractions.csproj -c Release -o out
dotnet pack templates/Hosihikari.Plugin.Templates/Hosihikari.Plugin.Templates.csproj -c Release -o out
```

For template changes, install the packed template, generate both languages, and build them. The
`template-verify` workflow is the reference sequence.

## Invariants

- The contract stays API version 3 and manifest version 1. The two versions have separate
  compatibility domains; bump only the one whose contract changed, and record the decision in an
  ADR.
- Plugin projects reference the contract with `ExcludeAssets="runtime"`; the shared layer owns the
  only runtime copy.
- `IPluginLoaderDiagnostics` is an optional companion interface; keep `IPluginLoader` at three
  members.
- Template output keeps the contract DLL out of the plugin package.
- Public default branches are protected. Work on a branch, open a pull request, and squash merge.

## Context

- Design decisions and terminology are kept by maintainers outside the public repositories; ask for
  the relevant decision when changing lifecycle or public API.
- [Documentation](https://hoshi.srcz.one/) — plugin authoring guides.
