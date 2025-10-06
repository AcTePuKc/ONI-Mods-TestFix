using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.UI;

[Serializable]
public sealed class GridColumnSpec
{
	public float FlexWidth { get; }

	public float Width { get; }

	public GridColumnSpec(float width = 0f, float flex = 0f)
	{
		if (width.IsNaNOrInfinity() || width < 0f)
		{
			throw new ArgumentOutOfRangeException("width");
		}
		if (flex.IsNaNOrInfinity() || flex < 0f)
		{
			throw new ArgumentOutOfRangeException("flex");
		}
		Width = width;
		FlexWidth = flex;
	}

	public override string ToString()
	{
		return $"GridColumnSpec[Width={Width:F2}]";
	}
}
