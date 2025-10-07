using System;
using System.Reflection;
using HarmonyLib;

namespace DevLoader;

[HarmonyPatch]
public static class LoaderFilterPatch
{
	private static MethodBase TargetMethod()
	{
		Type type = AccessTools.TypeByName("KMod.DLLLoader");
		if (type == null)
		{
			Debug.LogWarning((object)"[DevLoader] No encontré 'KMod.DLLLoader'");
			return null;
		}
		Type type2 = AccessTools.TypeByName("KMod.Mod");
		MethodInfo methodInfo = AccessTools.Method(type, "LoadDLLs", new Type[4]
		{
			type2,
			typeof(string),
			typeof(string),
			typeof(bool)
		}, (Type[])null);
		if (methodInfo != null)
		{
			return methodInfo;
		}
		methodInfo = AccessTools.FirstMethod(type, (Func<MethodInfo, bool>)((MethodInfo mi) => mi.Name == "LoadDLLs"));
		if (methodInfo == null)
		{
			Debug.LogWarning((object)"[DevLoader] No encontré método LoadDLLs");
		}
		return methodInfo;
	}

	private static bool Prefix(object ownerMod, string harmonyId, string path, bool isDev)
	{
		try
		{
			string p = (path ?? "").Replace('/', '\\');
			if (!LooksDev(p) && !isDev)
			{
				return true;
			}
			if (!Config.Enabled)
			{
				Debug.Log((object)("[DevLoader] (OFF) Bloqueando carga nativa para DEV: " + path));
				return false;
			}
			Debug.Log((object)("[DevLoader] (ON) Interceptando carga nativa DEV → shadow-load. Path=" + path));
			LiveLoader.LoadAll();
			return false;
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[DevLoader] Filtro DLLLoader error: " + ex));
		}
		return true;
	}

	private static bool LooksDev(string p)
	{
		string text = (p ?? "").ToLowerInvariant();
		return text.Contains("\\mods\\dev\\") || text.EndsWith("\\mods\\dev");
	}
}
