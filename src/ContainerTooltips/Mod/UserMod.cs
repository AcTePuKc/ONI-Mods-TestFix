using System;
using System.Collections.Generic;
using BadMod.ContainerTooltips.Configuration;
using BadMod.ContainerTooltips.Storage;
using HarmonyLib;
using Klei.AI;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace BadMod.ContainerTooltips.Mod;

public sealed class UserMod : UserMod2
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

    public const string ComposedPrefix = "STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM";
    public const string NameStringKey = ComposedPrefix + ".NAME";
    public const string TooltipStringKey = ComposedPrefix + ".TOOLTIP";
    public const string EmptyStringKey = ComposedPrefix + ".EMPTY";

    public static StatusItem? ContentsStatusItem { get; private set; }

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
            return;

        Strings.Add(new[]
        {
            NameStringKey,
            global::STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.NAME
        });
        Strings.Add(new[]
        {
            TooltipStringKey,
            global::STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.TOOLTIP
        });
        Strings.Add(new[]
        {
            EmptyStringKey,
            global::STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.EMPTY
        });

        ContentsStatusItem = new StatusItem(
            StatusItemId,
            ModStringsPrefix,
            "status_item_info",
            IconType.Info,
            NotificationType.Neutral,
            allowMultiples: false,
            OverlayModes.None.ID)
        {
            resolveStringCallback = ResolveStatusText,
            resolveTooltipCallback = ResolveTooltipText
        };
    }

    internal static void ClearSummaryCache()
    {
        StatusTextCache.Clear();
        TooltipTextCache.Clear();
    }

    internal static void InvalidateCache(Storage storage)
    {
        if (storage == null)
            return;

        var id = storage.GetInstanceID();
        StatusTextCache.Remove(id);
        TooltipTextCache.Remove(id);
    }

    private static string ResolveStatusText(string _, object data)
    {
        if (data is not Storage storage)
        {
            Debug.LogWarning("[ContainerTooltips]: ResolveStatusText received non-storage data");
            return string.Empty;
        }

        var tick = GameClock.Instance?.GetTime() ?? float.NaN;
        var instanceId = storage.GetInstanceID();

        if (StatusTextCache.TryGetValue(instanceId, out var cached) && IsSameTick(cached.Tick, tick))
            return cached.Result;

        var summary = StorageContentsSummarizer.SummarizeStorageContents(storage, Options.Instance.StatusLineLimit);
        var header = Strings.Get(NameStringKey).String;
        var empty = Strings.Get(EmptyStringKey).String;
        var result = string.Concat(header, ": ", string.IsNullOrEmpty(summary) ? empty : summary);

        StatusTextCache[instanceId] = new SummaryCacheEntry(tick, result);
        return result;
    }

    private static string ResolveTooltipText(string _, object data)
    {
        if (data is not Storage storage)
        {
            Debug.LogWarning("[ContainerTooltips]: ResolveTooltipText received non-storage data");
            return string.Empty;
        }

        var tick = GameClock.Instance?.GetTime() ?? float.NaN;
        var instanceId = storage.GetInstanceID();

        if (TooltipTextCache.TryGetValue(instanceId, out var cached) && IsSameTick(cached.Tick, tick))
            return cached.Result;

        var summary = StorageContentsSummarizer.SummarizeStorageContents(storage, Options.Instance.TooltipLineLimit);
        var header = Strings.Get(NameStringKey).String;
        var empty = Strings.Get(EmptyStringKey).String;
        var result = string.Concat(header, ": ", string.IsNullOrEmpty(summary) ? empty : summary);

        TooltipTextCache[instanceId] = new SummaryCacheEntry(tick, result);
        return result;
    }

    private static bool IsSameTick(float left, float right)
    {
        return !float.IsNaN(left) && !float.IsNaN(right) && Mathf.Approximately(left, right);
    }
}
