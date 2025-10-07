using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DevLoader;

[HarmonyPatch]
public static class ModLoadFilterPatch
{
	private static MethodBase TargetMethod()
	{
		Type type = AccessTools.TypeByName("KMod.Mod");
		Type type2 = AccessTools.TypeByName("KMod.Content");
		if (type == null || type2 == null)
		{
			Debug.LogWarning((object)"[DevLoader] No encontré KMod.Mod/Content");
			return null;
		}
		MethodInfo methodInfo = AccessTools.Method(type, "Load", new Type[1] { type2 }, (Type[])null);
		if (methodInfo == null)
		{
			Debug.LogWarning((object)"[DevLoader] No encontré KMod.Mod.Load(Content)");
		}
		return methodInfo;
	}

	private static bool Prefix(object __instance, object content)
	{
		try
		{
			string text = TryGetModPath(__instance);
			if (!LooksDev(text))
			{
				return true;
			}
			if (!Config.Enabled)
			{
				Debug.Log((object)("[DevLoader] (OFF) Skipping Mod.Load para DEV: " + text));
				return false;
			}
			Debug.Log((object)("[DevLoader] (ON) Permitido Mod.Load para DEV: " + text));
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[DevLoader] Filtro Mod.Load error: " + ex));
		}
		return true;
	}

	private static string TryGetModPath(object mod)
	{
		if (mod == null)
		{
			return "";
		}
		Type type = mod.GetType();
		object obj = AccessTools.Property(type, "label")?.GetValue(mod, null) ?? AccessTools.Field(type, "label")?.GetValue(mod);
		if (obj != null)
		{
			Type type2 = obj.GetType();
			string text = (string)(AccessTools.Property(type2, "install_path")?.GetValue(obj, null) ?? AccessTools.Field(type2, "install_path")?.GetValue(obj) ?? AccessTools.Property(type2, "path")?.GetValue(obj, null) ?? AccessTools.Field(type2, "path")?.GetValue(obj));
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		string text2 = (string)AccessTools.Field(type, "root")?.GetValue(mod);
		return text2 ?? "";
	}

	private static bool LooksDev(string p)
	{
		p = (p ?? "").Replace('/', '\\').ToLowerInvariant();
		return p.Contains("\\mods\\dev\\") || p.EndsWith("\\mods\\dev");
	}
}
