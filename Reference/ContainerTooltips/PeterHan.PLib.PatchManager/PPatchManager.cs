using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.PatchManager;

public sealed class PPatchManager : PForwardedComponent
{
	internal const BindingFlags FLAGS = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;

	internal const BindingFlags FLAGS_EITHER = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	internal static readonly Version VERSION = new Version("4.17.1.0");

	private static volatile bool afterModsLoaded = false;

	private readonly Harmony harmony;

	private readonly IDictionary<uint, ICollection<IPatchMethodInstance>> patches;

	internal static PPatchManager Instance { get; private set; }

	public override Version Version => VERSION;

	private static void Game_DestroyInstances_Postfix()
	{
		Instance?.InvokeAllProcess(6u, null);
	}

	private static void Game_OnPrefabInit_Postfix()
	{
		Instance?.InvokeAllProcess(5u, null);
	}

	private static void Initialize_Prefix()
	{
		Instance?.InvokeAllProcess(2u, null);
	}

	private static void Initialize_Postfix()
	{
		Instance?.InvokeAllProcess(3u, null);
	}

	private static void PostProcess_Prefix()
	{
		Instance?.InvokeAllProcess(8u, null);
	}

	private static void PostProcess_Postfix()
	{
		Instance?.InvokeAllProcess(9u, null);
	}

	private static void Instance_Postfix()
	{
		bool flag = false;
		if (Instance != null)
		{
			lock (VERSION)
			{
				if (!afterModsLoaded)
				{
					flag = (afterModsLoaded = true);
				}
			}
		}
		if (flag)
		{
			Instance.InvokeAllProcess(7u, null);
		}
	}

	private static void MainMenu_OnSpawn_Postfix()
	{
		Instance?.InvokeAllProcess(4u, null);
	}

	private static void DetailsScreen_OnPrefabInit_Postfix()
	{
		Instance?.InvokeAllProcess(10u, null);
	}

	internal static bool CheckConditions(string assemblyName, string typeName, out Type requiredType)
	{
		bool result = false;
		bool flag = string.IsNullOrEmpty(typeName);
		if (string.IsNullOrEmpty(assemblyName))
		{
			if (flag)
			{
				requiredType = null;
				result = true;
			}
			else
			{
				requiredType = PPatchTools.GetTypeSafe(typeName);
				result = requiredType != null;
			}
		}
		else if (flag)
		{
			requiredType = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				if (assemblies[i].GetName().Name == assemblyName)
				{
					result = true;
					break;
				}
			}
		}
		else
		{
			requiredType = PPatchTools.GetTypeSafe(typeName, assemblyName);
			result = requiredType != null;
		}
		return result;
	}

	public PPatchManager(Harmony harmony)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (harmony == null)
		{
			PUtil.LogWarning("Use the Harmony instance from OnLoad to create PPatchManager");
			harmony = new Harmony("PLib.PostLoad." + Assembly.GetExecutingAssembly().GetNameSafe());
		}
		this.harmony = harmony;
		patches = new Dictionary<uint, ICollection<IPatchMethodInstance>>(11);
		InstanceData = patches;
	}

	private void AddHandler(uint when, IPatchMethodInstance instance)
	{
		if (!patches.TryGetValue(when, out var value))
		{
			patches.Add(when, value = new List<IPatchMethodInstance>(16));
		}
		value.Add(instance);
	}

	public override void Initialize(Harmony plibInstance)
	{
		Instance = this;
		plibInstance.Patch(typeof(Db), "Initialize", PatchMethod("Initialize_Prefix"), PatchMethod("Initialize_Postfix"));
		plibInstance.Patch(typeof(Db), "PostProcess", PatchMethod("PostProcess_Prefix"), PatchMethod("PostProcess_Postfix"));
		plibInstance.Patch(typeof(Game), "DestroyInstances", null, PatchMethod("Game_DestroyInstances_Postfix"));
		plibInstance.Patch(typeof(Game), "OnPrefabInit", null, PatchMethod("Game_OnPrefabInit_Postfix"));
		plibInstance.Patch(typeof(GlobalResources), "Instance", null, PatchMethod("Instance_Postfix"));
		plibInstance.Patch(typeof(MainMenu), "OnSpawn", null, PatchMethod("MainMenu_OnSpawn_Postfix"));
		plibInstance.Patch(typeof(DetailsScreen), "OnPrefabInit", null, PatchMethod("DetailsScreen_OnPrefabInit_Postfix"));
	}

	public override void PostInitialize(Harmony plibInstance)
	{
		InvokeAllProcess(1u, null);
	}

	public override void Process(uint when, object _)
	{
		if (!patches.TryGetValue(when, out var value) || value == null || value.Count <= 0)
		{
			return;
		}
		string text = RunAt.ToString(when);
		foreach (IPatchMethodInstance item in value)
		{
			try
			{
				item.Run(harmony);
			}
			catch (TargetInvocationException ex)
			{
				PUtil.LogError("Error running patches for stage " + text + ":");
				PUtil.LogException(ex.GetBaseException());
			}
			catch (Exception thrown)
			{
				PUtil.LogError("Error running patches for stage " + text + ":");
				PUtil.LogException(thrown);
			}
		}
	}

	public void RegisterPatch(uint when, IPatchMethodInstance patch)
	{
		RegisterForForwarding();
		if (patch == null)
		{
			throw new ArgumentNullException("patch");
		}
		if (when == 0)
		{
			patch.Run(harmony);
		}
		else
		{
			AddHandler(when, patch);
		}
	}

	public void RegisterPatchClass(Type type)
	{
		int num = 0;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		RegisterForForwarding();
		MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			object[] customAttributes = methodInfo.GetCustomAttributes(inherit: true);
			for (int j = 0; j < customAttributes.Length; j++)
			{
				if (customAttributes[j] is IPLibAnnotation iPLibAnnotation)
				{
					uint runtime = iPLibAnnotation.Runtime;
					IPatchMethodInstance patchMethodInstance = iPLibAnnotation.CreateInstance(methodInfo);
					if (runtime == 0)
					{
						patchMethodInstance.Run(harmony);
					}
					else
					{
						AddHandler(iPLibAnnotation.Runtime, patchMethodInstance);
					}
					num++;
				}
			}
		}
		if (num > 0)
		{
			PRegistry.LogPatchDebug("Registered {0:D} handler(s) for {1}".F(num, Assembly.GetCallingAssembly().GetNameSafe() ?? "?"));
		}
		else
		{
			PRegistry.LogPatchWarning("RegisterPatchClass could not find any handlers!");
		}
	}
}
