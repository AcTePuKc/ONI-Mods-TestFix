using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.PatchManager;
using UnityEngine;

namespace PeterHan.PLib.Buildings;

public sealed class PBuildingManager : PForwardedComponent
{
	private sealed class BuildingTechRegistration : IPatchMethodInstance
	{
		public void Run(Harmony instance)
		{
			Instance?.AddAllTechs();
		}
	}

	internal static readonly Version VERSION = new Version("4.17.1.0");

	private readonly ICollection<PBuilding> buildings;

	internal static PBuildingManager Instance { get; private set; }

	public override Version Version => VERSION;

	public static void AddExistingBuildingToTech(string tech, string id)
	{
		if (string.IsNullOrEmpty(tech))
		{
			throw new ArgumentNullException("tech");
		}
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		(((ResourceSet<Tech>)(object)Db.Get().Techs)?.TryGet(id))?.unlockedItemIDs?.Add(tech);
	}

	private static void CreateBuildingDef_Postfix(BuildingDef __result, string anim, string id)
	{
		KAnimFile[] array = __result?.AnimFiles;
		if (array != null && array.Length != 0 && (Object)(object)array[0] == (Object)null)
		{
			Debug.LogWarningFormat("(when looking for KAnim named {0} on building {1})", new object[2] { anim, id });
		}
	}

	private static void CreateEquipmentDef_Postfix(EquipmentDef __result, string Anim, string Id)
	{
		if ((Object)(object)__result?.Anim == (Object)null)
		{
			Debug.LogWarningFormat("(when looking for KAnim named {0} on equipment {1})", new object[2] { Anim, Id });
		}
	}

	internal static void LogBuildingDebug(string message)
	{
		Debug.LogFormat("[PLibBuildings] {0}", new object[1] { message });
	}

	private static void LoadGeneratedBuildings_Prefix()
	{
		Instance?.AddAllStrings();
	}

	public PBuildingManager()
	{
		buildings = new List<PBuilding>(16);
	}

	private void AddAllStrings()
	{
		InvokeAllProcess(0u, null);
	}

	private void AddStrings()
	{
		int count = buildings.Count;
		if (count <= 0)
		{
			return;
		}
		LogBuildingDebug("Register strings for {0:D} building(s) from {1}".F(count, Assembly.GetExecutingAssembly().GetNameSafe() ?? "?"));
		foreach (PBuilding building in buildings)
		{
			if (building != null)
			{
				building.AddStrings();
				building.AddPlan();
			}
		}
	}

	private void AddAllTechs()
	{
		InvokeAllProcess(1u, null);
	}

	private void AddTechs()
	{
		int count = buildings.Count;
		if (count <= 0)
		{
			return;
		}
		LogBuildingDebug("Register techs for {0:D} building(s) from {1}".F(count, Assembly.GetExecutingAssembly().GetNameSafe() ?? "?"));
		foreach (PBuilding building in buildings)
		{
			building?.AddTech();
		}
	}

	public void Register(PBuilding building)
	{
		if (building == null)
		{
			throw new ArgumentNullException("building");
		}
		RegisterForForwarding();
		buildings.Add(building);
	}

	public override void Initialize(Harmony plibInstance)
	{
		Instance = this;
		try
		{
			plibInstance.Patch(typeof(BuildingTemplates), "CreateBuildingDef", null, PatchMethod("CreateBuildingDef_Postfix"));
			plibInstance.Patch(typeof(EquipmentTemplates), "CreateEquipmentDef", null, PatchMethod("CreateEquipmentDef_Postfix"));
		}
		catch (Exception)
		{
		}
		plibInstance.Patch(typeof(GeneratedBuildings), "LoadGeneratedBuildings", PatchMethod("LoadGeneratedBuildings_Prefix"));
		new PPatchManager(plibInstance).RegisterPatch(3u, new BuildingTechRegistration());
	}

	public override void Process(uint operation, object _)
	{
		switch (operation)
		{
		case 0u:
			AddStrings();
			break;
		case 1u:
			AddTechs();
			break;
		}
	}
}
