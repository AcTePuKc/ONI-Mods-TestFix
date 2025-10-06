using UnityEngine;

namespace PeterHan.PLib.UI.Layouts;

internal sealed class SizedGridComponent : GridComponentSpec
{
	public LayoutSizes HorizontalSize { get; set; }

	public LayoutSizes VerticalSize { get; set; }

	internal SizedGridComponent(GridComponentSpec spec, GameObject item)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		base.Alignment = spec.Alignment;
		base.Column = spec.Column;
		base.ColumnSpan = spec.ColumnSpan;
		base.Margin = spec.Margin;
		base.Row = spec.Row;
		base.RowSpan = spec.RowSpan;
		HorizontalSize = new LayoutSizes(item);
		VerticalSize = new LayoutSizes(item);
	}
}
