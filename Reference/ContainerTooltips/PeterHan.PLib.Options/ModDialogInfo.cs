using System;
using System.Reflection;
using KMod;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Options;

internal sealed class ModDialogInfo
{
	public string Image { get; }

	public string Title { get; }

	public string URL { get; }

	public string Version { get; }

	private static string GetModVersionText(Type optionsType)
	{
		Assembly assembly = optionsType.Assembly;
		string fileVersion = assembly.GetFileVersion();
		if (string.IsNullOrEmpty(fileVersion))
		{
			return string.Format(LocString.op_Implicit(PLibStrings.MOD_ASSEMBLY_VERSION), assembly.GetName().Version);
		}
		return string.Format(LocString.op_Implicit(PLibStrings.MOD_VERSION), fileVersion);
	}

	internal ModDialogInfo(Type type, string url, string image)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		Mod modFromType = POptions.GetModFromType(type);
		Image = image ?? "";
		string text2;
		string text3;
		if (modFromType != null)
		{
			PackagedModInfo packagedModInfo = modFromType.packagedModInfo;
			string text = ((packagedModInfo != null) ? packagedModInfo.version : null);
			text2 = modFromType.title;
			if (string.IsNullOrEmpty(url) && (int)modFromType.label.distribution_platform == 1)
			{
				url = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + modFromType.label.id;
			}
			text3 = (string.IsNullOrEmpty(text) ? GetModVersionText(type) : string.Format(LocString.op_Implicit(PLibStrings.MOD_VERSION), text));
		}
		else
		{
			text2 = type.Assembly.GetNameSafe();
			text3 = GetModVersionText(type);
		}
		Title = text2 ?? "";
		URL = url ?? "";
		Version = text3;
	}

	public override string ToString()
	{
		return base.ToString();
	}
}
