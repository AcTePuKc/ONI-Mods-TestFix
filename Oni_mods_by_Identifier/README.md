# Oni Mods by Identifier

This directory captures legacy identifiers of Oxygen Not Included mods that were once distributed separately from the main `src/` solution. In production it is published as its own repository so legacy packages can live on without pulling in the modern source tree. Use it to map historical package IDs to the actively maintained projects and to provide context for rebuilding or verifying those archived releases.

> **Reminder:** `Oni_mods_by_Identifier` should be tracked as its own repository. The build steps below assume you do **not** have access to the primary `src/` tree, so plan your workflow around the assets bundled here.

## Contents
- `ContainerTooltips/` — Original packaging of the Container Tooltips mod.
- `ZoomSpeed/` — Original packaging of the Zoom Speed mod.
- `lib/` — Placeholder for the shared binaries that supported the archived builds. The folder now stays empty in source control;
  populate it locally from your ONI installation or the maintained repository before rebuilding.
- `ONIMods.sln` — Solution file used when the identifier-based layout was current. Retain it for reference only; new builds should target the included `ContainerTooltips.csproj`.

## Rebuild Reference
This archive is self-contained. Treat `ContainerTooltips.csproj` inside `Oni_mods_by_Identifier/ContainerTooltips` as the authoritative project file when rebuilding or validating the mod. Restore packages and build directly against this project, using the `Directory.Build.props.user` override documented below to point MSBuild at your local ONI install. The legacy `ONIMods.sln` remains only as historical context; do not rely on the `src/` solution or its build graph when working from this repository.

## Aligning with `src/ContainerTooltips`
1. **Optional sibling checkout** — If you also maintain the primary mods repository, keep it in a sibling folder so you can compare history. Only source code and translation assets from `src/ContainerTooltips` need to be synchronized here; all project metadata should remain owned by this archive.
2. **Source of truth** — Treat `src/ContainerTooltips` in the maintained repository as the authoritative codebase for gameplay logic and localization. When updating this archive, copy over the mod's C# source files (excluding the project file) and the `Translations/` assets so the archival package reflects the latest behavior.
3. **Project ownership** — Leave `ContainerTooltips.csproj` under `Oni_mods_by_Identifier/ContainerTooltips` as the definitive build definition for this archive. Do not replace it with the version from `src/ContainerTooltips`; adjust it locally if build requirements change.
4. **Namespaces** — Ensure all classes stay under the `ContainerTooltips` namespace to maintain compatibility with save data and translation keys. The bundled `Mod/STRINGS.cs` scaffold should be updated whenever the maintained repository changes those constants.
5. **Dependencies** — Restore Harmony and PLib through NuGet before attempting a build. These packages remain prerequisites for the legacy and current versions of the mod.

## Synchronizing Localization & Assets
Before distributing an archival build:
- [ ] Pull the latest `.po` files from `src/ContainerTooltips/Translations` (or another trusted mirror) and ensure `_template.pot` is included. The translations do not originate from this archive, so keep them synced with the maintained repo before packaging.
- [ ] Verify asset manifests (textures, sounds) match the maintained project and copy any updates into `Oni_mods_by_Identifier/ContainerTooltips`.
- [ ] Regenerate release packages using the included `ContainerTooltips.csproj` with the desired MSBuild configuration.
- [ ] Update version metadata (mod_info.json, changelog) to match the mainline project.

## Configuring MSBuild Paths
When cloning this identifier archive by itself, copy `Directory.Build.props.default` to `Directory.Build.props.user` and adjust the directories so MSBuild can locate your ONI install directly. The `.user` file stays local thanks to `.gitignore`.

```xml
  <PropertyGroup>
    <SteamFolder>C:\Program Files (x86)\Steam</SteamFolder>
    <GameFolder>$(SteamFolder)\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed</GameFolder>
    <ModFolder>$(UserProfile)\Documents\Klei\OxygenNotIncluded\mods\dev</ModFolder>
  </PropertyGroup>
```

Adjust the values to match your workstation, then rebuild so the shared targets resolve assemblies from your live ONI installation.

## Handling Local Libraries
- The historic packages assumed a `lib/` folder populated with Harmony, PLib, and Unity assemblies extracted from a local ONI installation. These binaries are no longer tracked in source control to avoid redistributing game files.
- You can either populate `Oni_mods_by_Identifier/lib/` with the required DLLs **or** rely on `Directory.Build.props.user` to point directly at your ONI install so MSBuild references the live binaries without copying them. Choose the workflow that best fits your environment. The bundled `ContainerTooltips.csproj` and `ZoomSpeed.csproj` now honor the `$(GameFolder)` override automatically, falling back to the optional `lib/` cache only when the live game assemblies are unavailable.
- If new dependencies are required, document the source and expected filename in this section so future maintainers know how to repopulate their local `lib/` folder (or update their `.user` overrides accordingly).

By following these steps, the identifier-based archive stays consistent with the actively maintained mods while remaining ready for historical reference.

## License
The contents of this archive are distributed under the MIT License. See [`LICENSE`](./LICENSE) for the full terms, including the notice covering Peter Han's PLib dependency.
