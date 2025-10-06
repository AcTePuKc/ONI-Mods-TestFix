using System;
using System.Collections.Generic;
using PeterHan.PLib.Core;
using UnityEngine;

namespace PeterHan.PLib.Actions;

public sealed class PToolMode
{
	public string Key { get; }

	public ToggleState State { get; }

	public LocString Title { get; }

	public static IDictionary<string, ToggleState> PopulateMenu(ToolParameterMenu menu, ICollection<PToolMode> options)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		Dictionary<string, ToggleState> dictionary = new Dictionary<string, ToggleState>(options.Count);
		foreach (PToolMode option in options)
		{
			string key = option.Key;
			if (!string.IsNullOrEmpty(LocString.op_Implicit(option.Title)))
			{
				Strings.Add(new string[2]
				{
					"STRINGS.UI.TOOLS.FILTERLAYERS." + key,
					LocString.op_Implicit(option.Title)
				});
			}
			dictionary.Add(key, option.State);
		}
		menu.PopulateMenu(dictionary);
		return dictionary;
	}

	public static void RegisterTool<T>(PlayerController controller) where T : InterfaceTool
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)controller == (Object)null)
		{
			throw new ArgumentNullException("controller");
		}
		PooledList<InterfaceTool, PlayerController> val = ListPool<InterfaceTool, PlayerController>.Allocate();
		((List<InterfaceTool>)(object)val).AddRange((IEnumerable<InterfaceTool>)controller.tools);
		GameObject val2 = new GameObject(typeof(T).Name);
		T val3 = val2.AddComponent<T>();
		val2.transform.SetParent(((Component)controller).gameObject.transform);
		val2.gameObject.SetActive(true);
		val2.gameObject.SetActive(false);
		((List<InterfaceTool>)(object)val).Add((InterfaceTool)(object)val3);
		controller.tools = ((List<InterfaceTool>)(object)val).ToArray();
		val.Recycle();
	}

	public PToolMode(string key, LocString title, ToggleState state = (ToggleState)1)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		Key = key;
		State = state;
		Title = title;
	}

	public override bool Equals(object obj)
	{
		if (obj is PToolMode pToolMode)
		{
			return pToolMode.Key == Key;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Key.GetHashCode();
	}

	public override string ToString()
	{
		return "{0} ({1})".F(Key, Title);
	}
}
