using System;

namespace PeterHan.PLib.UI;

[Serializable]
internal sealed class GridComponent<T> : GridComponentSpec where T : class
{
	public T Item { get; }

	internal GridComponent(GridComponentSpec spec, T item)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		base.Alignment = spec.Alignment;
		Item = item;
		base.Column = spec.Column;
		base.ColumnSpan = spec.ColumnSpan;
		base.Margin = spec.Margin;
		base.Row = spec.Row;
		base.RowSpan = spec.RowSpan;
	}
}
