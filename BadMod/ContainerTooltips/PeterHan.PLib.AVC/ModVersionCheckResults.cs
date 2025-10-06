using Newtonsoft.Json;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.AVC;

[JsonObject(/*Could not decode attribute arguments.*/)]
public sealed class ModVersionCheckResults
{
	[JsonProperty]
	public bool IsUpToDate { get; set; }

	[JsonProperty]
	public string ModChecked { get; set; }

	[JsonProperty]
	public string NewVersion { get; set; }

	public ModVersionCheckResults()
		: this("", updated: false)
	{
	}

	public ModVersionCheckResults(string id, bool updated, string newVersion = null)
	{
		IsUpToDate = updated;
		ModChecked = id;
		NewVersion = newVersion;
	}

	public override bool Equals(object obj)
	{
		if (obj is ModVersionCheckResults modVersionCheckResults && modVersionCheckResults.ModChecked == ModChecked && IsUpToDate == modVersionCheckResults.IsUpToDate)
		{
			return NewVersion == modVersionCheckResults.NewVersion;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ModChecked.GetHashCode();
	}

	public override string ToString()
	{
		return "ModVersionCheckResults[{0},updated={1},newVersion={2}]".F(ModChecked, IsUpToDate, NewVersion ?? "");
	}
}
