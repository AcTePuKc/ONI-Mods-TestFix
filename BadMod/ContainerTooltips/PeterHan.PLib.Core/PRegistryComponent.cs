using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace PeterHan.PLib.Core;

internal sealed class PRegistryComponent : MonoBehaviour, IPLibRegistry
{
	internal const string PLIB_HARMONY = "PeterHan.PLib";

	private static PRegistryComponent instance;

	private static bool instantiated;

	private readonly ConcurrentDictionary<string, PVersionList> forwardedComponents;

	private readonly ConcurrentDictionary<string, object> instantiatedComponents;

	private readonly ConcurrentDictionary<string, PForwardedComponent> latestComponents;

	public IDictionary<string, object> ModData { get; }

	public Harmony PLibInstance { get; }

	private static void ApplyLatest()
	{
		bool flag = false;
		if ((Object)(object)instance != (Object)null)
		{
			lock (instance)
			{
				if (!instantiated)
				{
					flag = (instantiated = true);
				}
			}
		}
		if (flag)
		{
			instance.Instantiate();
		}
	}

	internal PRegistryComponent()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		if ((Object)(object)instance == (Object)null)
		{
			instance = this;
		}
		ModData = new ConcurrentDictionary<string, object>(2, 64);
		forwardedComponents = new ConcurrentDictionary<string, PVersionList>(2, 32);
		instantiatedComponents = new ConcurrentDictionary<string, object>(2, 32);
		latestComponents = new ConcurrentDictionary<string, PForwardedComponent>(2, 32);
		PLibInstance = new Harmony("PeterHan.PLib");
	}

	public void AddCandidateVersion(PForwardedComponent instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		AddCandidateVersion(instance.ID, instance);
	}

	private void AddCandidateVersion(string id, PForwardedComponent instance)
	{
		PVersionList orAdd = forwardedComponents.GetOrAdd(id, (string _) => new PVersionList());
		if (orAdd == null)
		{
			PRegistry.LogPatchWarning("Missing version info for component type " + id);
			return;
		}
		List<PForwardedComponent> components = orAdd.Components;
		bool flag = components.Count < 1;
		components.Add(instance);
		if (flag)
		{
			instance.Bootstrap(PLibInstance);
		}
	}

	internal void ApplyBootstrapper()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		try
		{
			PLibInstance.Patch(typeof(Mod), "PostLoad", new HarmonyMethod(typeof(PRegistryComponent), "ApplyLatest", (Type[])null));
		}
		catch (AmbiguousMatchException thrown)
		{
			PUtil.LogException(thrown);
		}
		catch (ArgumentException thrown2)
		{
			PUtil.LogException(thrown2);
		}
		catch (TypeLoadException thrown3)
		{
			PUtil.LogException(thrown3);
		}
	}

	internal void DoAddCandidateVersion(object instance)
	{
		AddCandidateVersion(instance.GetType().FullName, new PRemoteComponent(instance));
	}

	internal ICollection DoGetAllComponents(string id)
	{
		if (!forwardedComponents.TryGetValue(id, out var value))
		{
			value = null;
		}
		return value?.Components;
	}

	internal object DoGetLatestVersion(string id)
	{
		if (!instantiatedComponents.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public IEnumerable<PForwardedComponent> GetAllComponents(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		if (!forwardedComponents.TryGetValue(id, out var value))
		{
			value = null;
		}
		return value?.Components;
	}

	public PForwardedComponent GetLatestVersion(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		if (!latestComponents.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public object GetSharedData(string id)
	{
		if (!forwardedComponents.TryGetValue(id, out var value))
		{
			value = null;
		}
		return value?.SharedData;
	}

	public void Instantiate()
	{
		foreach (KeyValuePair<string, PVersionList> forwardedComponent in forwardedComponents)
		{
			List<PForwardedComponent> components = forwardedComponent.Value.Components;
			int count = components.Count;
			if (count > 0)
			{
				string key = forwardedComponent.Key;
				components.Sort();
				PForwardedComponent pForwardedComponent = components[count - 1];
				latestComponents.GetOrAdd(key, pForwardedComponent);
				try
				{
					instantiatedComponents.GetOrAdd(key, pForwardedComponent?.DoInitialize(PLibInstance));
				}
				catch (Exception thrown)
				{
					PRegistry.LogPatchWarning("Error when instantiating component " + key + ":");
					PUtil.LogException(thrown);
				}
			}
		}
		foreach (KeyValuePair<string, PForwardedComponent> latestComponent in latestComponents)
		{
			try
			{
				latestComponent.Value.PostInitialize(PLibInstance);
			}
			catch (Exception thrown2)
			{
				PRegistry.LogPatchWarning("Error when instantiating component " + latestComponent.Key + ":");
				PUtil.LogException(thrown2);
			}
		}
	}

	public void SetSharedData(string id, object data)
	{
		if (forwardedComponents.TryGetValue(id, out var value))
		{
			value.SharedData = data;
		}
	}

	public override string ToString()
	{
		return forwardedComponents.ToString();
	}
}
