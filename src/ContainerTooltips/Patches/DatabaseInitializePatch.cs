using BadMod.ContainerTooltips.Mod;
using HarmonyLib;
using UnityEngine;

namespace BadMod.ContainerTooltips.Patches;

[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
public static class DatabaseInitializePatch
{
    private static void Postfix()
    {
        Debug.Log("[ContainerTooltips]: Db.Initialize postfix running. Calling UserMod.InitializeStatusItem().");
        UserMod.InitializeStatusItem();
    }
}
