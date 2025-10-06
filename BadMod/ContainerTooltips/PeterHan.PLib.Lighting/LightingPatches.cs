using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;
using UnityEngine;

namespace PeterHan.PLib.Lighting;

internal static class LightingPatches
{
	private static readonly IDetouredField<Light2D, int> ORIGIN = PDetours.DetourFieldLazy<Light2D, int>("origin");

	private static readonly IDetouredField<LightShapePreview, int> PREVIOUS_CELL = PDetours.DetourFieldLazy<LightShapePreview, int>("previousCell");

	private static bool ComputeExtents_Prefix(Light2D __instance, ref Extents __result)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		PLightManager instance = PLightManager.Instance;
		bool result = true;
		if (instance != null && (Object)(object)__instance != (Object)null)
		{
			LightShape shape = __instance.shape;
			int num = Mathf.CeilToInt(__instance.Range);
			instance.AddLight(__instance.emitter, ((Component)__instance).gameObject);
			int num2;
			if ((int)shape > 1 && num > 0 && Grid.IsValidCell(num2 = ORIGIN.Get(__instance)))
			{
				int num3 = default(int);
				int num4 = default(int);
				Grid.CellToXY(num2, ref num3, ref num4);
				__result = new Extents(num3 - num, num4 - num, 2 * num, 2 * num);
				result = false;
			}
		}
		return result;
	}

	public static void ApplyPatches(Harmony plibInstance)
	{
		plibInstance.Patch(typeof(Light2D), "ComputeExtents", PatchMethod("ComputeExtents_Prefix"));
		plibInstance.Patch(typeof(Light2D), "FullRemove", null, PatchMethod("Light2D_FullRemove_Postfix"));
		plibInstance.Patch(typeof(Light2D), "RefreshShapeAndPosition", null, PatchMethod("Light2D_RefreshShapeAndPosition_Postfix"));
		try
		{
			plibInstance.PatchTranspile(typeof(LightBuffer), "LateUpdate", PatchMethod("LightBuffer_LateUpdate_Transpile"));
		}
		catch (Exception thrown)
		{
			PUtil.LogExcWarn(thrown);
		}
		plibInstance.Patch(typeof(LightGridEmitter), "ComputeLux", PatchMethod("ComputeLux_Prefix"));
		plibInstance.Patch(typeof(LightGridEmitter), "UpdateLitCells", PatchMethod("UpdateLitCells_Prefix"));
		plibInstance.Patch((MethodBase)typeof(LightGridManager).GetOverloadWithMostArguments("CreatePreview", true, typeof(int), typeof(float), typeof(LightShape), typeof(int)), PatchMethod("CreatePreview_Prefix"), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		plibInstance.Patch(typeof(LightShapePreview), "Update", PatchMethod("LightShapePreview_Update_Prefix"));
		plibInstance.Patch(typeof(Rotatable), "OrientVisualizer", null, PatchMethod("OrientVisualizer_Postfix"));
	}

	private static bool ComputeLux_Prefix(LightGridEmitter __instance, int cell, State ___state, ref int __result)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		PLightManager instance = PLightManager.Instance;
		if (instance != null)
		{
			return !instance.GetBrightness(__instance, cell, ___state, out __result);
		}
		return true;
	}

	private static bool CreatePreview_Prefix(int origin_cell, float radius, LightShape shape, int lux)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		PLightManager instance = PLightManager.Instance;
		if (instance != null)
		{
			return !instance.PreviewLight(origin_cell, radius, shape, lux);
		}
		return true;
	}

	private static void Light2D_FullRemove_Postfix(Light2D __instance)
	{
		PLightManager instance = PLightManager.Instance;
		if (instance != null && (Object)(object)__instance != (Object)null)
		{
			instance.DestroyLight(__instance.emitter);
		}
	}

	private static void Light2D_RefreshShapeAndPosition_Postfix(Light2D __instance, RefreshResult __result)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		PLightManager instance = PLightManager.Instance;
		if (instance != null && (Object)(object)__instance != (Object)null && (int)__result == 2)
		{
			instance.AddLight(__instance.emitter, ((Component)__instance).gameObject);
		}
	}

	private static IEnumerable<CodeInstruction> LightBuffer_LateUpdate_Transpile(IEnumerable<CodeInstruction> body)
	{
		MethodInfo methodInfo = typeof(Light2D).GetPropertySafe<LightShape>("shape", isStatic: false)?.GetGetMethod(nonPublic: true);
		if (!(methodInfo == null))
		{
			return PPatchTools.ReplaceMethodCallSafe(body, methodInfo, typeof(PLightManager).GetMethodSafe("LightShapeToRayShape", true, typeof(Light2D)));
		}
		return body;
	}

	private static void LightShapePreview_Update_Prefix(LightShapePreview __instance)
	{
		PLightManager instance = PLightManager.Instance;
		if (instance != null && (Object)(object)__instance != (Object)null)
		{
			instance.PreviewObject = ((Component)__instance).gameObject;
		}
	}

	private static void OrientVisualizer_Postfix(Rotatable __instance)
	{
		LightShapePreview arg = default(LightShapePreview);
		if ((Object)(object)__instance != (Object)null && ((Component)__instance).TryGetComponent<LightShapePreview>(ref arg))
		{
			PREVIOUS_CELL.Set(arg, -1);
		}
	}

	private static HarmonyMethod PatchMethod(string name)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		return new HarmonyMethod(typeof(LightingPatches), name, (Type[])null);
	}

	private static bool UpdateLitCells_Prefix(LightGridEmitter __instance, List<int> ___litCells, State ___state)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		PLightManager instance = PLightManager.Instance;
		if (instance != null)
		{
			return !instance.UpdateLitCells(__instance, ___state, ___litCells);
		}
		return true;
	}
}
