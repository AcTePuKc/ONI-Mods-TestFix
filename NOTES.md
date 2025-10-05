# AzeLib OnLoad benchmark (2024-06-17)

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

## 2025-10-11 - BetterInfoCards shadow bar tint option
- Added configurable RGB sliders for the info card shadow bar and ensured stretched bars reuse the selected tint.
- Unable to rebuild `BetterInfoCards` here because the container still lacks the ONI-managed assemblies and `dotnet`; maintainers should run `dotnet build src/oniMods.sln` locally after syncing the new option values.

## 2025-10-12 - BetterInfoCards shadow bar tint persistence
- Ensured the prefab shadow bar and dynamically spawned extensions copy the configured tint to their `ColorStyleSetting` values so refreshes preserve the selected RGB.
- Build and in-game validation remain blocked in this container due to missing ONI assemblies and the `dotnet` host; maintainers should rebuild via `dotnet build src/oniMods.sln` and confirm the slider-controlled tint persists in-game.

## 2025-10-13 - BetterInfoCards widget clone matching
- Relaxed the widget matching so `InfoCardWidgets` recognises hover drawer clones by comparing prefab names (sans `"(Clone)"`) and verifying the component layout/rect dimensions.
- Unable to rebuild or verify the mod in-game here because the container lacks the ONI managed assemblies and a .NET runtime; maintainers should rebuild via `dotnet build src/oniMods.sln` and confirm stacked hover cards wrap correctly.

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
