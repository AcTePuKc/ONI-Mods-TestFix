using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ContainerTooltips;

[JsonObject(/*Could not decode attribute arguments.*/)]
[ModInfo("https://github.com/Identifier/ONIMods", null, false)]
public sealed class Options : SingletonOptions<Options>, IOptions
{
	[Option("Sort Order", "Controls how container contents are ordered in the displayed list.", null)]
	public ContentSortMode SortMode { get; set; }

	[Option("Mass Units", "Preferred units when displaying mass values.", null)]
	public MassDisplayMode MassUnits { get; set; }

	[Option("Content List Limit", "Maximum number of contents to show in the hover card/tooltip (also appears in the information window's Status panel).", null)]
	[Limit(1.0, 100.0)]
	public int StatusLineLimit { get; set; } = 5;

	[Option("Detailed List Limit", "Maximum number of contents to show when hovering over the content list itself in the information window's Status panel.", null)]
	[Limit(1.0, 100.0)]
	public int TooltipLineLimit { get; set; } = 20;

	[Option("Format", "Format string for each line of the contents list. Use {0} for item name, {1} for item amount, and {2} for temperature.", null)]
	public string LineFormat { get; set; } = "{1} of {0} at {2}";

	public void OnOptionsChanged()
	{
		SingletonOptions<Options>.instance = POptions.ReadSettings<Options>() ?? new Options();
	}

	public IEnumerable<IOptionsEntry> CreateOptions()
	{
		yield break;
	}
}
