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
