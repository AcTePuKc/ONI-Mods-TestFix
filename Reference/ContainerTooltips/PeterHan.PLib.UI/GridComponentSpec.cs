using System;
using UnityEngine;

namespace PeterHan.PLib.UI;

public class GridComponentSpec
{
	public TextAnchor Alignment { get; set; }

	public int Column { get; set; }

	public int ColumnSpan { get; set; }

	public RectOffset Margin { get; set; }

	public int Row { get; set; }

	public int RowSpan { get; set; }

	internal GridComponentSpec()
	{
	}

	public GridComponentSpec(int row, int column)
	{
		if (row < 0)
		{
			throw new ArgumentOutOfRangeException("row");
		}
		if (column < 0)
		{
			throw new ArgumentOutOfRangeException("column");
		}
		Alignment = (TextAnchor)4;
		Row = row;
		Column = column;
		Margin = null;
		RowSpan = 1;
		ColumnSpan = 1;
	}

	public override string ToString()
	{
		return $"GridComponentSpec[Row={Row:D},Column={Column:D},RowSpan={RowSpan:D},ColumnSpan={ColumnSpan:D}]";
	}
}
