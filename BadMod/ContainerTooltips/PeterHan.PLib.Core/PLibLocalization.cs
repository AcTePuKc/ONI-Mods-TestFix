using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace PeterHan.PLib.Core;

public static class PLibLocalization
{
	public const string TRANSLATIONS_EXT = ".po";

	private const string TRANSLATIONS_RES_PATH = "PeterHan.PLib.Core.PLibStrings.";

	internal static void LocalizeItself(Locale locale)
	{
		if (locale == null)
		{
			throw new ArgumentNullException("locale");
		}
		Localization.RegisterForTranslation(typeof(PLibStrings));
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string text = locale.Code;
		if (string.IsNullOrEmpty(text))
		{
			text = Localization.GetCurrentLanguageCode();
		}
		try
		{
			using Stream stream = executingAssembly.GetManifestResourceStream("PeterHan.PLib.Core.PLibStrings." + text + ".po");
			if (stream == null)
			{
				return;
			}
			List<string> list = new List<string>(128);
			using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
			{
				string item;
				while ((item = streamReader.ReadLine()) != null)
				{
					list.Add(item);
				}
			}
			Localization.OverloadStrings(Localization.ExtractTranslatedStrings(list.ToArray(), false));
		}
		catch (Exception thrown)
		{
			PUtil.LogWarning("Failed to load {0} localization for PLib Core:".F(text));
			PUtil.LogExcWarn(thrown);
		}
	}
}
