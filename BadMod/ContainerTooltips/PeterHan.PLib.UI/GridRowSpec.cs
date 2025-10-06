using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.UI;

[Serializable]
public sealed class GridRowSpec
{
	public float FlexHeight { get; }

	public float Height { get; }

	public GridRowSpec(float height = 0f, float flex = 0f)
	{
		if (height.IsNaNOrInfinity() || height < 0f)
		{
			throw new ArgumentOutOfRangeException("height");
		}
		if (flex.IsNaNOrInfinity() || flex < 0f)
		{
			throw new ArgumentOutOfRangeException("flex");
		}
		Height = height;
		FlexHeight = flex;
	}

	public override string ToString()
	{
		return $"GridRowSpec[Height={Height:F2}]";
	}
}
