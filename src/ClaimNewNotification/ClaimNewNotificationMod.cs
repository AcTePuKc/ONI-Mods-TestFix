using AzeLib;
using HarmonyLib;
using Klei;
using Klei.AI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ClaimNewNotification
{
    /// <summary>
    /// Boots the Claim New Notification mod via AzeLib's load hooks.
    /// </summary>
    internal static class ClaimNewNotificationBootstrap
    {
        /// <summary>
        /// Configures non-Harmony services once runtime wiring is ready.
        /// </summary>
        [AzeLib.Attributes.OnLoad]
        public static void OnLoad()
        {
            ClaimState.Initialize();
        }

        /// <summary>
        /// Wires Harmony patches when the implementation lands.
        /// </summary>
        /// <param name="harmony">Harmony instance provided by AzeLib's bootstrapper.</param>
        [AzeLib.Attributes.OnLoad]
        public static void OnLoad(Harmony harmony)
        {
            if (harmony is null)
                throw new ArgumentNullException(nameof(harmony));

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        internal static readonly Action<object, object> OnClosetButtonRefreshedDispatcher = (data, context) =>
            (context as ClaimState)?.OnClosetButtonRefreshed(data);

        internal static readonly Action<object, object> OnClaimCompletedDispatcher = (data, context) =>
            (context as ClaimState)?.OnClaimCompleted(data);

        internal static readonly Action<object, object> OnDatabaseReloadedDispatcher = (data, context) =>
            (context as ClaimState)?.OnDatabaseReloaded();
    }

    /// <summary>
    /// Tracks Supply Closet claim state, persistence, and UI affordances.
    /// </summary>
    internal sealed class ClaimState : IDisposable
    {
        private const string ButtonBadgeName = "ClaimNewNotificationBadge";
        private const string ItemBadgeName = "ClaimNewNotificationItemBadge";
        private const string PersistenceFileName = "seen.json";
        private const int PersistenceVersion = 1;

        private static ClaimState instance;
        private static readonly object InstanceLock = new();

        private readonly object syncRoot = new();
        private readonly Dictionary<string, UnseenBlueprint> unseen;
        private readonly Dictionary<string, int> closetSnapshot;
        private readonly string persistencePath;
        private readonly MethodInfo getItemDropsMethod;
        private readonly MethodInfo hasItemsToShowMethod;

        private int closetButtonHandle;
        private int claimCompletedHandle;
        private int databaseReloadedHandle;

        private Image buttonBadge;

        private ClaimState()
        {
            unseen = new Dictionary<string, UnseenBlueprint>(StringComparer.OrdinalIgnoreCase);
            closetSnapshot = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            persistencePath = BuildPersistencePath();

            getItemDropsMethod = AccessTools.Method(typeof(KleiItemDropScreen), "GetItemDrops");
            hasItemsToShowMethod = AccessTools.Method(typeof(KleiItemDropScreen), "HasItemsToShow");

            Load();
        }

        /// <summary>
        /// Singleton accessor invoked by the bootstrapper.
        /// </summary>
        public static ClaimState Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    return instance ??= new ClaimState();
                }
            }
        }

        /// <summary>
        /// Instantiates the singleton and registers for game events.
        /// </summary>
        public static void Initialize()
        {
            var state = Instance;
            state.RegisterEventHooks();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (syncRoot)
            {
                if (Game.Instance != null)
                {
                    Game.Instance.Unsubscribe(ref closetButtonHandle);
                    Game.Instance.Unsubscribe(ref claimCompletedHandle);
                    Game.Instance.Unsubscribe(ref databaseReloadedHandle);
                }
            }
        }

        internal void OnClosetButtonRefreshed(object data)
        {
            try
            {
                CaptureClosetSnapshot();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to capture Supply Closet snapshot: {ex}");
            }
        }

        internal void OnClaimCompleted(object _)
        {
            try
            {
                ProcessClaimDelta();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to process claim completion: {ex}");
            }
        }

        internal void OnDatabaseReloaded()
        {
            try
            {
                Load();
                RefreshBadge();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to reload persistence state: {ex}");
            }
        }

        internal Dictionary<string, int> BeginClaim()
        {
            try
            {
                return BuildInventorySnapshot();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to snapshot Supply Closet before claim: {ex}");
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }
        }

        internal void CompleteClaim(Dictionary<string, int> preClaimSnapshot)
        {
            if (preClaimSnapshot == null)
                return;

            try
            {
                ProcessClaimDelta(preClaimSnapshot);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to reconcile claim delta: {ex}");
            }
        }

        internal void OnClosetActivated()
        {
            try
            {
                var seenIds = BuildInventorySnapshot().Keys.ToList();
                if (seenIds.Count == 0)
                    return;

                lock (syncRoot)
                {
                    var removed = false;
                    foreach (var id in seenIds)
                        removed |= unseen.Remove(id);

                    if (removed)
                        Save();
                }

                RefreshBadge();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to acknowledge Supply Closet contents: {ex}");
            }
        }

        internal void RefreshButtonBadge(TopLeftControlScreen screen)
        {
            if (screen == null)
                return;

            try
            {
                var button = screen.kleiItemDropButton;
                if (button == null)
                    return;

                buttonBadge ??= EnsureBadge(button.transform);
                if (buttonBadge == null)
                    return;

                lock (syncRoot)
                {
                    buttonBadge.gameObject.SetActive(unseen.Count > 0);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to refresh Supply Closet button badge: {ex}");
            }
        }

        internal void ApplyItemBadge(KleiItemDropVisuals visuals, object drop)
        {
            if (visuals == null || drop == null)
                return;

            try
            {
                var id = GetDropId(drop);
                if (string.IsNullOrWhiteSpace(id))
                    return;

                var badge = visuals.transform.Find(ItemBadgeName)?.GetComponent<Image>() ?? CreateItemBadge(visuals.transform);
                if (badge == null)
                    return;

                lock (syncRoot)
                {
                    badge.gameObject.SetActive(unseen.ContainsKey(id));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to refresh item badge: {ex}");
            }
        }

        private void RegisterEventHooks()
        {
            if (Game.Instance == null)
                return;

            lock (syncRoot)
            {
                Game.Instance.Unsubscribe(ref closetButtonHandle);
                Game.Instance.Unsubscribe(ref claimCompletedHandle);
                Game.Instance.Unsubscribe(ref databaseReloadedHandle);

                closetButtonHandle = Game.Instance.Subscribe((int)GameHashes.RefreshUserInterface, ClaimNewNotificationBootstrap.OnClosetButtonRefreshedDispatcher, this);
                claimCompletedHandle = Game.Instance.Subscribe((int)GameHashes.NewItemUnlocked, ClaimNewNotificationBootstrap.OnClaimCompletedDispatcher, this);
                databaseReloadedHandle = Game.Instance.Subscribe((int)GameHashes.DatabaseReloaded, ClaimNewNotificationBootstrap.OnDatabaseReloadedDispatcher, this);
            }
        }

        private void RefreshBadge()
        {
            var screen = UnityEngine.Object.FindObjectOfType<TopLeftControlScreen>(true);
            if (screen != null)
                RefreshButtonBadge(screen);
        }

        private void ProcessClaimDelta(Dictionary<string, int> preClaimSnapshot = null)
        {
            Dictionary<string, int> previous;
            if (preClaimSnapshot != null)
                previous = new Dictionary<string, int>(preClaimSnapshot, StringComparer.OrdinalIgnoreCase);
            else
            {
                lock (syncRoot)
                    previous = new Dictionary<string, int>(closetSnapshot, StringComparer.OrdinalIgnoreCase);
            }

            var current = BuildInventorySnapshot();
            var claimed = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            lock (syncRoot)
            {
                foreach (var pair in previous)
                {
                    var before = pair.Value;
                    current.TryGetValue(pair.Key, out var after);
                    var delta = before - after;
                    if (delta <= 0)
                        continue;

                    if (!unseen.TryGetValue(pair.Key, out var entry))
                        entry = new UnseenBlueprint();

                    entry.Quantity += delta;
                    entry.LastClaimedUtc = DateTime.UtcNow;
                    unseen[pair.Key] = entry;
                    claimed[pair.Key] = delta;
                }

                closetSnapshot.Clear();
                foreach (var pair in current)
                    closetSnapshot[pair.Key] = pair.Value;

                if (claimed.Count > 0)
                    Save();
            }

            if (claimed.Count > 0)
            {
                RefreshBadge();
                EmitToast(claimed);
            }
        }

        private void EmitToast(Dictionary<string, int> claimed)
        {
            if (claimed == null || claimed.Count == 0)
                return;

            var total = claimed.Values.Sum();
            var title = Strings.UI.TOASTS.CLAIMNEW.TITLE.ToString();
            string body;
            if (claimed.Count == 1)
            {
                var pair = claimed.First();
                body = string.Format(CultureInfo.CurrentCulture, Strings.UI.TOASTS.CLAIMNEW.SINGLE_BODY.ToString(), FormatBlueprintName(pair.Key), pair.Value);
            }
            else
            {
                body = string.Format(CultureInfo.CurrentCulture, Strings.UI.TOASTS.CLAIMNEW.MULTI_BODY.ToString(), total);
            }

            ToastManager.InstantiateToast(title, body);
        }

        private static string FormatBlueprintName(string id) => string.IsNullOrWhiteSpace(id)
            ? global::STRINGS.UI.SUPPLYCLOSET.NAME.ToString()
            : id;

        private void CaptureClosetSnapshot()
        {
            if (hasItemsToShowMethod == null)
                return;

            if (!(hasItemsToShowMethod.Invoke(null, Array.Empty<object>()) is bool hasItems && hasItems))
                return;

            var snapshot = BuildInventorySnapshot();
            lock (syncRoot)
            {
                closetSnapshot.Clear();
                foreach (var pair in snapshot)
                    closetSnapshot[pair.Key] = pair.Value;
            }
        }

        private Dictionary<string, int> BuildInventorySnapshot()
        {
            var snapshot = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (getItemDropsMethod == null)
                return snapshot;

            if (getItemDropsMethod.Invoke(null, Array.Empty<object>()) is not IEnumerable drops)
                return snapshot;

            foreach (var drop in drops)
            {
                var id = GetDropId(drop);
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                var quantity = GetDropQuantity(drop);
                if (quantity <= 0)
                    continue;

                snapshot[id] = quantity;
            }

            return snapshot;
        }

        private static string GetDropId(object drop)
        {
            if (drop == null)
                return string.Empty;

            var traverse = Traverse.Create(drop);
            return traverse.Property<string>("id")?.Value
                ?? traverse.Property<string>("Id")?.Value
                ?? traverse.Field<string>("id")?.Value
                ?? traverse.Field<string>("Id")?.Value
                ?? string.Empty;
        }

        private static int GetDropQuantity(object drop)
        {
            if (drop == null)
                return 0;

            var traverse = Traverse.Create(drop);
            return traverse.Property<int>("quantity")?.Value
                ?? traverse.Property<int>("Quantity")?.Value
                ?? traverse.Field<int>("quantity")?.Value
                ?? traverse.Field<int>("Quantity")?.Value;
        }

        private void Load()
        {
            lock (syncRoot)
            {
                unseen.Clear();

                try
                {
                    if (!File.Exists(persistencePath))
                        return;

                    var json = File.ReadAllText(persistencePath);
                    if (string.IsNullOrWhiteSpace(json))
                        return;

                    var payload = JsonConvert.DeserializeObject<PersistencePayload>(json);
                    if (payload?.Version != PersistenceVersion || payload.Unseen == null)
                        return;

                    foreach (var pair in payload.Unseen)
                    {
                        if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null)
                            continue;

                        unseen[pair.Key] = pair.Value;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to load persistence state: {ex}");
                }
            }
        }

        private void Save()
        {
            lock (syncRoot)
            {
                try
                {
                    var directory = Path.GetDirectoryName(persistencePath);
                    if (!string.IsNullOrWhiteSpace(directory))
                        Directory.CreateDirectory(directory);
                    var payload = new PersistencePayload
                    {
                        Version = PersistenceVersion,
                        Unseen = unseen.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase),
                    };

                    var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
                    File.WriteAllText(persistencePath, json);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to persist Supply Closet state: {ex}");
                }
            }
        }

        private static string BuildPersistencePath()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrWhiteSpace(documents))
                documents = Environment.GetEnvironmentVariable("USERPROFILE") ?? string.Empty;

            var folder = Path.Combine(documents, "Klei", "OxygenNotIncluded", "mods", "Local", "AcTePuKc.ClaimNewNotification");
            return Path.Combine(folder, PersistenceFileName);
        }

        private static Image EnsureBadge(Transform parent)
        {
            if (parent == null)
                return null;

            var existing = parent.Find(ButtonBadgeName)?.GetComponent<Image>();
            if (existing != null)
                return existing;

            var sprite = ModAssets.GetNewBadgeSprite();
            if (sprite == null)
                return null;

            var go = new GameObject(ButtonBadgeName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.75f, 0.75f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            return image;
        }

        private static Image CreateItemBadge(Transform parent)
        {
            if (parent == null)
                return null;

            var sprite = ModAssets.GetNewBadgeSprite();
            if (sprite == null)
                return null;

            var go = new GameObject(ItemBadgeName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-8f, -8f);
            rect.sizeDelta = new Vector2(32f, 32f);

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            return image;
        }

        private sealed class PersistencePayload
        {
            [JsonProperty("version")]
            public int Version { get; set; }

            [JsonProperty("unseen")]
            public Dictionary<string, UnseenBlueprint> Unseen { get; set; }
        }

        private sealed class UnseenBlueprint
        {
            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("lastClaimed")]
            public DateTime LastClaimedUtc { get; set; }
        }
    }

    /// <summary>
    /// Loads embedded assets for the mod.
    /// </summary>
    internal static class ModAssets
    {
        private static Sprite newBadgeSprite;

        public static Sprite GetNewBadgeSprite()
        {
            if (newBadgeSprite != null)
                return newBadgeSprite;

            try
            {
                var assemblyLocation = Path.GetDirectoryName(typeof(ClaimNewNotificationBootstrap).Assembly.Location);
                if (string.IsNullOrWhiteSpace(assemblyLocation))
                    return null;

                var path = Path.Combine(assemblyLocation, "ModAssets", "New.png");
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Missing badge asset at {path}.");
                    return null;
                }

                var bytes = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!ImageConversion.LoadImage(texture, bytes))
                {
                    UnityEngine.Object.Destroy(texture);
                    Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Unable to decode badge asset.");
                    return null;
                }

                texture.filterMode = FilterMode.Bilinear;
                newBadgeSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(ClaimNewNotification)}] Failed to load badge sprite: {ex}");
            }

            return newBadgeSprite;
        }
    }

    /// <summary>
    /// Mod-localized strings registered through AzeLib.
    /// </summary>
    internal sealed class Strings : AStrings<Strings>
    {
        public static class UI
        {
            public static class TOASTS
            {
                public static class CLAIMNEW
                {
                    public static LocString TITLE = (LocString)"Blueprints Claimed";
                    public static LocString SINGLE_BODY = (LocString)"You received {1} new copy(ies) of {0}.";
                    public static LocString MULTI_BODY = (LocString)"You received {0} new blueprints.";
                }
            }
        }
    }

    [HarmonyPatch(typeof(TopLeftControlScreen), nameof(TopLeftControlScreen.RefreshKleiItemDropButton))]
    internal static class TopLeftControlScreenRefreshKleiItemDropButtonPatch
    {
        private static void Postfix(TopLeftControlScreen __instance)
        {
            ClaimState.Instance.RefreshButtonBadge(__instance);
        }
    }

    [HarmonyPatch(typeof(KleiItemDropScreen), nameof(KleiItemDropScreen.Claim))]
    internal static class KleiItemDropScreenClaimPatch
    {
        private static Dictionary<string, int> Prefix()
        {
            return ClaimState.Instance.BeginClaim();
        }

        private static void Postfix(Dictionary<string, int> __state)
        {
            ClaimState.Instance.CompleteClaim(__state);
        }
    }

    [HarmonyPatch(typeof(KleiItemDropScreen), nameof(KleiItemDropScreen.ClaimAll))]
    internal static class KleiItemDropScreenClaimAllPatch
    {
        private static Dictionary<string, int> Prefix()
        {
            return ClaimState.Instance.BeginClaim();
        }

        private static void Postfix(Dictionary<string, int> __state)
        {
            ClaimState.Instance.CompleteClaim(__state);
        }
    }

    [HarmonyPatch(typeof(KleiItemDropScreen), "OnActivate")]
    internal static class KleiItemDropScreenOnActivatePatch
    {
        private static void Postfix()
        {
            ClaimState.Instance.OnClosetActivated();
        }
    }

    [HarmonyPatch(typeof(KleiItemDropVisuals), nameof(KleiItemDropVisuals.Bind))]
    internal static class KleiItemDropVisualsBindPatch
    {
        private static void Postfix(KleiItemDropVisuals __instance, object itemDrop)
        {
            ClaimState.Instance.ApplyItemBadge(__instance, itemDrop);
        }
    }
}
