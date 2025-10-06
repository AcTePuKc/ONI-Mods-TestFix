# Container Tooltips Translations

This folder hosts `.po` files for the Container Tooltips mod. A ready-to-use `_template.pot` is checked in and includes entries for the status item name, tooltip, and empty-state text exposed by the mod.

To start a translation:

1. Copy `_template.pot` to a new file named after the locale (for example, `es.po`).
2. Translate the `msgid` values while keeping the `msgctxt` keys intact so the game can load the strings.

The English source strings live in `UserMod.InitializeStatusItem` and are also registered through the `STRINGS.CONTAINERTOOLTIPS` class so AzeLib’s POT generation (available in debug builds) will recreate the same keys automatically when new strings are added. If new strings are introduced, regenerate the template in a debug build or update `_template.pot` manually to keep it in sync.
