using System.Collections;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;
using STRINGS;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.AVC;

internal sealed class ModOutdatedWarning : KMonoBehaviour
{
	private static readonly IDetouredField<MainMenu, KButton> RESUME_GAME = PDetours.DetourFieldLazy<MainMenu, KButton>("Button_ResumeGame");

	private GameObject modsButton;

	internal static ModOutdatedWarning Instance { get; private set; }

	internal ModOutdatedWarning()
	{
		modsButton = null;
	}

	private void FindModsButton(Transform buttonParent)
	{
		int childCount = buttonParent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			GameObject gameObject = ((Component)buttonParent.GetChild(i)).gameObject;
			if ((Object)(object)gameObject != (Object)null)
			{
				LocText componentInChildren = gameObject.GetComponentInChildren<LocText>();
				if (((componentInChildren != null) ? ((TMP_Text)componentInChildren).text : null) == LocString.op_Implicit(MODS.TITLE))
				{
					modsButton = gameObject;
					break;
				}
			}
		}
		if ((Object)(object)modsButton == (Object)null)
		{
			PUtil.LogWarning("Unable to find Mods menu button, main menu update warning will not be functional");
		}
	}

	protected override void OnCleanUp()
	{
		Instance = null;
		((KMonoBehaviour)this).OnCleanUp();
	}

	protected override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		MainMenu component = ((Component)this).GetComponent<MainMenu>();
		Instance = this;
		try
		{
			KButton val;
			Transform parent;
			if ((Object)(object)component != (Object)null && (Object)(object)(val = RESUME_GAME.Get(component)) != (Object)null && (Object)(object)(parent = ((KMonoBehaviour)val).transform.parent) != (Object)null)
			{
				FindModsButton(parent);
			}
		}
		catch (DetourException)
		{
		}
		UpdateText();
	}

	private void UpdateText()
	{
		PVersionCheck instance = PVersionCheck.Instance;
		if ((Object)(object)modsButton != (Object)null && instance != null)
		{
			LocText componentInChildren = modsButton.GetComponentInChildren<LocText>();
			int outdatedMods = instance.OutdatedMods;
			if (outdatedMods > 0 && (Object)(object)componentInChildren != (Object)null)
			{
				string text = LocString.op_Implicit(MODS.TITLE);
				text = ((outdatedMods != 1) ? (text + string.Format(LocString.op_Implicit(PLibStrings.MAINMENU_UPDATE), outdatedMods)) : (text + LocString.op_Implicit(PLibStrings.MAINMENU_UPDATE_1)));
				((TMP_Text)componentInChildren).text = text;
			}
		}
	}

	public IEnumerator UpdateTextThreaded()
	{
		yield return null;
		UpdateText();
	}
}
