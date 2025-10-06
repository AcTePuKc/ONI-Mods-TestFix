using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Database;

public sealed class PLocalization : PForwardedComponent
{
	public const string TRANSLATIONS_DIR = "translations";

	internal static readonly Version VERSION = new Version("4.17.1.0");

	private readonly ICollection<Assembly> toLocalize;

	public override Version Version => VERSION;

	private static void Localize(Assembly modAssembly, Locale locale)
	{
		string modPath = PUtil.GetModPath(modAssembly);
		string text = locale.Code;
		if (string.IsNullOrEmpty(text))
		{
			text = Localization.GetCurrentLanguageCode();
		}
		string text2 = Path.Combine(Path.Combine(modPath, "translations"), text + ".po");
		try
		{
			Localization.OverloadStrings(Localization.LoadStringsFile(text2, false));
			RewriteStrings(modAssembly);
		}
		catch (FileNotFoundException)
		{
		}
		catch (DirectoryNotFoundException)
		{
		}
		catch (IOException thrown)
		{
			PDatabaseUtils.LogDatabaseWarning("Failed to load {0} localization for mod {1}:".F(text, modAssembly.GetNameSafe() ?? "?"));
			PUtil.LogExcWarn(thrown);
		}
	}

	internal static void RewriteStrings(Assembly assembly)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Type[] types = assembly.GetTypes();
		for (int i = 0; i < types.Length; i++)
		{
			FieldInfo[] fields = types[i].GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.FieldType == typeof(LocString))
				{
					object value = fieldInfo.GetValue(null);
					LocString val = (LocString)((value is LocString) ? value : null);
					if (val != null)
					{
						Strings.Add(new string[2]
						{
							val.key.String,
							val.text
						});
					}
				}
			}
		}
	}

	public PLocalization()
	{
		toLocalize = new List<Assembly>(4);
		InstanceData = toLocalize;
	}

	internal void DumpAll()
	{
		IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(base.ID);
		if (allComponents == null)
		{
			return;
		}
		foreach (PForwardedComponent item in allComponents)
		{
			ICollection<Assembly> instanceData = item.GetInstanceData<ICollection<Assembly>>();
			if (instanceData == null)
			{
				continue;
			}
			foreach (Assembly item2 in instanceData)
			{
				ModUtil.RegisterForTranslation(item2.GetTypes()[0]);
			}
		}
	}

	public override void Initialize(Harmony plibInstance)
	{
	}

	public override void Process(uint operation, object _)
	{
		Locale locale = Localization.GetLocale();
		if (locale == null || operation != 0)
		{
			return;
		}
		foreach (Assembly item in toLocalize)
		{
			Localize(item, locale);
		}
	}

	public void Register(Assembly assembly = null)
	{
		if (assembly == null)
		{
			assembly = Assembly.GetCallingAssembly();
		}
		Type[] types = assembly.GetTypes();
		if (types == null || types.Length == 0)
		{
			PDatabaseUtils.LogDatabaseWarning("Registered assembly " + assembly.GetNameSafe() + " that had no types for localization!");
			return;
		}
		RegisterForForwarding();
		toLocalize.Add(assembly);
		Localization.RegisterForTranslation(types[0]);
	}
}
