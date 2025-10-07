using System;
using HarmonyLib;
using KMod;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevLoader;

public class Mod : UserMod2
{
	private static GameObject s_hotkeysGO;

	public override void OnLoad(Harmony harmony)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		try
		{
			new PatchClassProcessor(harmony, typeof(MainMenuPatch)).Patch();
			new PatchClassProcessor(harmony, typeof(LoaderFilterPatch)).Patch();
			Debug.Log((object)"[DevLoader] Parches aplicados (UI+Filtro).");
			if (Config.Enabled)
			{
				Debug.Log((object)"[DevLoader] Estado ON al arrancar → forzando LoadAll()");
				LiveLoader.LoadAll();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[DevLoader] Error al aplicar parches: " + ex));
		}
		try
		{
                        if ((Object)s_hotkeysGO == null)
			{
				s_hotkeysGO = new GameObject("DevLoaderHotkeys");
                                Object.DontDestroyOnLoad((Object)s_hotkeysGO);
				s_hotkeysGO.AddComponent<Hotkeys>();
				Debug.Log((object)"[DevLoader] Hotkeys listo (Ctrl+F1 / Ctrl+1).");
			}
		}
		catch (Exception ex2)
		{
			Debug.LogWarning((object)("[DevLoader] No se pudo crear Hotkeys: " + ex2));
		}
	}
}
