using System;
using UnityEngine;

namespace PeterHan.PLib.UI;

internal struct LayoutSizes
{
	public float flexible;

	public bool ignore;

	public float min;

	public float preferred;

	public readonly GameObject source;

	internal LayoutSizes(GameObject source)
		: this(source, 0f, 0f, 0f)
	{
	}

	internal LayoutSizes(GameObject source, float min, float preferred, float flexible)
	{
		ignore = false;
		this.source = source;
		this.flexible = flexible;
		this.min = min;
		this.preferred = preferred;
	}

	public void Add(LayoutSizes other)
	{
		flexible += other.flexible;
		min += other.min;
		preferred += other.preferred;
	}

	public void Max(LayoutSizes other)
	{
		flexible = Math.Max(flexible, other.flexible);
		min = Math.Max(min, other.min);
		preferred = Math.Max(preferred, other.preferred);
	}

	public override string ToString()
	{
		return $"LayoutSizes[min={min:F2},preferred={preferred:F2},flexible={flexible:F2}]";
	}
}
