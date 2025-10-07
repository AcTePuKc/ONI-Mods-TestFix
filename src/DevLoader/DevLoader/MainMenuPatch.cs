using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DevLoader;

[HarmonyPatch(typeof(MainMenu), "OnPrefabInit")]
public static class MainMenuPatch
{
	private static void Postfix(MainMenu __instance)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		VerticalLayoutGroup[] componentsInChildren = ((Component)__instance).GetComponentsInChildren<VerticalLayoutGroup>(true);
		Transform val = null;
		VerticalLayoutGroup[] array = componentsInChildren;
		foreach (VerticalLayoutGroup val2 in array)
		{
			KButton[] componentsInChildren2 = ((Component)val2).GetComponentsInChildren<KButton>(true);
			RectTransform component = ((Component)val2).GetComponent<RectTransform>();
                        if (componentsInChildren2 != null && componentsInChildren2.Length >= 5 && (Object)component != null && component.anchorMin.x <= 0.05f && component.anchorMax.x <= 0.4f)
			{
				val = ((Component)val2).transform;
				break;
			}
		}
                if ((Object)val == null)
		{
			VerticalLayoutGroup obj = componentsInChildren.FirstOrDefault((VerticalLayoutGroup vg) => ((Component)vg).GetComponentsInChildren<KButton>(true).Length >= 5);
			val = ((obj != null) ? ((Component)obj).transform : null);
		}
                if ((Object)val == null)
		{
			return;
		}
		UI.AttachBadge(val);
		val.GetChild(val.childCount - 1).SetAsLastSibling();
		try
		{
                        if ((Object)GameObject.Find("DevMiniBootstrap") == null)
			{
				GameObject val3 = new GameObject("DevMiniBootstrap");
				val3.AddComponent<DevMiniBootstrap>();
				Debug.Log((object)"[DevLoader][MiniCenter] Bootstrap creado desde MainMenu");
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[DevLoader][MiniCenter] Crear bootstrap ERROR: " + ex));
		}
	}
}
