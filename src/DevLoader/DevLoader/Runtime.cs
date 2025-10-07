using System;
using UnityEngine;

namespace DevLoader;

public static class Runtime
{
	public static event Action<bool> Toggled;

	public static void ApplyToggle(bool enabled)
	{
		try
		{
			Config.Set(enabled);
			if (enabled)
			{
				LiveLoader.LoadAll();
			}
			else
			{
				LiveLoader.UnloadAll();
			}
			Runtime.Toggled?.Invoke(enabled);
			Debug.Log((object)("[DevLoader] Estado: " + (enabled ? "ON (activos)" : "OFF (desactivados)")));
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[DevLoader] Toggle error: " + ex));
		}
	}
}
