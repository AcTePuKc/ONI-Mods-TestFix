using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HarmonyLib;
using PeterHan.PLib.Core;
using UnityEngine;

namespace PeterHan.PLib.Lighting;

public sealed class PLightManager : PForwardedComponent
{
	public delegate void CastLightDelegate(LightingArgs args);

	private sealed class CacheEntry
	{
		internal int BaseLux { get; set; }

		internal IDictionary<int, float> Intensity { get; }

		internal GameObject Owner { get; }

		internal CacheEntry(GameObject owner)
		{
			Intensity = new Dictionary<int, float>(64);
			Owner = owner;
		}

		public override string ToString()
		{
			return "Lighting Cache Entry for " + (((Object)(object)Owner == (Object)null) ? "" : ((Object)Owner).name);
		}
	}

	private static readonly List<object> EMPTY_SHAPES = new List<object>(1);

	internal static readonly Version VERSION = new Version("4.17.1.0");

	private readonly ConcurrentDictionary<LightGridEmitter, CacheEntry> brightCache;

	private readonly IList<ILightShape> shapes;

	internal static bool ForceSmoothLight { get; set; }

	internal static PLightManager Instance { get; private set; }

	public override Version Version => VERSION;

	internal GameObject PreviewObject { get; set; }

	public static float GetDefaultFalloff(float falloffRate, int cell, int origin)
	{
		return 1f / Math.Max(1f, Mathf.RoundToInt(falloffRate * (float)Math.Max(Grid.GetCellDistance(origin, cell), 1)));
	}

	public static float GetSmoothFalloff(float falloffRate, int cell, int origin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Vector2I val = Grid.CellToXY(cell);
		Vector2I val2 = Grid.CellToXY(origin);
		return 1f / Math.Max(1f, falloffRate * PUtil.Distance(((Vector2I)(ref val2)).X, ((Vector2I)(ref val2)).Y, ((Vector2I)(ref val)).X, ((Vector2I)(ref val)).Y));
	}

	internal static LightShape LightShapeToRayShape(Light2D light)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		LightShape val = light.shape;
		if ((int)val != 1 && (int)val != 0)
		{
			val = Instance.GetRayShape(val);
		}
		return val;
	}

	internal static void LogLightingDebug(string message)
	{
		Debug.LogFormat("[PLibLighting] {0}", new object[1] { message });
	}

	internal static void LogLightingWarning(string message)
	{
		Debug.LogWarningFormat("[PLibLighting] {0}", new object[1] { message });
	}

	public PLightManager()
	{
		brightCache = new ConcurrentDictionary<LightGridEmitter, CacheEntry>(2, 128);
		PreviewObject = null;
		shapes = new List<ILightShape>(16);
	}

	internal void AddLight(LightGridEmitter source, GameObject owner)
	{
		if ((Object)(object)owner == (Object)null)
		{
			throw new ArgumentNullException("owner");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		brightCache.TryAdd(source, new CacheEntry(owner));
	}

	public override void Bootstrap(Harmony plibInstance)
	{
		SetSharedData(new List<object>(16));
	}

	internal void DestroyLight(LightGridEmitter source)
	{
		if (source != null)
		{
			brightCache.TryRemove(source, out var _);
		}
	}

	internal bool GetBrightness(LightGridEmitter source, int location, State state, out int result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		LightShape shape = state.shape;
		bool flag;
		if ((int)shape != 1 && (int)shape != 0)
		{
			flag = brightCache.TryGetValue(source, out var value);
			if (flag)
			{
				flag = value.Intensity.TryGetValue(location, out var value2);
				if (flag)
				{
					result = Mathf.RoundToInt((float)value.BaseLux * value2);
				}
				else
				{
					result = 0;
				}
			}
			else
			{
				result = 0;
			}
		}
		else if (ForceSmoothLight)
		{
			result = Mathf.RoundToInt((float)state.intensity * GetSmoothFalloff(state.falloffRate, location, state.origin));
			flag = true;
		}
		else
		{
			result = 0;
			flag = false;
		}
		return flag;
	}

	internal LightShape GetRayShape(LightShape shape)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected I4, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		int num = shape - 1 - 1;
		ILightShape lightShape;
		if (num >= 0 && num < shapes.Count && (lightShape = shapes[num]) != null)
		{
			LightShape rayMode = lightShape.RayMode;
			if ((int)rayMode >= 0)
			{
				shape = rayMode;
			}
		}
		return shape;
	}

	public override void Initialize(Harmony plibInstance)
	{
		Instance = this;
		shapes.Clear();
		foreach (object sharedDatum in GetSharedData(EMPTY_SHAPES))
		{
			ILightShape lightShape = PRemoteLightWrapper.LightToInstance(sharedDatum);
			shapes.Add(lightShape);
			if (lightShape == null)
			{
				LogLightingWarning("Foreign contaminant in PLightManager!");
			}
		}
		LightingPatches.ApplyPatches(plibInstance);
	}

	internal bool PreviewLight(int origin, float radius, LightShape shape, int lux)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected I4, but got Unknown
		bool result = false;
		GameObject previewObject = PreviewObject;
		int num = shape - 1 - 1;
		if (num >= 0 && num < shapes.Count && (Object)(object)previewObject != (Object)null)
		{
			PooledDictionary<int, float, PLightManager> val = DictionaryPool<int, float, PLightManager>.Allocate();
			shapes[num]?.FillLight(new LightingArgs(previewObject, origin, (int)radius, (IDictionary<int, float>)val));
			foreach (KeyValuePair<int, float> item in (Dictionary<int, float>)(object)val)
			{
				int key = item.Key;
				if (Grid.IsValidCell(key))
				{
					int num2 = Mathf.RoundToInt((float)lux * item.Value);
					LightGridManager.previewLightCells.Add(new Tuple<int, int>(key, num2));
					LightGridManager.previewLux[key] = num2;
				}
			}
			PreviewObject = null;
			result = true;
			val.Recycle();
		}
		return result;
	}

	public ILightShape Register(string identifier, CastLightDelegate handler, LightShape rayMode = (LightShape)(-1))
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(identifier))
		{
			throw new ArgumentNullException("identifier");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		ILightShape lightShape = null;
		RegisterForForwarding();
		List<object> sharedData = GetSharedData(EMPTY_SHAPES);
		int count = sharedData.Count;
		foreach (object item in sharedData)
		{
			ILightShape lightShape2 = PRemoteLightWrapper.LightToInstance(item);
			if (lightShape2 != null && lightShape2.Identifier == identifier)
			{
				LogLightingDebug("Found existing light shape: " + identifier + " from " + (item.GetType().Assembly.GetNameSafe() ?? "?"));
				lightShape = lightShape2;
				break;
			}
		}
		if (lightShape == null)
		{
			lightShape = new PLightShape(count + 1, identifier, handler, rayMode);
			LogLightingDebug("Registered new light shape: " + identifier);
			sharedData.Add(lightShape);
		}
		return lightShape;
	}

	internal bool UpdateLitCells(LightGridEmitter source, State state, IList<int> litCells)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected I4, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		int num = state.shape - 1 - 1;
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (num >= 0 && num < shapes.Count && litCells != null && brightCache.TryGetValue(source, out var value))
		{
			ILightShape lightShape = shapes[num];
			IDictionary<int, float> intensity = value.Intensity;
			intensity.Clear();
			value.BaseLux = state.intensity;
			lightShape.FillLight(new LightingArgs(value.Owner, state.origin, (int)state.radius, intensity));
			foreach (KeyValuePair<int, float> item in intensity)
			{
				litCells.Add(item.Key);
			}
			result = true;
		}
		return result;
	}
}
