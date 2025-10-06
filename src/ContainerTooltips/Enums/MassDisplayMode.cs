using OniGameUtil = global::GameUtil;

namespace BadMod.ContainerTooltips.Enums;

/// <summary>
/// Specifies the preferred units when reporting mass values.
/// </summary>
public enum MassDisplayMode
{
    Default = (int)OniGameUtil.MetricMassFormat.UseThreshold,
    Kilogram = (int)OniGameUtil.MetricMassFormat.Kilogram,
    Gram = (int)OniGameUtil.MetricMassFormat.Gram,
    Tonne = (int)OniGameUtil.MetricMassFormat.Tonne
}
