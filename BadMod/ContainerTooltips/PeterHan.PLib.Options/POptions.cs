using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using HarmonyLib;
using KMod;
using Newtonsoft.Json;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

public sealed class POptions : PForwardedComponent
{
	private sealed class ModOptionsHandler
	{
		private readonly Type forType;

		private readonly PForwardedComponent options;

		internal ModOptionsHandler(PForwardedComponent options, Type forType)
		{
			this.forType = forType ?? throw new ArgumentNullException("forType");
			this.options = options ?? throw new ArgumentNullException("options");
		}

		internal void ShowDialog(GameObject _)
		{
			options.Process(0u, new OpenDialogArgs(forType, null));
		}

		public override string ToString()
		{
			return "ModOptionsHandler[Type={0}]".F(forType);
		}
	}

	private sealed class OpenDialogArgs
	{
		public Action<object> OnClose { get; }

		public Type OptionsType { get; }

		public OpenDialogArgs(Type optionsType, Action<object> onClose)
		{
			OnClose = onClose;
			OptionsType = optionsType ?? throw new ArgumentNullException("optionsType");
		}

		public override string ToString()
		{
			return "OpenDialogArgs[Type={0}]".F(OptionsType);
		}
	}

	public const string CONFIG_FILE_NAME = "config.json";

	public const int MAX_SERIALIZATION_DEPTH = 8;

	internal static readonly RectOffset OPTION_BUTTON_MARGIN = new RectOffset(11, 11, 5, 5);

	public const string SHARED_CONFIG_FOLDER = "config";

	internal static readonly Version VERSION = new Version("4.17.1.0");

	private readonly IDictionary<string, Type> modOptions;

	private readonly IDictionary<string, ModOptionsHandler> registered;

	internal static POptions Instance { get; private set; }

	public override Version Version => VERSION;

	private static void BuildDisplay_Postfix(GameObject ___entryPrefab, IEnumerable ___displayedMods)
	{
		if (Instance == null)
		{
			return;
		}
		int num = 0;
		foreach (object ___displayedMod in ___displayedMods)
		{
			Instance.AddModOptions(___displayedMod, num++, ___entryPrefab);
		}
	}

	public static string GetConfigFilePath(Type optionsType)
	{
		return GetConfigPath(optionsType.GetCustomAttribute<ConfigFileAttribute>(), optionsType.Assembly);
	}

	internal static Mod GetModFromType(Type optionsType)
	{
		if (optionsType == null)
		{
			throw new ArgumentNullException("optionsType");
		}
		if (!(PRegistry.Instance.GetSharedData(typeof(POptions).FullName) is IDictionary<Assembly, Mod> dictionary) || !dictionary.TryGetValue(optionsType.Assembly, out var value))
		{
			return null;
		}
		return value;
	}

	private static string GetConfigPath(ConfigFileAttribute attr, Assembly modAssembly)
	{
		string nameSafe = modAssembly.GetNameSafe();
		return Path.Combine((nameSafe != null && attr != null && attr.UseSharedConfigLocation) ? Path.Combine(Manager.GetDirectory(), "config", nameSafe) : PUtil.GetModPath(modAssembly), attr?.ConfigFileName ?? "config.json");
	}

	public static T ReadSettings<T>() where T : class
	{
		Type typeFromHandle = typeof(T);
		return ReadSettings(GetConfigPath(typeFromHandle.GetCustomAttribute<ConfigFileAttribute>(), typeFromHandle.Assembly), typeFromHandle) as T;
	}

	internal static object ReadSettings(string path, Type optionsType)
	{
		//IL_004e: Expected O, but got Unknown
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		object result = null;
		try
		{
			JsonTextReader val = new JsonTextReader((TextReader)File.OpenText(path));
			try
			{
				result = new JsonSerializer
				{
					MaxDepth = 8
				}.Deserialize((JsonReader)(object)val, optionsType);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (FileNotFoundException)
		{
		}
		catch (DirectoryNotFoundException)
		{
		}
		catch (UnauthorizedAccessException thrown)
		{
			PUtil.LogExcWarn(thrown);
		}
		catch (IOException thrown2)
		{
			PUtil.LogExcWarn(thrown2);
		}
		catch (JsonException ex3)
		{
			PUtil.LogExcWarn((Exception)ex3);
		}
		return result;
	}

	public static void ShowDialog(Type optionsType, Action<object> onClose = null)
	{
		OpenDialogArgs args = new OpenDialogArgs(optionsType, onClose);
		IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(typeof(POptions).FullName);
		if (allComponents == null)
		{
			return;
		}
		foreach (PForwardedComponent item in allComponents)
		{
			item?.Process(0u, args);
		}
	}

	public static void WriteSettings<T>(T settings) where T : class
	{
		ConfigFileAttribute customAttribute = typeof(T).GetCustomAttribute<ConfigFileAttribute>();
		WriteSettings(settings, GetConfigPath(customAttribute, typeof(T).Assembly), customAttribute?.IndentOutput ?? false);
	}

	internal static void WriteSettings(object settings, string path, bool indent = false)
	{
		//IL_0061: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (settings == null)
		{
			return;
		}
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			JsonTextWriter val = new JsonTextWriter((TextWriter)File.CreateText(path));
			try
			{
				new JsonSerializer
				{
					MaxDepth = 8,
					Formatting = (Formatting)(indent ? 1 : 0)
				}.Serialize((JsonWriter)(object)val, settings);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (UnauthorizedAccessException thrown)
		{
			PUtil.LogExcWarn(thrown);
		}
		catch (IOException thrown2)
		{
			PUtil.LogExcWarn(thrown2);
		}
		catch (JsonException ex)
		{
			PUtil.LogExcWarn((Exception)ex);
		}
	}

	public POptions()
	{
		modOptions = new Dictionary<string, Type>(8);
		registered = new Dictionary<string, ModOptionsHandler>(32);
		InstanceData = modOptions;
	}

	private void AddModOptions(object modEntry, int fallbackIndex, GameObject parent)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		List<Mod> list = Global.Instance.modManager?.mods;
		if (!PPatchTools.TryGetFieldValue<int>(modEntry, "mod_index", out var value))
		{
			value = fallbackIndex;
		}
		if (!PPatchTools.TryGetFieldValue<Transform>(modEntry, "rect_transform", out var value2))
		{
			value2 = parent.transform.GetChild(value);
		}
		if (list != null && value >= 0 && value < list.Count && (Object)(object)value2 != (Object)null)
		{
			Mod val = list[value];
			string staticID = val.staticID;
			if (val.IsEnabledForActiveDlc() && registered.TryGetValue(staticID, out var value3))
			{
				PButton pButton = new PButton("ModSettingsButton");
				pButton.FlexSize = Vector2.up;
				pButton.OnClick = value3.ShowDialog;
				pButton.ToolTip = PLibStrings.DIALOG_TITLE.text.F(val.title);
				pButton.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(PLibStrings.BUTTON_OPTIONS.text.ToLower());
				pButton.Margin = OPTION_BUTTON_MARGIN;
				pButton.SetKleiPinkStyle().AddTo(((Component)value2).gameObject, 4);
			}
		}
	}

	public override void Initialize(Harmony plibInstance)
	{
		Instance = this;
		registered.Clear();
		SetSharedData(PUtil.CreateAssemblyToModTable());
		foreach (PForwardedComponent allComponent in PRegistry.Instance.GetAllComponents(base.ID))
		{
			IDictionary<string, Type> instanceData = allComponent.GetInstanceData<IDictionary<string, Type>>();
			if (instanceData == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, Type> item in instanceData)
			{
				string key = item.Key;
				if (registered.ContainsKey(key))
				{
					PUtil.LogWarning("Mod {0} already has options registered - only one option type per mod".F(key ?? "?"));
				}
				else
				{
					registered.Add(key, new ModOptionsHandler(allComponent, item.Value));
				}
			}
		}
		plibInstance.Patch(typeof(ModsScreen), "BuildDisplay", null, PatchMethod("BuildDisplay_Postfix"));
	}

	public override void Process(uint operation, object args)
	{
		if (operation != 0 || !PPatchTools.TryGetPropertyValue<Type>(args, "OptionsType", out var value))
		{
			return;
		}
		foreach (KeyValuePair<string, Type> modOption in modOptions)
		{
			if (modOption.Value == value)
			{
				OptionsDialog optionsDialog = new OptionsDialog(value);
				if (PPatchTools.TryGetPropertyValue<Action<object>>(args, "OnClose", out var value2))
				{
					optionsDialog.OnClose = value2;
				}
				optionsDialog.ShowDialog();
				break;
			}
		}
	}

	public void RegisterOptions(UserMod2 mod, Type optionsType)
	{
		Mod obj = ((mod != null) ? mod.mod : null);
		if (optionsType == null)
		{
			throw new ArgumentNullException("optionsType");
		}
		if (obj == null)
		{
			throw new ArgumentNullException("mod");
		}
		RegisterForForwarding();
		string staticID = obj.staticID;
		if (modOptions.TryGetValue(staticID, out var value))
		{
			PUtil.LogWarning("Mod {0} already has options type {1}".F(staticID, value.FullName));
		}
		else
		{
			modOptions.Add(staticID, optionsType);
			PUtil.LogDebug("Registered mod options class {0} for {1}".F(optionsType.FullName, staticID));
		}
	}
}
