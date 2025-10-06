using HarmonyLib;

namespace ContainerTooltips;

[HarmonyPatch(typeof(Db), "Initialize")]
public static class Db_Initialize_Patch
{
	public static void Postfix()
	{
		Debug.Log((object)"[ContainerTooltips]: Db.Initialize postfix running. Calling UserMod.InitializeStatusItem()");
		UserMod.InitializeStatusItem();
	}
}
