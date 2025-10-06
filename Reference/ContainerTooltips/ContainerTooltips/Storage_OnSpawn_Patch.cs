using HarmonyLib;
using UnityEngine;

namespace ContainerTooltips;

[HarmonyPatch(typeof(Storage), "OnSpawn")]
public static class Storage_OnSpawn_Patch
{
	private static void Postfix(Storage __instance)
	{
		if ((Object)(object)__instance == (Object)null)
		{
			Debug.LogWarning((object)"[ContainerTooltips]: Storage_OnSpawn instance invalid");
		}
		else
		{
			EntityTemplateExtensions.AddOrGet<StorageContentsBehaviour>(((Component)__instance).gameObject);
		}
	}
}
