using GameUtil = global::GameUtil;

namespace BadMod.ContainerTooltips.Enums;

/// <summary>
/// Specifies the preferred units when reporting mass values.
/// </summary>
public enum MassDisplayMode
{
    Default = (int)GameUtil.MetricMassFormat.UseThreshold,
    Kilogram = (int)GameUtil.MetricMassFormat.Kilogram,
    Gram = (int)GameUtil.MetricMassFormat.Gram,
    Tonne = (int)GameUtil.MetricMassFormat.Tonne
}
