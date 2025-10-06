using System.Collections.Generic;

namespace PeterHan.PLib.UI.Layouts;

internal sealed class CardLayoutResults
{
	public readonly ICollection<LayoutSizes> children;

	public readonly PanelDirection direction;

	public LayoutSizes total;

	internal CardLayoutResults(PanelDirection direction, int presize)
	{
		children = new List<LayoutSizes>(presize);
		this.direction = direction;
		total = default(LayoutSizes);
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
