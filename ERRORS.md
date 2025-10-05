# Error Log

## 2025-11-07 - BetterInfoCards converter null guard
- **Module:** BetterInfoCards hover info replay
- **Issue:** Converters that returned `null` produced draw actions without a valid `TextInfo`, causing crashes when the hover drawer replay attempted to dereference the missing converter output.
- **Resolution:** Guarded `TextInfo.Create` and the replay draw call so null converters log a warning and skip rendering instead of invoking `HoverTextDrawer.DrawText` with invalid data.
- **Status:** Fixed
