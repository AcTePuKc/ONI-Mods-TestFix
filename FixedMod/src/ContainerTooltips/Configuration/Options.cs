using System.Collections.Generic;
using BadMod.ContainerTooltips.Enums;
using BadMod.ContainerTooltips.Mod;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace BadMod.ContainerTooltips.Configuration;

[JsonObject(MemberSerialization.OptIn)]
[ModInfo("Container Tooltips")]
public sealed class Options : SingletonOptions<Options>, IOptions
{
    [Option("Sort Order", "Controls how container contents are ordered in the displayed list.")]
    [JsonProperty]
    public ContentSortMode SortMode { get; set; } = ContentSortMode.Default;

    [Option("Mass Units", "Preferred units when displaying mass values.")]
    [JsonProperty]
    public MassDisplayMode MassUnits { get; set; } = MassDisplayMode.Default;

    [Option("Content List Limit", "Maximum number of contents to show in the hover card/tooltip (also appears in the information window's Status panel).")]
    [Limit(1, 100)]
    [JsonProperty]
    public int StatusLineLimit { get; set; } = 5;

    [Option("Detailed List Limit", "Maximum number of contents to show when hovering over the content list itself in the information window's Status panel.")]
    [Limit(1, 100)]
    [JsonProperty]
    public int TooltipLineLimit { get; set; } = 20;

    [Option("Format", "Format string for each line of the contents list. Use {0} for item name, {1} for item amount, and {2} for temperature.")]
    [JsonProperty]
    public string LineFormat { get; set; } = "{1} of {0} at {2}";

    public void OnOptionsChanged()
    {
        instance = POptions.ReadSettings<Options>() ?? new Options();
        UserMod.ClearSummaryCache();
    }

    public IEnumerable<IOptionsEntry> CreateOptions()
    {
        yield break;
    }
}
