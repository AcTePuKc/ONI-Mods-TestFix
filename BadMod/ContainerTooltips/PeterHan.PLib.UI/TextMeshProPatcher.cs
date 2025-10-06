using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

internal static class TextMeshProPatcher
{
	private const string HARMONY_ID = "TextMeshProPatch";

	private static volatile bool patchChecked = false;

	private static readonly object patchLock = new object();

	private static bool AssignPositioningIfNeeded_Prefix(TMP_InputField __instance, RectTransform ___caretRectTrans, TMP_Text ___m_TextComponent)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		if ((Object)(object)___m_TextComponent != (Object)null && (Object)(object)___caretRectTrans != (Object)null && (Object)(object)__instance != (Object)null && ((Behaviour)___m_TextComponent).isActiveAndEnabled)
		{
			RectTransform rectTransform = ___m_TextComponent.rectTransform;
			if (((Transform)___caretRectTrans).localPosition != ((Transform)rectTransform).localPosition || ((Transform)___caretRectTrans).localRotation != ((Transform)rectTransform).localRotation || ((Transform)___caretRectTrans).localScale != ((Transform)rectTransform).localScale || ___caretRectTrans.anchorMin != rectTransform.anchorMin || ___caretRectTrans.anchorMax != rectTransform.anchorMax || ___caretRectTrans.anchoredPosition != rectTransform.anchoredPosition || ___caretRectTrans.sizeDelta != rectTransform.sizeDelta || ___caretRectTrans.pivot != rectTransform.pivot)
			{
				((MonoBehaviour)__instance).StartCoroutine(ResizeCaret(___caretRectTrans, rectTransform));
				result = false;
			}
		}
		return result;
	}

	private static bool HasOurPatch(IEnumerable<Patch> patchList)
	{
		bool result = false;
		if (patchList != null)
		{
			foreach (Patch patch in patchList)
			{
				string text = patch.PatchMethod?.DeclaringType?.Name;
				if (text == "TextMeshProPatcher" || text == "PLibPatches")
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private static void InputFieldPatches(Type tmpType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		Harmony val = new Harmony("TextMeshProPatch");
		MethodInfo methodSafe = tmpType.GetMethodSafe("AssignPositioningIfNeeded", isStatic: false, PPatchTools.AnyArguments);
		if (methodSafe != null && !HasOurPatch(Harmony.GetPatchInfo((MethodBase)methodSafe)?.Prefixes))
		{
			val.Patch((MethodBase)methodSafe, new HarmonyMethod(typeof(TextMeshProPatcher), "AssignPositioningIfNeeded_Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}
		MethodInfo methodSafe2 = tmpType.GetMethodSafe("OnEnable", isStatic: false, PPatchTools.AnyArguments);
		if (methodSafe2 != null && !HasOurPatch(Harmony.GetPatchInfo((MethodBase)methodSafe2)?.Postfixes))
		{
			val.Patch((MethodBase)methodSafe2, (HarmonyMethod)null, new HarmonyMethod(typeof(TextMeshProPatcher), "OnEnable_Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
		}
	}

	private static void OnEnable_Postfix(Scrollbar ___m_VerticalScrollbar, TMP_Text ___m_TextComponent)
	{
		if ((Object)(object)___m_TextComponent != (Object)null)
		{
			___m_TextComponent.ignoreRectMaskCulling = (Object)(object)___m_VerticalScrollbar != (Object)null;
		}
	}

	public static void Patch()
	{
		lock (patchLock)
		{
			if (patchChecked)
			{
				return;
			}
			Type typeSafe = PPatchTools.GetTypeSafe("TMPro.TMP_InputField");
			if (typeSafe != null)
			{
				try
				{
					InputFieldPatches(typeSafe);
				}
				catch (Exception)
				{
					PUtil.LogWarning("Unable to patch TextMeshPro bug, text fields may display improperly inside scroll areas");
				}
			}
			patchChecked = true;
		}
	}

	private static IEnumerator ResizeCaret(RectTransform caretTransform, RectTransform textTransform)
	{
		yield return (object)new WaitForEndOfFrame();
		((Transform)caretTransform).localPosition = ((Transform)textTransform).localPosition;
		((Transform)caretTransform).localRotation = ((Transform)textTransform).localRotation;
		((Transform)caretTransform).localScale = ((Transform)textTransform).localScale;
		caretTransform.anchorMin = textTransform.anchorMin;
		caretTransform.anchorMax = textTransform.anchorMax;
		caretTransform.anchoredPosition = textTransform.anchoredPosition;
		caretTransform.sizeDelta = textTransform.sizeDelta;
		caretTransform.pivot = textTransform.pivot;
	}
}
