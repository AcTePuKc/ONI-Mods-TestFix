using System;
using Database;
using Klei.AI;
using PeterHan.PLib.Detours;

namespace PeterHan.PLib.Database;

public static class PDatabaseUtils
{
	private delegate AttributeModifier NewModifierFunc(string attributeID, float value, Func<string> getDescription, bool multiplier, bool uiOnly);

	private delegate AttributeModifier NewModifierString(string attributeID, float value, string description, bool multiplier, bool uiOnly, bool readOnly);

	private static readonly NewModifierFunc NEW_MODIFIER_FUNC = typeof(AttributeModifier).DetourConstructor<NewModifierFunc>();

	private static readonly NewModifierString NEW_MODIFIER_STRING = typeof(AttributeModifier).DetourConstructor<NewModifierString>();

	public static void AddColonyAchievement(ColonyAchievement achievement)
	{
		if (achievement == null)
		{
			throw new ArgumentNullException("achievement");
		}
		((ResourceSet<ColonyAchievement>)(object)Db.Get()?.ColonyAchievements)?.resources?.Add(achievement);
	}

	public static void AddStatusItemStrings(string id, string category, string name, string desc)
	{
		string text = id.ToUpperInvariant();
		string text2 = category.ToUpperInvariant();
		Strings.Add(new string[2]
		{
			"STRINGS." + text2 + ".STATUSITEMS." + text + ".NAME",
			name
		});
		Strings.Add(new string[2]
		{
			"STRINGS." + text2 + ".STATUSITEMS." + text + ".TOOLTIP",
			desc
		});
	}

	public static AttributeModifier CreateAttributeModifier(string attributeID, float value, string description = null, bool multiplier = false, bool uiOnly = false, bool readOnly = true)
	{
		return NEW_MODIFIER_STRING(attributeID, value, description, multiplier, uiOnly, readOnly);
	}

	public static AttributeModifier CreateAttributeModifier(string attributeID, float value, Func<string> getDescription = null, bool multiplier = false, bool uiOnly = false)
	{
		return NEW_MODIFIER_FUNC(attributeID, value, getDescription, multiplier, uiOnly);
	}

	internal static void LogDatabaseDebug(string message)
	{
		Debug.LogFormat("[PLibDatabase] {0}", new object[1] { message });
	}

	internal static void LogDatabaseWarning(string message)
	{
		Debug.LogWarningFormat("[PLibDatabase] {0}", new object[1] { message });
	}
}
