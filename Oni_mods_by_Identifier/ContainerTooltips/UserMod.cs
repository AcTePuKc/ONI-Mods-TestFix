using System;
using System.Collections.Generic;
using HarmonyLib;
using Klei;
using Klei.AI;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace ContainerTooltips
{
    public class UserMod : UserMod2
    {
        private readonly struct SummaryCacheEntry
        {
            internal SummaryCacheEntry(float tick, string result)
            {
                Tick = tick;
                Result = result;
            }

            internal float Tick { get; }

            internal string Result { get; }
        }

        public const string ModStringsPrefix = "CONTAINERTOOLTIPS";
        public const string StatusItemId = "CONTAINERTOOLTIPSTATUSITEM";

        public const string ComposedPrefix = $"STRINGS.{ModStringsPrefix}.STATUSITEMS.{StatusItemId}";
        public const string NameStringKey = ComposedPrefix + ".NAME";
        public const string TooltipStringKey = ComposedPrefix + ".TOOLTIP";
        public const string EmptyStringKey = ComposedPrefix + ".EMPTY";

        public static StatusItem? ContentsStatusItem;

        private static readonly Dictionary<int, SummaryCacheEntry> StatusTextCache = new();
        private static readonly Dictionary<int, SummaryCacheEntry> TooltipTextCache = new();

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(Options));
        }

        public static void InitializeStatusItem()
        {
            if (ContentsStatusItem != null)
            {
                Debug.Log("[ContainerTooltips]: ContentsStatusItem already initialized, skipping creation");
                return;
            }

            RegisterString(NameStringKey, global::STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.NAME);
            RegisterString(TooltipStringKey, global::STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.TOOLTIP);
            RegisterString(EmptyStringKey, global::STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.EMPTY);

            var statusItem = new StatusItem(
                StatusItemId,
                ModStringsPrefix,
                "status_item_info",
                StatusItem.IconType.Info,
                NotificationType.Neutral,
                false,
                OverlayModes.None.ID)
            {
                resolveStringCallback = ResolveStatusText,
                resolveTooltipCallback = ResolveTooltipText
            };

            ContentsStatusItem = statusItem;

            Debug.Log("[ContainerTooltips]: ContentsStatusItem created and callbacks assigned");
        }

        internal static void ClearSummaryCache()
        {
            StatusTextCache.Clear();
            TooltipTextCache.Clear();
        }

        internal static void InvalidateCache(Storage storage)
        {
            if (storage == null)
            {
                return;
            }

            var instanceId = storage.GetInstanceID();
            StatusTextCache.Remove(instanceId);
            TooltipTextCache.Remove(instanceId);
        }

        private static string ResolveStatusText(string _, object data)
        {
            return GenerateContainerTooltip(data, Options.Instance.StatusLineLimit, StatusTextCache);
        }

        private static string ResolveTooltipText(string _, object data)
        {
            return GenerateContainerTooltip(data, Options.Instance.TooltipLineLimit, TooltipTextCache);
        }

        private static string GenerateContainerTooltip(object? data, int lineLimit, Dictionary<int, SummaryCacheEntry> cache)
        {
            if (data is not Storage storage)
            {
                Debug.LogWarning("[ContainerTooltips]: GenerateContainerTooltip received non-storage data");
                return string.Empty;
            }

            var tick = GameClock.Instance?.GetTime() ?? float.NaN;
            var instanceId = storage.GetInstanceID();

            if (cache.TryGetValue(instanceId, out var cached) && IsSameTick(cached.Tick, tick))
            {
                return cached.Result;
            }

            var summary = StorageContentsSummarizer.SummarizeStorageContents(storage, lineLimit);
            var header = GetStringWithFallback(NameStringKey, global::STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.NAME);
            var empty = GetStringWithFallback(EmptyStringKey, global::STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.EMPTY);
            var result = string.Concat(header, ": ", string.IsNullOrEmpty(summary) ? empty : summary);
            cache[instanceId] = new SummaryCacheEntry(tick, result);
            return result;
        }

        private static bool IsSameTick(float left, float right)
        {
            return !float.IsNaN(left) && !float.IsNaN(right) && Mathf.Approximately(left, right);
        }

        private static string GetStringWithFallback(string key, LocString fallback)
        {
            if (Strings.TryGet(key, out var entry))
            {
                var value = entry.String;

                if (!string.IsNullOrEmpty(value) && !value.StartsWith("MISSING", StringComparison.Ordinal))
                {
                    return value;
                }
            }

            return GetLocStringText(fallback);
        }

        private static string GetLocStringText(LocString value)
        {
            var englishText = value.text ?? string.Empty;

            if (string.IsNullOrEmpty(englishText))
            {
                englishText = value.ToString();
            }

            return englishText;
        }

        private static void RegisterString(string key, LocString value)
        {
            Strings.Add(new[] { key, GetLocStringText(value) });
        }
    }
}
