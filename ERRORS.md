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
