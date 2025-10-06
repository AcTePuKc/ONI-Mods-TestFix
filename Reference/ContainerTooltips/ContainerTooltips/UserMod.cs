using System;
using System.Collections.Generic;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace ContainerTooltips;

public class UserMod : UserMod2
{
	private struct SummaryCacheEntry
	{
		public float Tick;

		public string Result;
	}

	public const string ModStringsPrefix = "CONTAINERTOOLTIPS";

	public const string StatusItemId = "CONTAINERTOOLTIPSTATUSITEM";

	public const string ComposedPrefix = "STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM";

	public const string NameStringKey = "STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.NAME";

	public const string TooltipStringKey = "STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.TOOLTIP";

	public const string EmptyStringKey = "STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.EMPTY";

	public static StatusItem? ContentsStatusItem;

	private static readonly Dictionary<int, SummaryCacheEntry> statusTextCache = new Dictionary<int, SummaryCacheEntry>();

	private static readonly Dictionary<int, SummaryCacheEntry> tooltipTextCache = new Dictionary<int, SummaryCacheEntry>();

	public override void OnLoad(Harmony harmony)
	{
		((UserMod2)this).OnLoad(harmony);
		PUtil.InitLibrary();
		new POptions().RegisterOptions((UserMod2)(object)this, typeof(Options));
	}

	public static void InitializeStatusItem()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		if (ContentsStatusItem != null)
		{
			Debug.Log((object)"[ContainerTooltips]: ContentsStatusItem already initialized, skipping creation");
			return;
		}
		Strings.Add(new string[2] { "STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.NAME", "Contents" });
		Strings.Add(new string[2] { "STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.TOOLTIP", "Shows the items in internal storage." });
		Strings.Add(new string[2] { "STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.EMPTY", "None" });
		ContentsStatusItem = new StatusItem("CONTAINERTOOLTIPSTATUSITEM", "CONTAINERTOOLTIPS", "status_item_info", (IconType)0, (NotificationType)4, false, None.ID, true, 129022, (Func<string, object, string>)null)
		{
			resolveStringCallback = ResolveStatusText,
			resolveTooltipCallback = ResolveTooltipText
		};
		Debug.Log((object)"[ContainerTooltips]: ContentsStatusItem created and callbacks assigned");
	}

	private static string ResolveStatusText(string _, object data)
	{
		GameClock instance = GameClock.Instance;
		float num = ((instance != null) ? instance.GetTime() : float.NaN);
		Storage val = (Storage)((data is Storage) ? data : null);
		if (val == null)
		{
			Debug.LogWarning((object)"[ContainerTooltips]: ResolveStatusText received non-storage data");
			return string.Empty;
		}
		int instanceID = ((Object)val).GetInstanceID();
		if (statusTextCache.TryGetValue(instanceID, out var value) && value.Tick == num)
		{
			return value.Result;
		}
		string text = StorageContentsSummarizer.SummarizeStorageContents(val, SingletonOptions<Options>.Instance.StatusLineLimit);
		string result = StringEntry.op_Implicit(Strings.Get("STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.NAME")) + ": " + (string.IsNullOrEmpty(text) ? StringEntry.op_Implicit(Strings.Get("STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.EMPTY")) : text);
		statusTextCache[instanceID] = new SummaryCacheEntry
		{
			Tick = num,
			Result = result
		};
		return result;
	}

	private static string ResolveTooltipText(string _, object data)
	{
		GameClock instance = GameClock.Instance;
		float num = ((instance != null) ? instance.GetTime() : float.NaN);
		Storage val = (Storage)((data is Storage) ? data : null);
		if (val == null)
		{
			Debug.LogWarning((object)"[ContainerTooltips]: ResolveTooltipText received non-storage data");
			return string.Empty;
		}
		int instanceID = ((Object)val).GetInstanceID();
		if (tooltipTextCache.TryGetValue(instanceID, out var value) && value.Tick == num)
		{
			return value.Result;
		}
		string text = StorageContentsSummarizer.SummarizeStorageContents(val, SingletonOptions<Options>.Instance.TooltipLineLimit);
		string result = StringEntry.op_Implicit(Strings.Get("STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.NAME")) + ": " + (string.IsNullOrEmpty(text) ? StringEntry.op_Implicit(Strings.Get("STRINGS.CONTAINERTOOLTIPS.STATUSITEMS.CONTAINERTOOLTIPSTATUSITEM.EMPTY")) : text);
		tooltipTextCache[instanceID] = new SummaryCacheEntry
		{
			Tick = num,
			Result = result
		};
		return result;
	}
}
