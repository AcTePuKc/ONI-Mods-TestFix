# Oni_mods_by_Identifier Maintenance Notes

## Standalone Workflow
- Treat this directory as a self-contained archive. Build `ContainerTooltips.csproj` (and other legacy projects here) directly without relying on the modern `src/` tree.
- Keep `lib/` empty in source control. Populate it locally with Harmony/PLib/Unity assemblies from your ONI install **only when** you need offline references; otherwise configure MSBuild to read from the live game files.
- Copy `Directory.Build.props.default` to `Directory.Build.props.user` on each workstation and update `SteamFolder`, `GameFolder`, and `ModFolder` so MSBuild resolves the ONI binaries. The `.user` file remains untracked.
- Package releases by rebuilding with the desired configuration and zipping the exported contents under `Releases/` for distribution.

## Folder Layout Expectations
- Debug builds should land under `bin/Debug/net47/` (or the active TFMs) and mirror Aze's historical structure.
- Release builds belong in `bin/Release/net47/` (or the relevant TFM) and feed the distributable archives.
- Final archives must live in `Releases/*.zip` so this repository stays aligned with Aze's published packages.
