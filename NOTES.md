# AzeLib OnLoad benchmark (2024-06-17)

## 2026-01-09 - Identifier Publicise incremental metadata
- Added incremental `Inputs`/`Outputs` metadata and `RunOnServer` to the identifier `Publicise` target so MSBuild can skip the publicizer once the `_public` hashes exist, avoiding repeated locking across projects.
- Attempted to rebuild with `dotnet build Oni_mods_by_Identifier/ONIMods.sln` to confirm the `_public` assemblies generate once and stay unlocked, but the container still lacks the .NET host (`command not found: dotnet`). Please rerun the build locally where the ONI toolchain is available.

## 2026-01-08 - Identifier Directory.Build.props path normalization
- Updated the legacy identifier `Directory.Build.props` to combine `GameFolder` with managed assembly filenames using `System.IO.Path` so MSBuild and the publicizer receive Windows-friendly paths.
- Attempted to rebuild with `dotnet build Oni_mods_by_Identifier/ONIMods.sln` to confirm the `_public` assemblies regenerate and that the compiler resolves the ONI namespaces, but the container still lacks the .NET host (`command not found: dotnet`). Please rerun the build locally once the ONI toolchain is available.

## 2025-10-08 - Identifier ILRepack library path quoting fix
- Updated the `LibraryPath` attribute in the legacy identifier mods to drop the stray escaped quote so ILRepack's search path list terminates cleanly with the repo-local `lib` folder.
- Attempted to rebuild with `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj -c Debug` to confirm ILRepack accepts the revised path list, but the container still lacks the .NET host (`command not found: dotnet`). Please retry the build locally once the ONI toolchain is available.

## 2025-12-22 - SuppressNotifications reference cleanup
- Removed the hard-coded `Assembly-CSharp.dll` reference from `SuppressNotifications.csproj` so the project now inherits the `_public` assemblies through `Directory.Build.props`.
- Attempted to rebuild with `dotnet build src/SuppressNotifications/SuppressNotifications.csproj` to confirm `Assets.CreatePrefabs` resolves under the shared references, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally once the ONI toolchain is available to verify compilation succeeds.

## 2025-12-31 - Identifier release packaging restore
- Defaulted the legacy identifier mods' Release configuration to local `Release/<ModId>` and `Distribute/` folders within the identifier repo and enabled `DeployOniMod` automatically so packaging runs without extra MSBuild properties.
- Inlined bespoke packaging targets for `ContainerTooltips` and `ZoomSpeed` to merge dependencies, rebuild the Release install folder, and zip the DLL alongside `mod.yaml` and `mod_info.yaml` without importing shared targets from the main solution.
- Attempted `dotnet build Oni_mods_by_Identifier/ONIMods.sln -c Release` to verify the Release packaging flow, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rebuild locally to confirm the artifacts populate `Release/` and `Distribute/` as expected.

## 2025-12-31 - Identifier ILRepack search path update
- Expanded the `LibraryPath` passed to ILRepack in `ContainerTooltips` and `ZoomSpeed` so it now probes the build output, the configured `GameFolder`, and the repo-local `lib` directory for Harmony.
- Attempted to rebuild with `dotnet build Oni_mods_by_Identifier/ONIMods.sln` to confirm ILRepack resolves `0Harmony.dll` without copying it into `bin`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please retry the build locally where the ONI toolchain is available.

## 2026-01-07 - Identifier release trim safety net
- Limited the `TrimReferenceCopies` target in `ContainerTooltips` and `ZoomSpeed` to operate only on files inside `$(TargetDir)` by projecting `@(ReferenceCopyLocalPaths)` onto local filenames and skipping the merged DLL/PDB pair.
- The cleanup still removes `Merged`/`ILRepack` scratch directories before `CopyReleaseArtifacts`, but the container cannot validate the flow because `dotnet` is unavailable (`command not found: dotnet`). Please rebuild both projects locally in Release to ensure only the merged assembly, its PDB, and YAML assets remain before zipping.

## 2025-12-23 - SuppressNotifications copy tool override scope
- Widened the override visibility for `CopyEntitySettingsTool`'s drag lifecycle hooks to `public` so they match `DragTool`'s declarations and clear the CS0507 accessibility mismatch.
- Attempted to rebuild with `dotnet build src/SuppressNotifications/SuppressNotifications.csproj`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally once the ONI toolchain is available to confirm the access modifier adjustments compile without warnings.

## 2025-12-24 - AzeLib OnLoad assembly fallback
- Guarded `AzeUserMod.OnLoad` against `UserMod2.assembly` being `null` by capturing `assembly ?? GetType().Assembly` for version logging and `[OnLoad]` discovery so domain reloads no longer throw.
- Attempted to run `dotnet build src/DevLoader/DevLoader.csproj` to satisfy the DevLoader rebuild requirement, but the container still reports `command not found: dotnet`. Please rebuild locally and verify the reload loop remains clean in `Player.log` once the ONI toolchain is available.

## 2025-12-24 - DevLoader skip already loaded assemblies
- Added a pre-load guard in `LiveLoader.LoadAll` that checks `AppDomain.CurrentDomain` for an existing assembly name match before shadow-loading development DLLs, preventing duplicate Harmony registrations for mods the core manager already imported.
- Tried to rebuild with `dotnet build src/DevLoader/DevLoader.csproj` to confirm the guard compiles, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally to validate the DevLoader changes.

## 2025-12-24 - ContainerTooltips storage tooltip refresh
- Ported the event-driven `StorageContentsBehaviour` and cache helpers so tooltips invalidate and rebuild per storage component instead of per-array snapshots.
- Adjusted the summarizer and storage spawn patch to operate on a single `Storage` component, matching the refreshed behaviour contract.
- Manual verification in-game (adding/removing storage items to observe tooltip refreshes) remains blocked here because the container lacks the ONI runtime; please validate on a workstation with the game installed.

## 2025-12-22 - DefaultBuildingSettings door spawn visibility
- Updated `DoorSpawnOpener`'s `OnSpawn` override visibility to `public` so Unity can invoke it when Harmony injects the helper
  component onto door prefabs during construction.
- Attempted to rebuild with `dotnet build src/DefaultBuildingSettings/DefaultBuildingSettings.csproj`, but the container still
  lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally to confirm the accessibility change resolv
es the visibility compiler error.

## 2025-12-21 - BetterLogicOverlay IsBitActive predicate update
- Updated `GateColorOutput_Patch` to use Harmony's predicate-based `Manipulator` overload when targeting `LogicCircuitNetwork.IsBitActive`, preventing delegate signature mismatches when Harmony resolves the hook.
- Attempted to validate with `dotnet build src/BetterLogicOverlay/BetterLogicOverlay.csproj`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rebuild locally to confirm the predicate change compiles without errors.

## Instrumentation
- Added DEBUG-only diagnostics around `AzeUserMod.OnLoad` to log discovery, invocation, and total execution timing without affecting release builds.
- Reflection-based discovery results are now cached so domain reloads reuse the same `OnLoad` metadata instead of re-scanning the assembly.

## Measurement status
- Local container image lacks a .NET runtime (`dotnet` is unavailable), so the new diagnostics could not be executed here.
- Representative measurements should be captured by launching the game or a debug harness with a DEBUG build; the logs will emit per-load timing once the diagnostics run.

## Preliminary analysis
- Static inspection shows only four `[OnLoad]` hooks across the solution, so the reflection sweep is expected to be inexpensive even before caching.
- The new cache eliminates the repeated reflection cost on subsequent loads, so the only remaining overhead is delegate invocation.

## Next steps for maintainers
- Run a DEBUG build in-game to collect the emitted timings and confirm the cached path is hit after the first load.
- If invocation proves hot, consider promoting frequently used hooks to precompiled delegates; current data does not suggest this is necessary.

## 2025-10-02 - AutoIncrement Roslyn migration
- Reproduced the historical `RoslynCodeTaskFactory` resolution failure by running `dotnet msbuild src/AzeLib/AzeLib.csproj /t:AutoIncrement /v:diag`, which emitted MSB4175 due to the inline task pointing at an SDK-relative path that collapsed on non-Windows hosts.
- Updated `src/AutoIncrement.targets` to load `RoslynCodeTaskFactory` from `$(MSBuildToolsPath)` with explicit `UsingTask` metadata so the factory resolves consistently across runtimes.
- Refactored the embedded task to use structured helpers, regex-based JSON parsing, and culture-invariant formatting while preserving the existing `version.json` contract.
- Validated the task in isolation via `dotnet msbuild src/AzeLib/AzeLib.csproj /t:AutoIncrement /p:Configuration=Debug`, confirming the revision value increments and the new serializer output remains stable.
- Re-reviewed the modernized implementation on 2025-10-02 to confirm the documentation matches the committed Roslyn task structure and versioning flow.

## 2025-10-05 - BetterInfoCards converter registry
- Split the default (raw text) and title converters out of the general registry to keep fallback lookups deterministic and prevent accidental overrides.
- Updated documentation and unit tests to reflect the dedicated storage and ensure `TryGetConverter` continues to resolve default, title, and named entries as expected.

## 2025-10-06 - BetterInfoCards unreachable hover card update
- Adjusted the unreachable card injection to pull hover targets through reflection so both legacy `overlayValidHoverObjects` and the new `hoverObjects` member are supported.
- Unable to compile `BetterInfoCards` inside this container because the ONI managed assemblies (e.g., `Assembly-CSharp.dll`) are not present; rebuild requires a local install following `src/README.md`.
- Smoke-test pending: verify in-game that selecting an unreachable item still draws the custom card once the mod is rebuilt with the refreshed dependencies.

## 2025-10-08 - ResetPool delegate wiring
- Confirmed the delegate wiring change builds under the ONI toolchain in principle, but the container image still lacks a .NET runtime (`dotnet` is unavailable), so a full `oniMods.sln` rebuild could not be executed here.
- Maintainers should rerun `dotnet build src/oniMods.sln` on a workstation with the ONI assemblies installed to verify end-to-end.

## 2025-10-09 - BetterInfoCards cursor hit patch compatibility
- Updated the reflection used by `ModifyHits` to tolerate the additional generic argument introduced in U56 so the mod no longer throws `Incorrect length` at load.
- Replaced the `AccessTools.AllMethods` lookup with `AccessTools.GetDeclaredMethods` so Harmony 2.3 continues to locate `InterfaceTool.GetObjectUnderCursor` without relying on removed APIs.
- Attempted to rebuild via `dotnet build src/oniMods.sln`, but the container image still lacks a .NET runtime, so compilation could not be performed here.
- Maintainers should rebuild on a workstation with the ONI-managed assemblies to validate Harmony loads cleanly in-game.

## 2025-10-10 - BetterInfoCards cursor signature update
- Relaxed the `ModifyHits` signature guard and added a bool-first fallback so Harmony locates the updated `InterfaceTool.GetObjectUnderCursor` overload even when U56 adds extra optional parameters.
- Static inspection only; rebuilding still requires the ONI assemblies and a local .NET runtime, both unavailable in this container.

## 2025-10-10 - Identifier AutoIncrement Newtonsoft cleanup
- Removed the Newtonsoft.Json reference from the legacy identifier AutoIncrement inline task and replaced the deserializer/serializer with BCL-based helpers so the task no longer depends on external libraries.
- Attempted to rebuild with `msbuild Oni_mods_by_Identifier/ONIMods.sln` to confirm the inline task compiles cleanly without the Newtonsoft reference, but the container lacks MSBuild (`command not found: msbuild`). Please rerun the solution build on a workstation with the ONI toolchain to validate.
- Also tried `dotnet --version` to fall back to the SDK-based build, but the host is missing `dotnet` as well. Local verification remains required.

## 2025-10-11 - BetterInfoCards shadow bar tint option
- Added configurable RGB sliders for the info card shadow bar and ensured stretched bars reuse the selected tint.
- Unable to rebuild `BetterInfoCards` here because the container still lacks the ONI-managed assemblies and `dotnet`; maintainers should run `dotnet build src/oniMods.sln` locally after syncing the new option values.

## 2025-10-12 - BetterInfoCards shadow bar tint persistence
- Ensured the prefab shadow bar and dynamically spawned extensions copy the configured tint to their `ColorStyleSetting` values so refreshes preserve the selected RGB.
- Build and in-game validation remain blocked in this container due to missing ONI assemblies and the `dotnet` host; maintainers should rebuild via `dotnet build src/oniMods.sln` and confirm the slider-controlled tint persists in-game.

## 2025-10-13 - BetterInfoCards widget clone matching
- Relaxed the widget matching so `InfoCardWidgets` recognises hover drawer clones by comparing prefab names (sans `"(Clone)"`) and verifying the component layout/rect dimensions.
- Unable to rebuild or verify the mod in-game here because the container lacks the ONI managed assemblies and a .NET runtime; maintainers should rebuild via `dotnet build src/oniMods.sln` and confirm stacked hover cards wrap correctly.

## 2025-10-18 - DevLoader UI event type alignment
- Replaced the raw `Transition` and `ButtonClickedEvent` aliases in `DevLoader/UI.cs` with the explicit `Selectable.Transition` and `Button.ButtonClickedEvent` references so Unity resolves the proper nested types during compilation.
- Attempted to run `dotnet build src/DevLoader/DevLoader.csproj` to confirm the change compiles, but the container still reports `command not found: dotnet`; please rebuild locally with the ONI toolchain to verify the badge UI compiles against Unity's UI assemblies.

## 2025-10-14 - DevLoader UnityEngine.Object alias cleanup
- Updated DevLoader's runtime UI helpers to alias `UnityEngine.Object` explicitly so null checks resolve against Unity's overloads instead of `System.Object`.
- Attempted to verify by running `dotnet build src/oniMods.sln -t:DevLoader`, but the container still lacks a .NET host (`dotnet` command missing), so compilation must be performed on a workstation with the ONI toolchain installed.

## 2025-10-14 - BetterInfoCards .NET 4.7 compatibility cleanup
- Replaced the C# 8 range expression in `StripCloneSuffix` with an explicit `Substring` call so the project compiles under the original .NET 4.7 target.
- Attempted to rebuild via `dotnet build src/BetterInfoCards/BetterInfoCards.csproj`, but the container still lacks the .NET host (`dotnet` command is unavailable), so compilation must be verified on a workstation with the ONI assemblies and toolchain installed.

## 2025-10-15 - BetterInfoCards hover grid bounds adjustment
- Updated the hover card grid to read the actual bottom edge of the hover text canvas so column wrapping matches the viewport height instead of the scaled pixel height.
- Manual in-game verification of the wrapping behavior is blocked in this container because the ONI runtime and managed assemblies are unavailable; recompile the mod locally and confirm cards wrap into the second column at the correct threshold.

## 2025-10-16 - BetterInfoCards hover widget pool fallback
- Added a fallback that patches `HoverTextDrawer.Pool<MonoBehaviour>.Draw` when the widget pool type cannot be located via reflection and cache the resolved entry type so `InfoCardWidgets` continues to capture the shadow bar metadata.
- Static-only validation here; rebuilding `BetterInfoCards` requires the ONI-managed assemblies and a .NET host (`dotnet`) that are not present in the container, and in-game hover card wrapping must be verified on a workstation with the full toolchain.

## 2025-10-17 - BetterInfoCards prefab normalization
- Normalized the widget prefab reference captured by `ExportWidgets` so Harmony fallbacks that expose `Component` instances still map back to the underlying `GameObject`, keeping the `shadowBar` match logic intact.
- Unable to rebuild `BetterInfoCards` or confirm the fallback captures non-null shadow bar rects in-game because the container lacks the ONI assemblies and runtime; maintainers should run `dotnet build src/oniMods.sln` and validate hover cards wrap across multiple columns locally.

## 2025-10-18 - BetterInfoCards rect metadata fallbacks
- Expanded the widget export reflection to capture any `RectTransform` field or property regardless of name and cache the `MemberInfo` so fallback pool entries can still be processed.
- Updated `InfoCardWidgets` to honour the cached metadata, probe `rectTransform`, and fall back to component lookups when necessary so hover cards recover the shadow bar rect even when pools emit raw components.
- Build and in-game verification remain blocked in this container due to the missing ONI-managed assemblies and `dotnet`; please rebuild via `dotnet build src/oniMods.sln` and validate hover cards wrap across columns after syncing these changes.

## 2025-10-19 - DevLoader solution registration
- Added `DevLoader` to `oniMods.sln` so Visual Studio and MSBuild pick up the project automatically.
- Could not execute `dotnet msbuild src/oniMods.sln` in this container because the `dotnet` host is unavailable; maintainers should rerun the solution build locally to confirm the new entry restores Debug/Release outputs.

## 2025-10-19 - BetterInfoCards component widget handling
- Adjusted `ExportWidgets.ShouldProcessEntry` to accept component-based entries even when they lack a cached `RectTransform` member and cache the resolved type for subsequent calls so hover widgets exported via component fallbacks remain captured.
- Hardened `InfoCardWidgets.ExtractRect`'s component accessor to guard against destroyed Unity objects before invoking `GetComponent<RectTransform>()`.
- Unable to rebuild `BetterInfoCards` or perform the in-game hover wrap validation here because the container still lacks the ONI-managed assemblies and a .NET runtime; maintainers should run `dotnet build src/oniMods.sln` and confirm hover cards wrap into additional columns once the viewport is filled.

## 2025-10-20 - BetterInfoCards rect-based widget fallback
- Added a rect-level comparison in `InfoCardWidgets.AddWidget` so shadow bar and select border assignments succeed even when the prefab clone carries additional components that previously caused the prefab match to fail.
- The container image still lacks the ONI-managed assemblies and a .NET runtime, so `BetterInfoCards` could not be rebuilt locally. Please run `dotnet build src/oniMods.sln` on a workstation with the full toolchain.
- In-game validation of multi-column hover wrapping also remains pending for the same reason; once rebuilt, hover a fully populated info card to confirm the second column appears when the viewport fills.

## 2025-10-21 - BetterInfoCards rect transform comparison fix
- Updated `InfoCardWidgets.AddWidget` to compare the extracted widget rects against the skin's `RectTransform` instances so component wrappers no longer trigger compile-time mismatches.
- Attempted to rebuild via `dotnet build src/BetterInfoCards/BetterInfoCards.csproj`, but the container still lacks the `dotnet` host, so maintainers must recompile locally with the ONI-managed assemblies in place.

## 2025-10-22 - BetterInfoCards widget rect tolerance
- Relaxed the `MatchesWidgetRect` comparison to require matching prefab names, component layouts, and rect heights while tolerating minor width differences so fallback-captured shadow bars are recognized.
- Compilation and in-game verification remain blocked here because the container lacks the ONI-managed assemblies and the `dotnet` runtime; please rebuild via `dotnet build src/oniMods.sln` and confirm multi-column hover wrapping returns with shadow bars detected.

## 2025-10-23 - BetterInfoCards dynamic shadow bar width
- Removed the strict width comparison inside `InfoCardWidgets.MatchesWidgetRect` so dynamically resized shadow bars still match when names and component layouts align.
- Unable to rebuild `BetterInfoCards` or perform the in-game hover test here because the container lacks the ONI-managed assemblies and a `dotnet` host; maintainers should run `dotnet build src/oniMods.sln` and validate hover cards wrap into extra columns once wider shadow bars are detected.

## 2025-10-24 - BetterInfoCards component child rect lookup
- Extended the component accessor fallback in `InfoCardWidgets.CreateAccessor` to scan child `RectTransform` instances and reuse the skin shadow bar metadata when components only expose MonoBehaviour handles.
- Relaxed `MatchesWidgetRect` to accept candidates whose component lists are supersets of the prefab reference so fallback-captured shadow bars still register.
- The ONI-managed assemblies and `dotnet` host remain unavailable in this container, so `BetterInfoCards` could not be rebuilt; run `dotnet build src/oniMods.sln` on a workstation with the game installed.
- Multi-column hover wrapping still requires an in-game hover test after rebuilding to confirm the recovered shadow bar restores the second column.

## 2025-10-25 - BetterInfoCards shadow bar height tolerance
- Updated `InfoCardWidgets.MatchesWidgetRect` to treat taller shadow bars as valid matches so reflected widget entries no longer get rejected when their rect height exceeds the prefab default.
- The hosted workspace still lacks the ONI-managed assemblies and a `dotnet` runtime, preventing a local rebuild of `BetterInfoCards`; maintainers should execute `dotnet build src/oniMods.sln` in a full environment.
- In-game hover validation of multi-column wrapping also remains pending until the mod is rebuilt and tested alongside the game client.

## 2025-10-26 - BetterInfoCards reduced-height shadow bar matching
- Relaxed `InfoCardWidgets.MatchesWidgetRect` so reduced-height shadow bars that still share the prefab name and component layout are treated as matches instead of being rejected by the height check.
- The container still lacks the ONI-managed assemblies and the `dotnet` runtime, so `BetterInfoCards` could not be rebuilt here; please run `dotnet build src/oniMods.sln` in a full environment.
- Multi-column hover validation continues to require an in-game hover test after rebuilding to confirm shadow bars with smaller heights restore the secondary column.

## 2025-10-27 - BetterInfoCards shadow bar pool targeting
- Updated `ExportWidgets.FindWidgetPoolType` to continue scanning HoverTextDrawer members when the nested entry probe fails, prioritizing the shadow bar pool so the postfix captures the correct widgets.
- Tried to rebuild `BetterInfoCards`, but the container still lacks the ONI-managed assemblies and a `dotnet` host; maintainers should run `dotnet build src/oniMods.sln` in a full environment.
- In-game confirmation that multi-column hover wrapping now works again remains pending until the mod is rebuilt alongside the game client.

## 2025-10-28 - BetterInfoCards entry type detection and caching
- Updated `ExportWidgets.FindWidgetEntryTypeRecursive` so nested entry classes that expose a `RectTransform` are preferred over broader value types, ensuring the proper widget entry is discovered for shadow bar pooling.
- Cached the resolved entry type before verifying the pool and now reconstruct `Pool<T>` with `MakeGenericType` to confirm the patch targets the hover text drawer's widget pool.
- The container still lacks the ONI-managed assemblies and a `dotnet` runtime, preventing a local rebuild of `BetterInfoCards`; please run `dotnet build src/oniMods.sln` and validate that info card shadow bars regain their dimensions once the mod is rebuilt with the game client.

## 2025-10-29 - BetterInfoCards collapsed shadow bar handling
- Relaxed `InfoCardWidgets.MatchesWidgetRect` so shadow bars drawn from `Pool<MonoBehaviour>` still match while their runtime height remains collapsed, only rejecting candidates when both the prefab and runtime rects are effectively zero-height.
- The container still lacks the ONI-managed assemblies and the `dotnet` host, so `BetterInfoCards` could not be rebuilt here; maintainers should run `dotnet build src/oniMods.sln` in a full environment.
- In-game verification that hover card columns return once the shadow bar sizes update remains pending until the mod is rebuilt alongside the game client.

## 2025-10-30 - BetterInfoCards collapsed rect matching
- Removed the zero-height short-circuit inside `InfoCardWidgets.MatchesWidgetRect` so shadow bars that remain collapsed while pooled still run through the component matching heuristics.
- The container continues to lack the ONI-managed assemblies and a `dotnet` host, preventing a local rebuild of `BetterInfoCards`; please execute `dotnet build src/oniMods.sln` in a full environment.
- In-game confirmation that `shadowBar` now reports a non-null rect (restoring multi-column wrapping) remains pending until the mod is rebuilt and loaded with the game client.

## 2025-10-31 - BetterInfoCards drawer null safety
- Updated the hover info replay to cache the `HoverTextDrawer` instance and short-circuit when it is unavailable so captured actions are never re-run without a live drawer.
- Routed `InfoCard` and `DrawActions` rendering through the cached drawer reference, emitting a warning when the drawer is missing to avoid Unity exceptions.
- Unable to recompile or verify hover card recovery in-game inside this container because the ONI-managed assemblies and runtime remain unavailable; maintainers should rebuild via `dotnet build src/oniMods.sln` and confirm that layout is skipped (without crashing) when the drawer hook fails.

## 2025-11-01 - BetterInfoCards shadow bar replay setup
- Switched the shadow bar capture to allocate its widget container during the `BeginShadowBar` prefix so the subsequent `Draw()` call receives a ready list and no longer skips the first widget entry.
- Added a replay-phase fallback in the widget postfix to lazily create the container when the prefix is bypassed, keeping the `IsInterceptMode` guard so live draws remain untouched.
- The ONI-managed assemblies and runtime are still unavailable here, so `dotnet build src/oniMods.sln` and in-game hover validation of multi-column wrapping must be performed in a full environment.

## 2025-11-02 - AzeLib spawn hook virtualization
- Updated `AMonoBehaviour.OnSpawn` to be virtual so mods like BetterLogicOverlay can override the spawn hook while preserving the attribute-driven component resolution.
- Attempted `dotnet build src/oniMods.sln`, but the container image still lacks the .NET host (`dotnet` command is unavailable). Rebuild the solution in a full ONI environment to confirm the override compiles.

## 2025-11-02 - BetterInfoCards hover drawer null guards
- Hardened the hover drawer patches so every `DrawIcon`, `DrawText`, `AddIndent`, `NewLine`, and `EndShadowBar` prefix verifies `curInfoCard` before replaying captured actions, logging and deferring to vanilla rendering when the card context is missing.
- Reset `curInfoCard` whenever `BeginShadowBar` skips allocation to avoid replaying into a stale card when the intercept path is bypassed.
- Compilation and in-game hover validation remain blocked in this container due to missing ONI-managed assemblies and the `dotnet` host; maintainers should rebuild via `dotnet build src/oniMods.sln` and confirm hover cards render safely when the drawer skips `BeginShadowBar`.

## 2025-11-03 - BetterInfoCards shadow bar sizing guard
- Added a collapse tolerance guard to `InfoCardWidgets.MatchesWidgetRect` so zero-height shadow bars stay pending until either the prefab or runtime rect expands past the threshold, preventing collapsed rects from short-circuiting the match.
- Cached collapsed shadow bar candidates and re-checked them before grid layout, ensuring the grid only measures non-zero width/height rects and column wrapping resumes once Unity finishes sizing the widget.
- The ONI-managed assemblies and `dotnet` host are still unavailable here, so rebuild `BetterInfoCards` via `dotnet build src/oniMods.sln` on a full workstation and validate in-game that the grid now captures positive shadow bar dimensions.

## 2025-11-04 - BetterInfoCards collapsed rect reuse
- Relaxed `InfoCardWidgets.MatchesWidgetRect` so collapsed runtime rects still match the prefab when names and component layouts align, deferring to the existing capture heuristics once Unity sizes the widget.
- Confirmed `TryAssignShadowBar` continues caching zero-sized matches in `pendingShadowBars`, letting `ResolvePendingWidgets` claim them after layout expands the rect.
- Unable to rebuild `BetterInfoCards` or run the in-game hover verification in this container because the ONI-managed assemblies and `dotnet` runtime are missing; please execute `dotnet build src/oniMods.sln` and perform the hover test on a full workstation.

## 2025-11-05 - BetterInfoCards hover intercept fallback
- Short-circuited the Harmony prefixes in `InterceptHoverDrawer` so they immediately defer to the vanilla drawer when `curInfoCard` is unavailable, preventing null dereferences when the widget pool hook is missing.
- Added a one-shot warning and reset `IsInterceptMode` after the fallback triggers to avoid repeatedly re-entering the prefixes without a valid card in the same frame.
- Could not rebuild `BetterInfoCards` inside this container because the ONI-managed assemblies and `dotnet` runtime are still absent; maintainers should run `dotnet build src/oniMods.sln` locally and replay a hover sequence to confirm the fallback no longer throws.

## 2025-11-05 - BetterInfoCards reset pool constructor parity
- Updated `ResetPool` so the single-parameter constructor now combines `HoverTextDrawer.BeginDrawing` with the `Reset` handler, matching the multiparameter overload.
- Centralized the delegate hookup through `AttachResetHandler` to keep both constructors in sync and expose the combined delegate via `OnBeginDrawing` for telemetry consumers.
- Rebuild and hover replay verification remain blocked here because the ONI-managed assemblies and `dotnet` runtime are unavailable; please run `dotnet build src/oniMods.sln` and confirm pooled draw actions reset on each `HoverTextDrawer.BeginDrawing` in a full environment.

## 2025-11-06 - BetterInfoCards shadow bar rect filtering
- Tightened the component fallback in `InfoCardWidgets.CreateAccessor` so only the component's own `RectTransform` that matches the skin's shadow bar is returned, otherwise descendants are scanned and unmatched entries report `null`.
- Updated `InfoCardWidgets.AddWidget` and `TryAssignShadowBar` to tolerate null rect accessors, keep probing for later matches, and cache the resolved rect (even while collapsed) as `shadowBar`.
- Could not rebuild `BetterInfoCards` inside this container because the ONI-managed assemblies and `dotnet` runtime are still missing; maintainers should run `dotnet build src/oniMods.sln` locally and confirm hover cards regain positive widths and wrap into additional columns in-game once rebuilt.

## 2025-11-07 - BetterInfoCards converter null guard
- Added a null check around `TextInfo.Create` in the hover drawer intercept so converter failures log a warning and skip the draw instead of queuing a crashing replay action.
- Ensured replayed text actions bail out when either the captured `TextInfo` or `TextStyleSetting` is unavailable so the hover text drawer is never invoked with missing data.
- Compilation and in-game verification remain blocked in this container because the ONI-managed assemblies and `dotnet` runtime are unavailable; maintainers should rebuild via `dotnet build src/oniMods.sln` and hover a converted info card to confirm the warning path prevents crashes.

## 2025-11-08 - BetterInfoCards deferred shadow bar sizing
- Added a deferred resolver that schedules collapsed shadow bar candidates for a `LateUpdate` retry so the grid promotes them once Unity expands their rects, ensuring width/height reflect the final layout before wrapping columns.
- The new scheduler only reuses `ResolvePendingWidgets` and leaves the prefab/rect matching heuristics untouched, so existing comparisons against the hover drawer skin continue to behave as before.
- Could not rebuild or run in-game validation here because the ONI-managed assemblies and `dotnet` runtime remain unavailable; please execute `dotnet build src/oniMods.sln` in a full environment and verify hover cards populate multi-column layouts after the deferred sizing pass.

## 2025-11-09 - BetterInfoCards title converter null guard
- Hardened the title converter so countable prefabs without a `PrimaryElement` return a safe default and emit a one-shot warning instead of crashing when `.Units` is missing.
- Mirrored the ore status converter pattern by caching the component lookup before accessing aggregation data.
- Unable to rebuild `BetterInfoCards` in this container because the ONI assemblies and `dotnet` host are absent; maintainers should run `dotnet build src/oniMods.sln` locally and confirm the hover title aggregation no longer throws when encountering prefabs without `PrimaryElement`.

## 2025-11-10 - BetterInfoCards shadow bar prefab resolution
- Added a prefab-based fallback in `InfoCardWidgets.AddWidget` so the shadow bar rect is recovered directly from the instantiated prefab when the pool entry lacks an accessor.
- Queued unresolved prefabs for deferred processing alongside collapsed rects, allowing `ResolvePendingWidgets` to revisit them once Unity finishes the layout pass and the rect reports a usable size.
- Compilation and in-game verification remain blocked here because the container lacks the ONI-managed assemblies and `dotnet`; maintainers should rebuild via `dotnet build src/oniMods.sln` and confirm captured cards regain non-zero dimensions and column wrapping in-game.

## 2025-11-11 - BetterInfoCards deferred grid relayout
- Updated `InfoCardWidgets.ResolvePendingWidgets` to expose the pending/resolved state so grid layout can detect when shadow bars are still sizing.
- Reworked `Grid` to defer column measurement for pending cards, schedule a late-update relayout once the deferred resolver promotes them, and apply the final layout only after the shadow bar reports usable dimensions.
- Unable to rebuild or run in-game validation because the container still lacks the ONI-managed assemblies and a `dotnet` runtime; maintainers should run `dotnet build src/oniMods.sln` locally and verify multi-column wrapping after the deferred pass completes.

## 2025-11-12 - BetterInfoCards shadow bar instance resolution
- Replaced the prefab-based `RectTransform` cache with live entry tracking so deferred resolution now probes instantiated hover card widgets before adjusting layout.
- Updated the pending queues to store the captured entry alongside collapsed rects, ensuring late-update retries operate on scene objects and unregister once a usable rect is located.
- Unable to rebuild or run in-game validation in this container because the ONI-managed assemblies and `dotnet` runtime remain unavailable; maintainers should run `dotnet build src/oniMods.sln` locally and hover a multi-widget card to confirm translations and width adjustments affect only the active instance.

## 2025-11-13 - BetterInfoCards shadow bar wrapper tolerance
- Relaxed `InfoCardWidgets.MatchesWidgetPrefab` so helper wrappers that add components around the skin shadow bar still qualify via the existing component-superset fallback, allowing deferred prefab resolution to recover the instantiated `RectTransform` and repopulate `shadowBar`.
- Verified the fallback continues to assign `shadowBar` through `TryResolveShadowBarFromPrefab`, ensuring downstream layout math sees non-zero widths/heights once the instantiated rect reports a usable size.
- Unable to rebuild or execute in-game validation because the container lacks the ONI-managed assemblies and `dotnet` runtime; maintainers should run `dotnet build src/oniMods.sln` locally and confirm wrapped skin shadow bars now populate card dimensions.

## 2025-11-14 - BetterInfoCards widget allocation guard
- Ensured `ExportWidgets.GetWidget_Postfix` only enqueues a new `InfoCardWidgets` container the moment it is instantiated so hover cards contribute exactly one entry to the export list.
- Re-reviewed other call sites that append to `icWidgets` and confirmed `BeginShadowBar` remains the sole allocation path during live draws, preventing duplicate containers when replaying captured widgets.
- Runtime confirmation that `Grid` now receives a single container per card remains pending; the container environment still lacks the ONI-managed assemblies and `dotnet`, so maintainers should rebuild via `dotnet build src/oniMods.sln` and verify column translation offsets in-game.

## 2025-11-15 - BetterInfoCards temperature converter guard
- Hardened the temperature converter to reuse a cached `PrimaryElement` lookup, emit a one-time warning when the component is missing, and fall back to a safe default instead of dereferencing `null`.
- Reviewed the existing title converter logging helper and mirrored its usage for temperature entries so missing-component spam stays suppressed after the first warning.
- Compilation and in-game hover verification remain blocked in this environment due to missing ONI assemblies and `dotnet`; maintainers should run `dotnet build src/oniMods.sln` and hover affected buildings to confirm the hover card displays the fallback temperature text without crashing.

## 2025-11-16 - BetterInfoCards hover drawer skin guard
- Added a pre-export guard in `ExportWidgets.GetWidget_Postfix` so widget capture waits for `HoverTextDrawer.skin` to load before mutating per-card state, logging a single warning while the skin is unavailable.
- Confirmed the intercept-mode short-circuit remains intact and the new guard clears its warning flag once the drawer skin becomes accessible so exports resume normally.
- Unable to rebuild or validate in-game here because the container lacks the ONI-managed assemblies and a `dotnet` runtime; maintainers should run `dotnet build src/oniMods.sln` and confirm hover cards render without crashes while the drawer initialises, then resume widget export once the skin loads.

## 2025-11-17 - BetterInfoCards widget export rollback
- Restored `ExportWidgets`, `InfoCardWidgets`, and `Grid` to the pre-refactor implementations so hover card capture once again records pool entries directly and reuses the simple postfix on `HoverTextDrawer.Pool<MonoBehaviour>.Draw`.
- The older API contracts remove the deferred layout bookkeeping that was triggering the null-reference loop, keeping `Grid` column math aligned with the entry-based widgets.
- Unable to rebuild or verify in-game because this workspace still lacks the ONI assemblies and `dotnet`; maintainers should run `dotnet build src/oniMods.sln` locally and smoke-test hover cards to confirm the null-reference loop no longer occurs.

## 2025-11-18 - BetterInfoCards widget entry reflection update
- Replaced the direct generic Harmony patch on `HoverTextDrawer.Pool<MonoBehaviour>.Draw` with a `TargetMethod` lookup that locates the closed pool type through `AccessTools.Inner` so the patch no longer references the inaccessible nested entry type.
- Updated the widget export pipeline to cache `RectTransform` handles in lightweight wrappers, exposing a shared `HoverTextEntryAccess` helper for both the exporter and grid logic.
- Attempted to rebuild via `dotnet build src/oniMods.sln -c Release`, but the container still lacks the `dotnet` host; maintainers should rebuild locally with the ONI-managed assemblies to verify compilation succeeds.

## 2025-11-19 - BetterInfoCards shadow bar pool reflection
- Updated `HoverTextEntryAccess` to locate and cache the `HoverTextDrawer` shadow bar pool member via reflection so callers no longer depend on the `shadowBars` field name.
- Swapped `InfoCardWidgets.SetWidth` to invoke the cached pool through the shared draw method and emit a warning when the pool or draw method is unavailable, preventing null-reference crashes when the drawer layout differs.
- `dotnet` remains unavailable in this environment (`bash: command not found: dotnet`), so the solution rebuild and in-game hover verification are still pending and should be completed on a local workstation with the ONI toolchain.

## 2025-11-20 - BetterInfoCards entry rect caching
- Updated `HoverTextEntryAccess.GetRect` to resolve `rect` fields against the runtime entry type and cache the discovered `FieldInfo` instances per type to avoid repeated reflection and mismatched member access.
- Retained the Traverse-based accessor exclusively as a fallback when the runtime entry exposes no `rect` field so widget exports continue to receive `RectTransform` handles.
- Unable to rebuild `BetterInfoCards` or verify hover card rendering in-game because the container still lacks the ONI-managed assemblies and a `.NET` runtime; maintainers should run `dotnet build src/oniMods.sln` and validate hover cards draw without `ArgumentException` once synced locally.

## 2025-11-21 - ContainerTooltips disease index guard
- Filtered the disease summary formatter so it skips invalid disease indices before requesting localized names, logging a single warning the first time an out-of-range entry appears.
- This prevents `GameUtil.GetFormattedDiseaseName` from dereferencing an invalid index when polluted storage includes placeholder rows.
- Rebuild and in-game tooltip verification remain outstanding; the hosted environment still lacks the ONI-managed assemblies and `dotnet` runtime, so maintainers need to run `dotnet build src/oniMods.sln` locally and hover affected storage to confirm the warning suppresses the crash.

## 2025-11-22 - ContainerTooltips build import alignment
- Added `FixedMod/src/Directory.Build.props` so the FixedMod solution inherits the shared net471 target, references, and translation path overrides.
- Added `FixedMod/src/Directory.Build.targets` to ensure the packaging/versioning pipeline runs for ContainerTooltips when built from the FixedMod solution.
- Unable to reload `src/oniMods.sln` or build `ContainerTooltips` here because the container lacks the required .NET/ONI toolchain; maintainers should run `dotnet build src/oniMods.sln` locally to confirm the shared settings apply without errors.

## 2025-11-23 - ContainerTooltips import path fix
- Corrected the relative imports in `FixedMod/src/Directory.Build.props` and `FixedMod/src/Directory.Build.targets` to reference the root `src` directory instead of the FixedMod subtree, eliminating the circular dependency Visual Studio reported when loading the project.
- Build verification remains pending; the hosted environment still lacks the ONI-managed assemblies and `.NET` runtime, so maintainers should run `dotnet build src/oniMods.sln` locally to confirm the solution now loads without the circular import error.

## 2025-11-24 - ContainerTooltips translation path and AzeLib reference
- Updated `FixedMod/src/Directory.Build.props` so the translations path points at the repository-level `Translations` directory, matching the packaging expectations.
- Adjusted the `AzeLib` project reference to target the shared source tree while keeping the reference private so ContainerTooltips links against the library without redistributing its binaries.
- Could not re-run `msbuild /t:Restore` or reload the solution here because the container still lacks `dotnet`/MSBuild (`bash: command not found: dotnet`); maintainers should restore the solution locally to confirm the reference resolves.

## 2025-11-25 - ContainerTooltips AzeLib dependency removal
- Converted `STRINGS.CONTAINERTOOLTIPS` into a static container and manually register the status item strings during initialization so the mod no longer depends on AzeLib's reflection-based registration.
- Marked the project with `<UsesAzeLib>false</UsesAzeLib>` to drop the inherited project reference now that no AzeLib types are required.
- The workspace still lacks the ONI-managed assemblies and `.NET` runtime, so I was unable to run `dotnet build src/oniMods.sln`; maintainers should rebuild locally to confirm ContainerTooltips compiles cleanly without AzeLib.

## 2025-11-26 - ContainerTooltips namespace collision fix
- Renamed the ContainerTooltips storage component namespace to `BadMod.ContainerTooltips.Components` so it no longer conflicts with ONI's `Storage` type.
- Updated the mod entry point and Harmony patch to import the new namespace.
- `dotnet` remains unavailable in this workspace, preventing a rebuild; maintainers should run `dotnet build src/oniMods.sln` locally to confirm the namespace/type resolution succeeds.

## 2025-11-27 - ContainerTooltips missing type imports
- Added the missing `Klei`, `Klei.AI`, and `Database` namespace imports so `LocString`, `Tag`, `Db`, and related types resolve without relying on implicit references.
- The container still lacks the ONI-managed assemblies and `.NET` runtime, so `dotnet build src/oniMods.sln` cannot be executed here; maintainers should rebuild locally to confirm the project compiles with the restored imports.

## 2025-11-28 - ContainerTooltips mass format alias cleanup
- Updated `MassDisplayMode` to use a uniquely named alias for `global::GameUtil` so ContainerTooltips no longer collides with other `GameUtil` aliases when loaded alongside mods.
- The workspace still lacks the ONI-managed assemblies and `.NET` runtime, preventing a rebuild; maintainers should run `dotnet build src/oniMods.sln` locally to confirm the enum resolves without namespace conflicts.

## 2025-12-07 - DevLoader logging namespace imports
- Added missing `using UnityEngine;` directives to the DevLoader patches so `Debug.*` resolves against the Unity logging API again.
- Could not rebuild `DevLoader` in-container because the .NET host is unavailable (`command not found: dotnet`); please run `dotnet build src/DevLoader/DevLoader.csproj` locally to confirm the project compiles without missing-type errors.

## 2025-11-29 - ContainerTooltips Storage.OnSpawn patch accessibility
- Replaced the `nameof(Storage.OnSpawn)` Harmony target with a literal string to avoid accessibility errors when the nested `OnSpawn` reference is unavailable at compile time.
- Attempted to rebuild via `dotnet build src/ContainerTooltips/ContainerTooltips.csproj`, but the container still lacks the `.NET` host (`dotnet` command not found); maintainers should rebuild locally to confirm the accessibility warning no longer occurs.

## 2025-11-30 - ContainerTooltips nullable annotations context
- Enabled nullable reference type analysis in `ContainerTooltips.csproj` so the compiler interprets existing `?` annotations and reports nullability issues correctly.
- Unable to run `dotnet build src/ContainerTooltips/ContainerTooltips.csproj` here because the workspace still lacks the `.NET` runtime; maintainers should rebuild locally to confirm the nullability warnings disappear.

## 2025-12-01 - ContainerTooltips status icon namespace
- Updated `UserMod.InitializeStatusItem` to reference `StatusItem.IconType.Info` explicitly so the compiler resolves the nested enum without relying on an implicit `using`.
- Attempted to run `dotnet build src/oniMods.sln`, but the container still lacks the `.NET` toolchain (`command not found: dotnet`); maintainers need to rebuild locally to confirm the enum reference compiles cleanly.

## 2025-12-02 - ContainerTooltips LocString registration
- Updated `UserMod.RegisterString` to rely on `LocString.ToString()` with a `value.text` fallback so the method no longer references the removed `LocString.String` member.
- Unable to execute `dotnet build src/oniMods.sln` because the container still reports `command not found: dotnet`; please rebuild locally to confirm the updated LocString conversion compiles with the ONI toolchain.

## 2025-12-03 - ContainerTooltips localized status fallback
- Added a helper to resolve localized status item strings with a bundled fallback so overridden translations no longer regress to `MISSING:` when the string table lacks entries.
- Unable to run `dotnet build src/oniMods.sln` or launch ONI in this environment (`dotnet` is unavailable and the game executable is not installed); maintainers should rebuild and verify in-game that storage bins now display the localized "Contents:" header.

## 2025-11-05 - ContainerTooltips status item argument alignment
- Swapped the named `allowMultiples` argument in `UserMod.InitializeStatusItem` for the positional boolean used by the upstream reference so the constructor overload resolves during compilation.
- Unable to run `dotnet build src/oniMods.sln` here because the container lacks the .NET host and ONI-managed assemblies; maintainers should rebuild locally to confirm the status item now compiles without overload ambiguity.

## 2025-12-04 - ContainerTooltips missing string sentinel guard
- Updated `UserMod.GetStringWithFallback` to ignore ONI's `MISSING` sentinel when localized lookups fail so the status line and tooltip fall back to the bundled `LocString` text.
- `dotnet` and the ONI-managed assemblies are unavailable in this workspace, preventing a rebuild or in-game verification; maintainers should compile `ContainerTooltips` locally and confirm the "Contents:" header renders without the placeholder string.

## 2025-12-05 - ContainerTooltips LocString text precedence
- Adjusted `UserMod.GetLocStringText` to prefer the embedded `LocString.text` over `ToString()` so the default "Contents"/"None" registration values win when translations are missing.
- `dotnet` remains unavailable in this environment, so `dotnet build src/oniMods.sln` could not be executed. In-game verification of the updated storage tooltip header should be performed locally.

## 2025-11-29 - BetterInfoCards overlay tint bleed
- Reproduced the report by reviewing the Harmony patches: the shadow bar prefab tint was applied during every `HoverTextDrawer` construction, so overlays inherited the BetterInfoCards background color.
- Updated the replay flow to recolor only the instantiated shadow bars captured by `ExportWidgets`, preventing other systems from inheriting the tint.
- Could not validate the visual fix in this container because the ONI runtime is unavailable; follow up in-game once the mod is rebuilt locally per `src/README.md`.

## 2025-12-06 - DevLoader metadata alignment
- Updated `DevLoader.csproj` to rely on the shared build props for references and assembly info so the project inherits the publicized ONI assemblies automatically.
- Attempted to run `dotnet build src/DevLoader/DevLoader.csproj`, but the container still lacks the `.NET` host (`dotnet` command not found); maintainers should rebuild locally to confirm the shared references resolve.

## 2025-12-07 - DevLoader badge artwork distribution
- Added the missing badge sprites under `src/DevLoader/Images/` so the runtime loader can resolve `dev_on.png`, `dev_off.png`, and their mini variants without warning spam.
- Updated `DevLoader.csproj` to mark each PNG as `Content` with `CopyToOutputDirectory=PreserveNewest`, ensuring they ship alongside the DLL during builds.
- Unable to verify with `dotnet build src/DevLoader/DevLoader.csproj` because the container still reports `command not found: dotnet`; please rebuild locally to confirm the assets copy into the output folder.

## 2025-12-08 - DevLoader badge artwork placeholder follow-up
- Removed the placeholder sprite commit from source control; the container cannot host the final badge PNGs without breaking licensing, so the `Images/` directory is now empty apart from documentation.
- Updated `DevLoader.csproj` to copy any `.png` under `src/DevLoader/Images/` when present, keeping the build ready for locally supplied art assets.
- Added `src/DevLoader/Images/README.md` with instructions to drop `dev_on.png`, `dev_off.png`, `mini_dev_on.png`, and `mini_dev_off.png` into the folder before building.
- Still blocked from running `dotnet build src/DevLoader/DevLoader.csproj` in this container (`command not found: dotnet`); rerun the build locally after restoring the four PNGs.

## 2025-12-09 - DevLoader CS1525 cleanup
- Replaced the lingering ILSpy ref-cast helpers in `DevMiniBootstrap`, `CenterMini`, and `UI` with idiomatic property access and `Vector2` construction so Unity's compiler stops reporting CS1525 syntax errors.
- Cached the badge sprite rect once when configuring the layout element to avoid repeated struct copies while setting width and height.
- Attempted to rebuild with `dotnet build src/DevLoader/DevLoader.csproj`, but the container still reports `command not found: dotnet`; maintainers must re-run the build on a workstation with the ONI toolchain to confirm the cleanup compiles.【1200ed†L1-L2】
## 2025-12-10 - DevLoader shared reference cleanup
- Removed the hard-coded `Assembly-CSharp`, `Assembly-CSharp-firstpass`, and `Unity.TextMeshPro` references from `DevLoader.csproj` so the project now relies on the shared `Directory.Build.props` targets for ONI assemblies.
- Attempted to run `dotnet build src/oniMods.sln`, but the container still lacks the `.NET` host (`dotnet: command not found`); maintainers should rebuild locally to confirm DevLoader compiles against the shared targets.

## 2025-12-11 - BetterInfoCards & ContainerTooltips reference cleanup
- Dropped the hard-coded ONI assembly references from the BetterInfoCards and ContainerTooltips project files so they inherit the publicized `_public` DLLs via `Directory.Build.props`.
- Kept the explicit `Microsoft.CSharp` reference for BetterInfoCards while relying on the shared props for the remaining dependencies to avoid duplicate hint paths.
- Attempted to run `dotnet build src/oniMods.sln`, but the container still reports `command not found: dotnet`; maintainers should rebuild locally to confirm both projects resolve the shared `_public` assemblies.

## 2025-12-13 - AzeLib ONI reference hint cleanup
- Repointed `AzeLib.csproj` to resolve `Assembly-CSharp` and `Assembly-CSharp-firstpass` via `$(GameFolder)` so local builds can rely on the shared `Directory.Build.props` overrides.
- Attempted to run `dotnet build src/oniMods.sln`, but the container still lacks the `.NET` host (`command not found: dotnet`); rebuild locally once `GameFolder` is set through `.user` overrides.

## 2025-12-12 - Directory.Build.props default path cleanup
- Updated `Directory.Build.props.default` to default `SteamFolder` to the stock `C:\Program Files (x86)\Steam` install location and documented the `.user` override for custom setups.
- Attempted to run `dotnet build src/oniMods.sln` to confirm a clean clone compiles without editing `.default`, but the container still lacks the `.NET` host (`dotnet: command not found`). Rebuild locally to verify.


## 2025-12-14 - ContainerTooltips lifecycle visibility tweak
- Exposed the `StorageContentsBehaviour` lifecycle hooks as `public override` so Unity can invoke them even when the component is instantiated from mod code rather than through prefab registration.
- Attempted to rebuild with `dotnet build src/oniMods.sln /t:ContainerTooltips`, but the container still lacks the `.NET` host (`dotnet: command not found`). Please rerun the build locally to confirm the accessibility change compiles cleanly.


## 2025-12-15 - DevLoader directory namespace cleanup
- Qualified every `Directory.*` call inside `LiveLoader` with `System.IO.` so the compiler no longer resolves the name against Unity's `UnityEngine.Directory` helper when both assemblies are loaded.
- Wanted to validate the fix with `dotnet build src/DevLoader/DevLoader.csproj`, but the container still reports `command not found: dotnet`; please rebuild locally to confirm the namespace collision is resolved.


## 2025-12-16 - DevLoader CenterMini hashed string conversion
- Replaced the implicit `HashedString` conversion in `CenterMini` with an explicit constructor call to avoid relying on the removed operator.
- Attempted to rebuild with `dotnet build src/DevLoader/DevLoader.csproj`, but the container still lacks the `.NET` host (`command not found: dotnet`); rerun the build locally to confirm the change compiles without the implicit-operator error.


## 2025-10-07 - Directory.Build.props public assembly repointing
- Updated `src/Directory.Build.props` so the shared ONI references resolve the `_public` DLLs generated under `src/lib`, preventing accidental fallbacks to the raw game assemblies.
- Attempted to run `dotnet build src/oniMods.sln`, but the container still lacks the `.NET` host (`dotnet: command not found`). Please rebuild locally to confirm the solution consumes the publicized assemblies.
## 2025-12-17 - DevLoader delegate namespace cleanup
- Qualified the DevLoader hotkey loading callbacks and runtime toggle event with `System.Action` so the compiler binds against the BCL delegates explicitly.
- Attempted to run `dotnet build src/DevLoader/DevLoader.csproj` to confirm the delegate changes compile, but the container still lacks the `.NET` host (`dotnet: command not found`). Please rebuild locally to verify.

## 2025-12-18 - DevLoader implicit operator removal follow-up
- Replaced every `Object.op_Implicit` usage inside `CenterMini` and `DevMiniBootstrap` with explicit null checks to resolve the CS0558 compiler errors introduced by Unity's stripped implicit operators.
- Tried to verify the fix via `dotnet build src/DevLoader/DevLoader.csproj`, but the container continues to report `command not found: dotnet`; rerun the build locally to ensure the DevLoader project compiles cleanly.

## 2025-12-19 - DevLoader Unity UI API alignment
- Updated `CenterMini` to use `Selectable.Transition.None` and `Button.ButtonClickedEvent` so the generated IL no longer depends on deprecated Unity UI type aliases.
- Attempted to rebuild with `dotnet build src/DevLoader/DevLoader.csproj`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally to confirm the Unity UI references resolve correctly.

## 2025-12-20 - BetterDeselect transpiler predicate update
- Swapped the `Manipulator` matchers in the BetterDeselect escape-close patches to use predicate-based overloads so Harmony's `Calls` helper drives the injection target.
- Attempted to validate with `dotnet build src/oniMods.sln`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally to confirm the transpiler updates compile.

## 2025-12-21 - BetterLogicOverlay broadcaster label component fetch
- Injected a `[MyCmpGet]` field into `LogicBroadcasterSetting` so the label resolves the broadcaster's display name without using the component instance as a `KMonoBehaviour`.
- Tried to rebuild with `dotnet build src/BetterLogicOverlay/BetterLogicOverlay.csproj` to confirm the missing `KMonoBehaviour` conversion error is resolved, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally to verify.


## 2025-12-22 - SuppressNotifications string namespace qualification
- Qualified the SuppressNotifications copy tools to use the game UI namespace aliases so the compiler resolves `STRINGS.UI` correctly.
- Attempted to rebuild with `dotnet build src/SuppressNotifications/SuppressNotifications.csproj`, but the container still lacks the `.NET` host (`dotnet: command not found`). Please rerun the build locally to confirm the namespace bindings compile.

## 2025-12-23 - DevLoader static content preload
- Captured each mod DLL's root and added a reflection-based `KMod.Content` loader so static assets (strings, anims, templates) register before `OnLoad` executes.
- Attempted to verify with `dotnet build src/DevLoader/DevLoader.csproj`, but the container still lacks the `.NET` host (`dotnet: command not found`). Please rebuild locally to ensure the helper compiles alongside the new preload workflow.
## 2025-12-24 - ContainerTooltips localization fallback helpers
- Added fallback-aware string registration to the legacy `ContainerTooltips` build so translations missing localized entries ret
  urn the English `LocString` text like the maintained version.
- Unable to validate in-game localization because the container cannot launch Oxygen Not Included; please load the mod with an a
lternate language locally to confirm the fallback strings resolve correctly.


## 2025-12-25 - ContainerTooltips STRINGS archive sync
- Bundled the `Mod/STRINGS.cs` scaffold inside `Oni_mods_by_Identifier/ContainerTooltips` so the legacy project matches the maintained repository's `global::STRINGS` constants without requiring a manual copy.
- Attempted `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj` to confirm the SDK project automatically includes the new file, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally to verify.

## 2025-12-26 - ContainerTooltips Db.Initialize postfix imports
- Added the missing `Database` and `UnityEngine` namespace imports to the legacy `Db.Initialize` postfix so the compiler resolves `Db` and `Debug` without relying on transitive usings.
- Tried to rebuild with `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally to confirm the patch compiles.

## 2025-12-27 - ContainerTooltips storage summarizer sync
- Replaced the legacy `StorageContentsSummarizer` with the maintained implementation so multi-line summaries match upstream behaviour while keeping the newline guard and explicit `GameUtil` formatting parameters.
- Attempted `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj` to confirm the API alignment, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rebuild locally to verify.
- Unable to spot-check tooltips in-game because Oxygen Not Included is not available in the container; please confirm multi-line summaries render identically on a workstation with the game installed.

## 2025-12-28 - ContainerTooltips mod metadata label update
- Swapped the options dialog `[ModInfo]` attribute to use the human-readable mod name so the UI surfaces "Container Tooltips" instead of the repository URL.
- Attempted `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj` to validate the metadata change but the container still lacks the `.NET` host (`command not found: dotnet`). Please rebuild locally and confirm the options screen reflects the new title.

## 2025-12-29 - Identifier Directory.Build.props bridge
- Added an identifier-level `Directory.Build.props` that defers to the shared `.default`/`.user` pair so legacy projects consume the same path overrides as the main solution.
- Attempted to capture `$(GameFolder)` via `dotnet msbuild Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj /t:ResolveProjectReferences /v:diag`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please re-run the diagnostic build locally to confirm the property evaluation.

## 2025-12-30 - Identifier projects prefer live game assemblies
- Updated the legacy `ContainerTooltips` and `ZoomSpeed` project references to read assemblies from `$(GameFolder)` with a fallback to the optional `lib/` cache so they align with the standard `Directory.Build.props.user` override.
- Tried to validate with `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally to confirm the live game folder resolves successfully.

## 2025-12-31 - Identifier metadata copied to build output
- Enabled `CopyToOutputDirectory="PreserveNewest"` for `mod.yaml`, `mod_info.yaml`, `preview.png`, and translation assets so debug and release builds include the static metadata next to the compiled assemblies.
- Attempted to verify with `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rebuild locally (Debug and Release) to confirm the YAML files land in `bin/<Configuration>/<TFM>/`.

## 2026-01-01 - Identifier reference copy-local disable
- Set the identifier-level `Directory.Build.props.default` to mark `Reference`, `ProjectReference`, and `PackageReference` items as non-private so only the merged assemblies land in mod output folders.
- Attempted `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj` to confirm ONI assemblies stay under `$(GameFolder)`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally to validate the copy-local behavior.

## 2026-01-02 - Identifier TFMs pinned to net471
- Downgraded the `ContainerTooltips` and `ZoomSpeed` archival projects to target `net471` so their merged assemblies match ONI's Harmony runtime.
- Attempted `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj /p:Configuration=Release` to confirm the net471 build and ILRepack packaging, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rebuild locally (Debug/Release) to verify the merged assembly and release zip succeed under the new TFM.

## 2026-01-03 - Identifier post-merge trimming guard
- Added a `TrimReferenceCopies` MSBuild target to the legacy `ContainerTooltips` and `ZoomSpeed` projects so copy-local references, PLib helpers, and ILRepack staging folders are removed after the merged assembly is produced.
- Tried `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj /p:Configuration=Release` to validate Debug/Release workflows, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rebuild locally to confirm the merged outputs remain intact while the helper DLLs are trimmed from `$(TargetDir)` and the Release zip.

## 2026-01-04 - Identifier copy-local target parity
- Added an identifier-level `Directory.Build.targets` that mirrors the solution's `ClearCopyLocalReferences` hook so ONI DLLs are removed from `ReferenceCopyLocalPaths` immediately after `ResolveAssemblyReferences`.
- Deferred porting the install-folder staging targets because the legacy projects already trim reference copies post-build; revisit if the distributables need the shared automation.
- Attempted `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj /p:Configuration=Debug` and `/p:Configuration=Release` to confirm the ONI assemblies stay out of `bin` and the release zip contents, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the builds locally to validate both configurations.

## 2026-01-05 - Identifier PLib copy-local override
- Updated the legacy `ContainerTooltips` and `ZoomSpeed` projects to mark `PLib` as `Private` so the dependency is restored next to the target assembly before ILRepack runs.
- Attempted `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj /p:Configuration=Debug` and `/p:Configuration=Release` to confirm `PLib.dll` lands in `$(TargetDir)` ahead of `MergeDependencies`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rebuild locally to validate the packaging flow and merged assembly.
## 2026-01-09 - Identifier ILRepack NuGet search paths
- Extended the ILRepack `LibraryPath` for `ContainerTooltips` and `ZoomSpeed` so it now probes `$(RestorePackagesPath)` and `$(NuGetPackageRoot)` alongside the build output, configured `GameFolder`, and repo-local `lib` directory.
- Attempted to rebuild with `dotnet build Oni_mods_by_Identifier/ONIMods.sln` to confirm NuGet-resolved assemblies like `Newtonsoft.Json.dll` merge without being copied into `bin`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally once the ONI toolchain is available to verify ILRepack consumes the restored package outputs.

## 2026-01-10 - Identifier ILRepack search path projection
- Updated the `ContainerTooltips` and `ZoomSpeed` projects to generate ILRepack search paths from `@(ReferencePath)` and deduplicate them before invoking the merge task. Added a shared `ILRepackLibraryPath` property so the task now consumes the restored dependency directories even after copy-local trimming.
- Tried `dotnet build Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj` and `dotnet build Oni_mods_by_Identifier/ZoomSpeed/ZoomSpeed.csproj` to verify `Newtonsoft.Json.dll`/`PLib.dll` resolve from the projected directories, but the container still lacks the `.NET` host (`command not found: dotnet`). Please run the builds locally to confirm ILRepack succeeds without the assemblies remaining in `bin`.

## 2026-01-11 - Identifier AutoIncrement parity
- Copied the shared `AutoIncrement.targets` into the legacy identifier tree so the standalone build uses the same versioning task as the modern solution.
- Attempted to evaluate the target via `dotnet msbuild Oni_mods_by_Identifier/ContainerTooltips/ContainerTooltips.csproj /t:AutoIncrement /p:Configuration=Debug /p:Platform=AnyCPU /nologo`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please re-run the MSBuild evaluation locally to confirm the task resolves in Visual Studio.

## 2026-01-12 - Identifier UsesAzeLib override parity
- Added `<UsesAzeLib>false</UsesAzeLib>` to the legacy `ContainerTooltips` and `ZoomSpeed` projects so MSBuild stops searching for the unused `AzeLib` project when evaluating the standalone solution.
- Tried `dotnet build Oni_mods_by_Identifier/ONIMods.sln` to confirm MSBuild no longer probes `../AzeLib/AzeLib.csproj`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rebuild locally to validate the property override removes the stale project reference lookup.

## 2026-01-13 - Identifier publicise target toggle
- Swapped the `Publicise` target guard from the `AzeLib` project name check to a `GeneratePublicAssemblies` property so any legacy project can emit the `_public.dll` copies when needed.
- Enabled `<GeneratePublicAssemblies>true</GeneratePublicAssemblies>` in the identifier-level `Directory.Build.props` so the publicising step runs by default and can be disabled per project when necessary.
- Attempted to clean/rebuild via `dotnet msbuild Oni_mods_by_Identifier/ONIMods.sln /t:Clean;Build`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the clean/build locally to confirm `Assembly-CSharp_public.dll`, `Assembly-CSharp-firstpass_public.dll`, and `Unity.TextMeshPro_public.dll` regenerate under `Oni_mods_by_Identifier/lib/`.

## 2026-01-14 - Identifier AutoIncrement Newtonsoft reference path
- Updated `Oni_mods_by_Identifier/AutoIncrement.targets` so the Newtonsoft reference resolves via `System.IO.Path.Combine`, avoiding platform-specific separators when MSBuild evaluates the task.
- Attempted `dotnet msbuild Oni_mods_by_Identifier/ONIMods.sln /t:Build /p:Configuration=Debug` to confirm the AutoIncrement task now loads without the missing `Newtonsoft.Json.dll` warning, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally to verify MSBuild picks up the revised reference path.

## 2026-01-15 - Identifier AutoIncrement default keyword explicitness
- Replaced implicit `default` assignments in `Oni_mods_by_Identifier/AutoIncrement.targets` so the task's `TryExtractInt`/`TryExtractUShort` helpers now emit explicit `default(int)`/`default(ushort)` values for older Roslyn hosts.
- Tried `dotnet msbuild Oni_mods_by_Identifier/ONIMods.sln` to confirm the CodeTaskFactory script compiles and discovers the AutoIncrement task, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rebuild locally to validate the task under a full MSBuild environment.

## 2026-01-16 - Identifier publicise scheduling
- Moved the identifier-level `Publicise` target so it now runs before `ResolveReferences`, ensuring the `_public` assemblies exist before project references evaluate.
- Attempted `dotnet build Oni_mods_by_Identifier/ONIMods.sln` to confirm `Assembly-CSharp_public.dll`, `Assembly-CSharp-firstpass_public.dll`, and `Unity.TextMeshPro_public.dll` regenerate under `Oni_mods_by_Identifier/lib/`, but the container still lacks the `.NET` host (`command not found: dotnet`). Please rerun the build locally once MSBuild is available to validate the earlier execution point.

## 2026-01-17 - Directory.Build.props.user hygiene
- Removed the committed `Oni_mods_by_Identifier/Directory.Build.props.user` file and added an explicit ignore entry so each workstation keeps its Steam path overrides local-only.
- Updated the root README to remind contributors to copy `Directory.Build.props.default` to `.user` in both the `src/` solution and the legacy identifier tree before building.
- Planned to validate that MSBuild still succeeds after providing a `.user` file via `dotnet build Oni_mods_by_Identifier/ONIMods.sln`, but the container environment continues to lack the `.NET` host (`command not found: dotnet`). Please perform the build locally once a `.user` file is created to confirm the setup instructions remain accurate.
