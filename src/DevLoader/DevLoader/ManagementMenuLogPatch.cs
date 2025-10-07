using HarmonyLib;
using UnityEngine;

namespace DevLoader;

[HarmonyPatch(typeof(ManagementMenu), "OnPrefabInit")]
public static class ManagementMenuLogPatch
{
	private static void Prefix()
	{
		Debug.Log((object)("[DevLoader] ManagementMenu.OnPrefabInit(PREFIX) → DEV=" + (Config.Enabled ? "ON" : "OFF") + " (antes de CodexCacheInit)"));
	}

	private static void Postfix()
	{
		Debug.Log((object)("[DevLoader] ManagementMenu.OnPrefabInit(POSTFIX) → DEV=" + (Config.Enabled ? "ON" : "OFF") + " (después de CodexCacheInit)"));
	}
}
