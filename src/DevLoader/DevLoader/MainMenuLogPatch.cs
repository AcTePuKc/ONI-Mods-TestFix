using HarmonyLib;
using UnityEngine;

namespace DevLoader;

[HarmonyPatch(typeof(MainMenu), "OnPrefabInit")]
public static class MainMenuLogPatch
{
	private static void Prefix()
	{
		Debug.Log((object)("[DevLoader] MainMenu.OnPrefabInit(PREFIX) → estado DEV=" + (Config.Enabled ? "ON" : "OFF")));
	}

	private static void Postfix()
	{
		Debug.Log((object)("[DevLoader] MainMenu.OnPrefabInit(POSTFIX) → estado DEV=" + (Config.Enabled ? "ON" : "OFF")));
	}
}
