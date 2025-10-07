# Error Log

## 2025-11-07 - BetterInfoCards converter null guard
- **Module:** BetterInfoCards hover info replay
- **Issue:** Converters that returned `null` produced draw actions without a valid `TextInfo`, causing crashes when the hover drawer replay attempted to dereference the missing converter output.
- **Resolution:** Guarded `TextInfo.Create` and the replay draw call so null converters log a warning and skip rendering instead of invoking `HoverTextDrawer.DrawText` with invalid data.
- **Status:** Fixed

## 2025-11-09 - BetterInfoCards title converter null guard
- **Module:** BetterInfoCards hover title aggregation
- **Issue:** Countable prefabs missing a `PrimaryElement` caused the title converter to dereference a null component while computing `Units`, crashing the hover drawer replay.
- **Resolution:** Cached the `PrimaryElement`, logged a one-shot warning when absent, and returned a safe default so the aggregation can continue without throwing.
- **Status:** Fixed

## 2025-11-10 - BetterInfoCards shadow bar prefab resolution
- **Module:** BetterInfoCards hover widget capture
- **Issue:** Shadow bar prefabs captured as components without a resolved `RectTransform` left `InfoCardWidgets.shadowBar` null, so exported cards reported zero width/height and never triggered column wrapping.
- **Resolution:** Added prefab-based rect discovery with deferred retries so shadow bars resolve after Unity finishes layout, restoring non-zero dimensions for captured cards.
- **Status:** Fixed

## 2025-11-12 - BetterInfoCards shadow bar instance resolution
- **Module:** BetterInfoCards hover widget capture
- **Issue:** Resolving `shadowBar` from prefab assets caused layout translations and width adjustments to mutate the prefab instead of the live hover card instance, so later draws inherited stale offsets and asset changes persisted across sessions.
- **Resolution:** Track pending widget entries and re-resolve the `RectTransform` from instantiated scene objects, falling back to hierarchy scans so deferred sizing now operates on the live hover card rather than the prefab asset.
- **Status:** Fixed

## 2025-11-13 - BetterInfoCards wrapper shadow bar prefabs
- **Module:** BetterInfoCards hover widget capture
- **Issue:** Skin shadow bars wrapped in helper objects exposed extra components and differing rect sizes, so `InfoCardWidgets` rejected the prefab match and never promoted the instantiated shadow bar, leaving card widths and heights at zero.
- **Resolution:** Relaxed the prefab comparison so wrappers that contain a component superset of the skin shadow bar still qualify, allowing the runtime `RectTransform` to be recovered from the entry hierarchy and restored to `shadowBar`.
- **Status:** Fixed

## 2025-11-15 - BetterInfoCards temperature converter guard
- **Module:** BetterInfoCards hover temperature aggregation
- **Issue:** Buildings without a `PrimaryElement` caused the temperature converter to dereference `null` while reading `Temperature`, crashing hover card rendering.
- **Resolution:** Cache the component lookup, emit a one-shot warning when it is missing, and return a safe default so aggregation proceeds without throwing.
- **Status:** Fixed

## 2025-11-23 - ContainerTooltips build imports
- **Module:** ContainerTooltips project configuration
- **Issue:** The FixedMod-specific `Directory.Build.props` and `.targets` imported `../src/...`, which resolved to the same directory and caused MSBuild to report a circular dependency when the solution loaded in Visual Studio.
- **Resolution:** Updated the relative paths to `../../src/...` so the FixedMod projects now import the shared root build settings without recursion.
- **Status:** Fixed

## 2025-11-29 - BetterInfoCards overlay tint bleed
- **Module:** BetterInfoCards hover card styling
- **Issue:** Overriding the `shadowBarWidget` tint at construction time recolored every `HoverTextDrawer` consumer, so world overlays adopted the info card background color.
- **Resolution:** Stop mutating the skin prefab and instead recolor the instantiated shadow bar for each BetterInfoCards hover card during replay.
- **Status:** Fixed

## 2025-12-09 - DevLoader decompiler artifacts (CS1525)
- **Module:** DevLoader bootstrap and badge UI wiring
- **Issue:** The DevLoader project would not compile because the decompiled sources still contained ILSpy ref-cast helpers (e.g., `((Scene)(ref scene)).name`, `((Rect)(ref rect)).width`) that generate CS1525 syntax errors under the Unity/.NET compiler.
- **Resolution:** Replaced the ref-cast helper calls with direct property access, reintroduced idiomatic `Vector2` construction for anchor settings, and cached the sprite rect once when configuring the badge layout.
- **Status:** Fixed
