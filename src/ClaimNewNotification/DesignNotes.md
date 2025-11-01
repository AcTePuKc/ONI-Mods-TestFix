# Claim New Notification – Design Notes

## Blueprint claim flow instrumentation
- Reuse the `TopLeftControlScreen.RefreshKleiItemDropButton` Harmony postfix that ships with `ClaimNotification` to detect when the Supply Closet button transitions between the hidden, idle, and attention states.
- Capture the supply closet inventory snapshot by walking `KleiItemDropScreen.GetItemDrops()` when the button first surfaces claimable drops. Persist the item `id`/`quantity` pairs so the mod can compare them to the post-claim state.
- Patch the claim pathway (`KleiItemDropScreen.Claim` for single picks and `KleiItemDropScreen.ClaimAll`) so we can recalculate the closet inventory immediately after the base game removes the selected drops.
- Compute deltas between the pre-claim and post-claim inventories and queue any positive differences as "unseen" blueprint handles for toast generation and UI badges.

## Event dispatchers and handles
- Maintain dispatcher singletons for the modern event API:
  - `private static readonly Action<object, object> OnClosetButtonRefreshedDispatcher = (data, context) => ((ClaimState)context).OnClosetButtonRefreshed(data);`
  - `private static readonly Action<object, object> OnClaimCompletedDispatcher = (data, context) => ((ClaimState)context).OnClaimCompleted(data);`
  - `private static readonly Action<object, object> OnDatabaseReloadedDispatcher = (data, context) => ((ClaimState)context).OnDatabaseReloaded();`
- Store the returned handles on the owning state object:
  - `private int closetButtonHandle;`
  - `private int claimCompletedHandle;`
  - `private int databaseReloadedHandle;`
- Register with `Subscribe((int)GameHashes.RefreshUserInterface, OnClosetButtonRefreshedDispatcher, this);` and equivalent overloads that best match the runtime payloads once verified.
- Tear down the hooks in `OnCleanUp()` by calling `Game.Instance.Unsubscribe(ref closetButtonHandle);` (and for each handle) to follow the allocation-reduction guidance from `dev_log.md` and avoid stale closures.

## Persistence contract (`seen.json`)
- Store unseen state in `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\AcTePuKc.ClaimNewNotification\seen.json`.
- JSON schema:
  ```json
  {
    "version": 1,
    "unseen": {
      "BlueprintId": {
        "quantity": 2,
        "lastClaimed": "2026-01-19T04:00:00Z"
      }
    }
  }
  ```
- Loading rules:
  - On mod load, read the file if present and ignore malformed payloads by logging a warning and starting from an empty dictionary.
  - Merge claims by summing quantities and keeping the most recent `lastClaimed` timestamp per blueprint id.
  - Persist the file after any inventory delta is processed.
- Clearing logic:
  - When the Supply Closet UI is opened (patch `KleiItemDropScreen.OnActivate`), mark every displayed blueprint as seen and flush the cleared dictionary to disk.

## Harmony patch plan
- **Post-claim toast:** Patch `KleiItemDropScreen.Claim`/`ClaimAll` postfixes to emit localized `ToastManager.InstantiateToast()` calls summarizing how many unseen blueprints were queued.
- **Supply Closet button badge:** Extend the `TopLeftControlScreen.RefreshKleiItemDropButton` postfix to append a badge state if the unseen dictionary is non-empty. The badge should animate using the same refresh cadence as the base game's attention pulse and derive its string from `STRINGS.UI.SUPPLYCLOSET.NEW_ITEMS`.
- **Per-item NEW chip:** Patch the closet item row prefab binding (e.g., `KleiItemDropVisuals.Bind`) to overlay `ModAssets/New.png` when the corresponding blueprint id is marked unseen. Clear the chip once the closet acknowledges the item, mirroring the hook that `ClaimNotification` uses to prompt the initial dialog.

## Tooling follow-up
- Document progress and outstanding tasks in `NOTES.md` with the current date.
- Add TODO tracker rows mirroring the `ClaimNewNotificationMod.cs` TODO so the repository’s central checklist stays aligned.
- Rerun `python tools/oni_eventscan.py` after implementation work begins to refresh the shared `findings.json` entry for Supply Closet events and confirm the dispatcher targets remain accurate.
