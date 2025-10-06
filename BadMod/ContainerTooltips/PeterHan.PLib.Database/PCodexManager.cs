using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Klei;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Database;

public sealed class PCodexManager : PForwardedComponent
{
	public const string CREATURES_DIR = "codex/Creatures";

	public const string PLANTS_DIR = "codex/Plants";

	public const string STORY_DIR = "codex/StoryTraits";

	public const string CODEX_FILES = "*.yaml";

	public const string CREATURES_CATEGORY = "CREATURES";

	public const string PLANTS_CATEGORY = "PLANTS";

	public const string STORY_CATEGORY = "STORYTRAITS";

	internal static readonly Version VERSION = new Version("4.17.1.0");

	private static readonly FieldInfo WIDGET_TAG_MAPPINGS = typeof(CodexCache).GetFieldSafe("widgetTagMappings", isStatic: true);

	private readonly ISet<string> creaturePaths;

	private readonly ISet<string> plantPaths;

	private readonly ISet<string> storyPaths;

	internal static PCodexManager Instance { get; private set; }

	public override Version Version => VERSION;

	private static void CollectEntries_Postfix(string folder, List<CodexEntry> __result, string ___baseEntryPath)
	{
		if (Instance == null)
		{
			return;
		}
		string obj = (string.IsNullOrEmpty(folder) ? ___baseEntryPath : Path.Combine(___baseEntryPath, folder));
		bool flag = false;
		if (obj.EndsWith("Creatures"))
		{
			__result.AddRange(Instance.LoadEntries("CREATURES"));
			flag = true;
		}
		if (obj.EndsWith("Plants"))
		{
			__result.AddRange(Instance.LoadEntries("PLANTS"));
			flag = true;
		}
		if (obj.EndsWith("StoryTraits"))
		{
			__result.AddRange(Instance.LoadEntries("STORYTRAITS"));
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		foreach (CodexEntry item in __result)
		{
			if (string.IsNullOrEmpty(item.sortString))
			{
				item.sortString = StringEntry.op_Implicit(Strings.Get(item.title));
			}
		}
		__result.Sort((CodexEntry x, CodexEntry y) => string.Compare(x.sortString, y.sortString, StringComparison.CurrentCulture));
	}

	private static void CollectSubEntries_Postfix(List<SubEntry> __result)
	{
		if (Instance == null)
		{
			return;
		}
		int count = __result.Count;
		__result.AddRange(Instance.LoadSubEntries());
		if (__result.Count != count)
		{
			__result.Sort((SubEntry x, SubEntry y) => string.Compare(x.title, y.title, StringComparison.CurrentCulture));
		}
	}

	private static void LoadFromDirectory(ICollection<CodexEntry> entries, string dir, string category)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		string[] array = Array.Empty<string>();
		try
		{
			array = Directory.GetFiles(dir, "*.yaml");
		}
		catch (UnauthorizedAccessException thrown)
		{
			PUtil.LogExcWarn(thrown);
		}
		catch (IOException thrown2)
		{
			PUtil.LogExcWarn(thrown2);
		}
		List<Tuple<string, Type>> list = WIDGET_TAG_MAPPINGS?.GetValue(null) as List<Tuple<string, Type>>;
		if (list == null)
		{
			PDatabaseUtils.LogDatabaseWarning("Unable to load codex files: no tag mappings found");
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			try
			{
				CodexEntry val = YamlIO.LoadFile<CodexEntry>(text, new ErrorHandler(YamlParseErrorCB), list);
				if (val != null)
				{
					val.category = category;
					entries?.Add(val);
				}
			}
			catch (IOException thrown3)
			{
				PDatabaseUtils.LogDatabaseWarning("Unable to load codex files from {0}:".F(dir));
				PUtil.LogExcWarn(thrown3);
			}
			catch (InvalidDataException thrown4)
			{
				PUtil.LogException(thrown4);
			}
		}
	}

	private static void LoadFromDirectory(ICollection<SubEntry> entries, string dir)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		string[] array = Array.Empty<string>();
		try
		{
			array = Directory.GetFiles(dir, "*.yaml", SearchOption.AllDirectories);
		}
		catch (UnauthorizedAccessException thrown)
		{
			PUtil.LogExcWarn(thrown);
		}
		catch (IOException thrown2)
		{
			PUtil.LogExcWarn(thrown2);
		}
		List<Tuple<string, Type>> list = WIDGET_TAG_MAPPINGS?.GetValue(null) as List<Tuple<string, Type>>;
		if (list == null)
		{
			PDatabaseUtils.LogDatabaseWarning("Unable to load codex files: no tag mappings found");
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			try
			{
				SubEntry item = YamlIO.LoadFile<SubEntry>(text, new ErrorHandler(YamlParseErrorCB), list);
				entries?.Add(item);
			}
			catch (IOException thrown3)
			{
				PDatabaseUtils.LogDatabaseWarning("Unable to load codex files from {0}:".F(dir));
				PUtil.LogExcWarn(thrown3);
			}
			catch (InvalidDataException thrown4)
			{
				PUtil.LogException(thrown4);
			}
		}
	}

	internal static void YamlParseErrorCB(Error error, bool _)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		throw new InvalidDataException($"{error.severity} parse error in {error.file.full_path}\n{error.message}", error.inner_exception);
	}

	public PCodexManager()
	{
		creaturePaths = new HashSet<string>();
		plantPaths = new HashSet<string>();
		storyPaths = new HashSet<string>();
		InstanceData = new Dictionary<string, ISet<string>>(4)
		{
			{ "CREATURES", creaturePaths },
			{ "PLANTS", plantPaths },
			{ "STORYTRAITS", storyPaths }
		};
		PUtil.InitLibrary(logVersion: false);
		PRegistry.Instance.AddCandidateVersion(this);
	}

	public override void Initialize(Harmony plibInstance)
	{
		Instance = this;
		plibInstance.Patch(typeof(CodexCache), "CollectEntries", null, PatchMethod("CollectEntries_Postfix"));
		plibInstance.Patch(typeof(CodexCache), "CollectSubEntries", null, PatchMethod("CollectSubEntries_Postfix"));
	}

	private IEnumerable<CodexEntry> LoadEntries(string category)
	{
		List<CodexEntry> list = new List<CodexEntry>(32);
		IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(base.ID);
		if (allComponents != null)
		{
			foreach (PForwardedComponent item in allComponents)
			{
				Dictionary<string, ISet<string>> dictionary = item?.GetInstanceData<Dictionary<string, ISet<string>>>();
				if (dictionary == null || !dictionary.TryGetValue(category, out var value))
				{
					continue;
				}
				foreach (string item2 in value)
				{
					LoadFromDirectory(list, item2, category);
				}
			}
		}
		return list;
	}

	private IEnumerable<SubEntry> LoadSubEntries()
	{
		List<SubEntry> list = new List<SubEntry>(32);
		IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(base.ID);
		if (allComponents != null)
		{
			foreach (PForwardedComponent item in allComponents)
			{
				Dictionary<string, ISet<string>> dictionary = item?.GetInstanceData<Dictionary<string, ISet<string>>>();
				if (dictionary == null)
				{
					continue;
				}
				foreach (KeyValuePair<string, ISet<string>> item2 in dictionary)
				{
					foreach (string item3 in item2.Value)
					{
						LoadFromDirectory(list, item3);
					}
				}
			}
		}
		return list;
	}

	public void RegisterCreatures(Assembly assembly = null)
	{
		if (assembly == null)
		{
			assembly = Assembly.GetCallingAssembly();
		}
		string item = Path.Combine(PUtil.GetModPath(assembly), "codex/Creatures");
		creaturePaths.Add(item);
	}

	public void RegisterPlants(Assembly assembly = null)
	{
		if (assembly == null)
		{
			assembly = Assembly.GetCallingAssembly();
		}
		string item = Path.Combine(PUtil.GetModPath(assembly), "codex/Plants");
		plantPaths.Add(item);
	}

	public void RegisterStory(Assembly assembly = null)
	{
		if (assembly == null)
		{
			assembly = Assembly.GetCallingAssembly();
		}
		string item = Path.Combine(PUtil.GetModPath(assembly), "codex/StoryTraits");
		storyPaths.Add(item);
	}
}
