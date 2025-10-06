using BadMod.ContainerTooltips.Components;
using HarmonyLib;
using UnityEngine;

namespace BadMod.ContainerTooltips.Patches;

[HarmonyPatch(typeof(Storage), nameof(Storage.OnSpawn))]
public static class StorageOnSpawnPatch
{
    private static void Postfix(Storage __instance)
    {
        if (__instance == null)
        {
            Debug.LogWarning("[ContainerTooltips]: Storage_OnSpawn instance invalid");
            return;
        }

        __instance.gameObject.AddOrGet<StorageContentsBehaviour>();
    }
}
