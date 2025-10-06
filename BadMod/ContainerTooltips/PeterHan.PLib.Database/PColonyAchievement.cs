using System;
using System.Collections.Generic;
using Database;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;

namespace PeterHan.PLib.Database;

public sealed class PColonyAchievement
{
	private delegate ColonyAchievement NewColonyAchievement(string Id, string platformAchievementId, string Name, string description, bool isVictoryCondition, List<ColonyAchievementRequirement> requirementChecklist, string messageTitle, string messageBody, string videoDataName, string victoryLoopVideo, Action<KMonoBehaviour> VictorySequence);

	private static readonly NewColonyAchievement NEW_COLONY_ACHIEVEMENT = typeof(ColonyAchievement).DetourConstructor<NewColonyAchievement>();

	public string Description { get; set; }

	public string Icon { get; set; }

	public string ID { get; }

	public bool IsVictory { get; set; }

	public string Name { get; set; }

	public Action<KMonoBehaviour> OnVictory { get; set; }

	public List<ColonyAchievementRequirement> Requirements { get; set; }

	[Obsolete("Set victory audio snapshot directly due to Klei changes in the Sweet Dreams update")]
	public string VictoryAudioSnapshot { get; set; }

	public string VictoryMessage { get; set; }

	public string VictoryTitle { get; set; }

	public string VictoryVideoData { get; set; }

	public string VictoryVideoLoop { get; set; }

	public PColonyAchievement(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		Description = "";
		Icon = "";
		ID = id;
		IsVictory = false;
		Name = "";
		OnVictory = null;
		Requirements = null;
		VictoryMessage = "";
		VictoryTitle = "";
		VictoryVideoData = "";
		VictoryVideoLoop = "";
	}

	public void AddAchievement()
	{
		if (Requirements == null)
		{
			throw new ArgumentNullException("Requirements");
		}
		ColonyAchievement obj = NEW_COLONY_ACHIEVEMENT(ID, "", Name, Description, IsVictory, Requirements, VictoryTitle, VictoryMessage, VictoryVideoData, VictoryVideoLoop, OnVictory);
		obj.icon = Icon;
		PDatabaseUtils.AddColonyAchievement(obj);
	}

	public override bool Equals(object obj)
	{
		if (obj is PColonyAchievement pColonyAchievement)
		{
			return ID == pColonyAchievement.ID;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ID.GetHashCode();
	}

	public override string ToString()
	{
		return "PColonyAchievement[ID={0},Name={1}]".F(ID, Name);
	}
}
