using HarmonyLib;
using UnityEngine;

namespace ContainerTooltips
{
    [HarmonyPatch(typeof(Storage), "OnSpawn")]
    public static class Storage_OnSpawn_Patch
    {
        private static void Postfix(Storage __instance)
        {
            // Debug.Log($"[ContainerTooltips]: Storage_OnSpawn called for {__instance?.name ?? "<null>"}");
            if (__instance == null)
            {
                Debug.LogWarning($"[ContainerTooltips]: Storage_OnSpawn instance invalid");
                return;
            }

            // Attach the behaviour so it can supply this storage component directly to the status item callbacks.
            __instance.gameObject.AddOrGet<StorageContentsBehaviour>();

            // Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour ensured on {__instance.name}");
        }
    }
}