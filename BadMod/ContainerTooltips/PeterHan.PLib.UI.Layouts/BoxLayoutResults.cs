using System.Collections.Generic;

namespace PeterHan.PLib.UI.Layouts;

internal sealed class BoxLayoutResults
{
	public readonly ICollection<LayoutSizes> children;

	public readonly PanelDirection direction;

	private bool haveMinSpace;

	private bool havePrefSpace;

	public LayoutSizes total;

	internal BoxLayoutResults(PanelDirection direction, int presize)
	{
		children = new List<LayoutSizes>(presize);
		this.direction = direction;
		haveMinSpace = false;
		havePrefSpace = false;
		total = default(LayoutSizes);
	}

	public void Accum(LayoutSizes sizes, float spacing)
	{
		float num = sizes.min;
		float num2 = sizes.preferred;
		if (num > 0f)
		{
			if (haveMinSpace)
			{
				num += spacing;
			}
			haveMinSpace = true;
		}
		total.min += num;
		if (num2 > 0f)
		{
			if (havePrefSpace)
			{
				num2 += spacing;
			}
			havePrefSpace = true;
		}
		total.preferred += num2;
		total.flexible += sizes.flexible;
	}

	public void Expand(LayoutSizes sizes)
	{
		float min = sizes.min;
		float preferred = sizes.preferred;
		float flexible = sizes.flexible;
		if (min > total.min)
		{
			total.min = min;
		}
		if (preferred > total.preferred)
		{
			total.preferred = preferred;
		}
		if (flexible > total.flexible)
		{
			total.flexible = flexible;
		}
	}

	public override string ToString()
	{
		string text = direction.ToString();
		LayoutSizes layoutSizes = total;
		return text + " " + layoutSizes.ToString();
	}
}
