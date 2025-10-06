using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.UI;

namespace PeterHan.PLib.Core;

internal sealed class PLibCorePatches : PForwardedComponent
{
	internal static readonly Version VERSION = new Version("4.17.1.0");

	public override Version Version => VERSION;

	private static void Initialize_Postfix()
	{
		Locale locale = Localization.GetLocale();
		if (locale == null)
		{
			return;
		}
		int num = 0;
		string text = locale.Code;
		if (string.IsNullOrEmpty(text))
		{
			text = Localization.GetCurrentLanguageCode();
		}
		IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(typeof(PLibCorePatches).FullName);
		if (allComponents != null)
		{
			foreach (PForwardedComponent item in allComponents)
			{
				item.Process(0u, locale);
			}
		}
		IEnumerable<PForwardedComponent> allComponents2 = PRegistry.Instance.GetAllComponents("PeterHan.PLib.Database.PLocalization");
		if (allComponents2 != null)
		{
			foreach (PForwardedComponent item2 in allComponents2)
			{
				item2.Process(0u, locale);
				num++;
			}
		}
		if (num > 0)
		{
			PRegistry.LogPatchDebug("Localized {0:D} mod(s) to locale {1}".F(num, text));
		}
	}

	private static IEnumerable<CodeInstruction> LoadPreviewImage_Transpile(IEnumerable<CodeInstruction> body)
	{
		MethodInfo methodSafe = typeof(Debug).GetMethodSafe("LogFormat", true, typeof(string), typeof(object[]));
		if (!(methodSafe == null))
		{
			return PPatchTools.RemoveMethodCall(body, methodSafe);
		}
		return body;
	}

	public override void Initialize(Harmony plibInstance)
	{
		TextMeshProPatcher.Patch();
		Type typeSafe = PPatchTools.GetTypeSafe("SteamUGCService", "Assembly-CSharp");
		if (typeSafe != null)
		{
			try
			{
				plibInstance.PatchTranspile(typeSafe, "LoadPreviewImage", PatchMethod("LoadPreviewImage_Transpile"));
			}
			catch (Exception)
			{
			}
		}
		plibInstance.Patch(typeof(Localization), "Initialize", null, PatchMethod("Initialize_Postfix"));
	}

	public override void Process(uint operation, object _)
	{
		Locale locale = Localization.GetLocale();
		if (locale != null && operation == 0)
		{
			PLibLocalization.LocalizeItself(locale);
		}
	}

	internal void Register(IPLibRegistry instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		instance.AddCandidateVersion(this);
	}
}
