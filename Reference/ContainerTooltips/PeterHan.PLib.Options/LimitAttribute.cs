using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Options;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class LimitAttribute : Attribute
{
	public double Maximum { get; }

	public double Minimum { get; }

	public LimitAttribute(double min, double max)
	{
		Minimum = (min.IsNaNOrInfinity() ? 0.0 : min);
		Maximum = ((max.IsNaNOrInfinity() || max < min) ? min : max);
	}

	public float ClampToRange(float value)
	{
		return value.InRange((float)Minimum, (float)Maximum);
	}

	public int ClampToRange(int value)
	{
		return value.InRange((int)Minimum, (int)Maximum);
	}

	public bool InRange(double value)
	{
		if (value >= Minimum)
		{
			return value <= Maximum;
		}
		return false;
	}

	public override string ToString()
	{
		return "{0:F2} to {1:F2}".F(Minimum, Maximum);
	}
}
