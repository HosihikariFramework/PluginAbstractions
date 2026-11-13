# Hosihikari.Plugin.Model

The plugin manifest model: `PluginManifest` and its dependency types.

It lives outside the contract assembly so the plugin manager and the packer can share one model
without either of them depending on the other (ADR-0059). The contract package references it, so a
project that references `Hosihikari.Plugin.Abstractions` gets the types without an extra reference.

The model carries no behaviour beyond reading the manifest's own fields; the format itself is
described in the [contract readme](https://github.com/HosihikariFramework/PluginAbstractions#manifest).
