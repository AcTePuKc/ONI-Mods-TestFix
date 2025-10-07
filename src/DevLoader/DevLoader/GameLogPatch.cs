using HarmonyLib;

namespace DevLoader;

[HarmonyPatch(typeof(Game), "OnSpawn")]
public static class GameLogPatch
{
	private static void Prefix()
	{
		Debug.Log((object)("[DevLoader] Game.OnSpawn(PREFIX) → DEV=" + (Config.Enabled ? "ON" : "OFF")));
	}

	private static void Postfix()
	{
		Debug.Log((object)("[DevLoader] Game.OnSpawn(POSTFIX) → DEV=" + (Config.Enabled ? "ON" : "OFF")));
	}
}
