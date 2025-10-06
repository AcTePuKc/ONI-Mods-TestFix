using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.PLib.Core;

public static class PRegistry
{
	private static IPLibRegistry instance = null;

	private static readonly object instanceLock = new object();

	public static IPLibRegistry Instance
	{
		get
		{
			lock (instanceLock)
			{
				if (instance == null)
				{
					Init();
				}
			}
			return instance;
		}
	}

	public static T GetData<T>(string key)
	{
		T result = default(T);
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		IDictionary<string, object> modData = Instance.ModData;
		if (modData != null && modData.TryGetValue(key, out var value) && value is T result2)
		{
			return result2;
		}
		return result;
	}

	private static void Init()
	{
		Global obj = Global.Instance;
		GameObject val = ((obj != null) ? ((Component)obj).gameObject : null);
		if ((Object)(object)val != (Object)null)
		{
			Component component = val.GetComponent("PRegistryComponent");
			if ((Object)(object)component == (Object)null)
			{
				PRegistryComponent pRegistryComponent = val.AddComponent<PRegistryComponent>();
				string name = ((object)pRegistryComponent).GetType().Name;
				if (name != "PRegistryComponent")
				{
					LogPatchWarning("PRegistryComponent has the type name " + name + "; this may be the result of ILMerging PLib more than once!");
				}
				pRegistryComponent.ApplyBootstrapper();
				instance = pRegistryComponent;
			}
			else
			{
				instance = new PRemoteRegistry(component);
			}
		}
		else
		{
			instance = null;
		}
		if (instance != null)
		{
			new PLibCorePatches().Register(instance);
		}
	}

	internal static void LogPatchDebug(string message)
	{
		Debug.LogFormat("[PLibPatches] {0}", new object[1] { message });
	}

	internal static void LogPatchWarning(string message)
	{
		Debug.LogWarningFormat("[PLibPatches] {0}", new object[1] { message });
	}

	public static void PutData(string key, object value)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		IDictionary<string, object> modData = Instance.ModData;
		if (modData != null)
		{
			if (modData.ContainsKey(key))
			{
				modData[key] = value;
			}
			else
			{
				modData.Add(key, value);
			}
		}
	}
}
