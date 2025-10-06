using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.PLib.UI.Layouts;

internal sealed class GridLayoutResults
{
	public IList<GridColumnSpec> ColumnSpecs { get; }

	public ICollection<SizedGridComponent> Components { get; }

	public int Columns { get; }

	public IList<GridColumnSpec> ComputedColumnSpecs { get; }

	public IList<GridRowSpec> ComputedRowSpecs { get; }

	public float MinHeight { get; private set; }

	public float MinWidth { get; private set; }

	public ICollection<SizedGridComponent>[,] Matrix { get; }

	public IList<GridRowSpec> RowSpecs { get; }

	public int Rows { get; }

	public float TotalFlexHeight { get; private set; }

	public float TotalFlexWidth { get; private set; }

	private static ICollection<SizedGridComponent>[,] GetMatrix(int rows, int columns, ICollection<SizedGridComponent> components)
	{
		ICollection<SizedGridComponent>[,] array = new ICollection<SizedGridComponent>[rows, columns];
		foreach (SizedGridComponent component in components)
		{
			int row = component.Row;
			int column = component.Column;
			if (row >= 0 && row < rows && column >= 0 && column < columns)
			{
				ICollection<SizedGridComponent> collection = array[row, column];
				if (collection == null)
				{
					collection = (array[row, column] = new List<SizedGridComponent>(8));
				}
				collection.Add(component);
			}
		}
		return array;
	}

	internal GridLayoutResults(IList<GridRowSpec> rows, IList<GridColumnSpec> columns, ICollection<GridComponent<GameObject>> components)
	{
		if (rows == null)
		{
			throw new ArgumentNullException("rows");
		}
		if (columns == null)
		{
			throw new ArgumentNullException("columns");
		}
		if (components == null)
		{
			throw new ArgumentNullException("components");
		}
		Columns = columns.Count;
		Rows = rows.Count;
		ColumnSpecs = columns;
		MinHeight = (MinWidth = 0f);
		RowSpecs = rows;
		ComputedColumnSpecs = new List<GridColumnSpec>(Columns);
		ComputedRowSpecs = new List<GridRowSpec>(Rows);
		TotalFlexHeight = (TotalFlexWidth = 0f);
		Components = new List<SizedGridComponent>(Math.Max(components.Count, 4));
		foreach (GridComponent<GameObject> component in components)
		{
			GameObject item = component.Item;
			if ((Object)(object)item != (Object)null)
			{
				Components.Add(new SizedGridComponent(component, item));
			}
		}
		Matrix = GetMatrix(Rows, Columns, Components);
	}

	internal void CalcBaseHeights()
	{
		int rows = Rows;
		float minHeight = (TotalFlexHeight = 0f);
		MinHeight = minHeight;
		ComputedRowSpecs.Clear();
		for (int i = 0; i < rows; i++)
		{
			GridRowSpec gridRowSpec = RowSpecs[i];
			float num2 = gridRowSpec.Height;
			float flexHeight = gridRowSpec.FlexHeight;
			if (num2 <= 0f)
			{
				for (int j = 0; j < Columns; j++)
				{
					num2 = Math.Max(num2, PreferredHeightAt(i, j));
				}
			}
			if (flexHeight > 0f)
			{
				TotalFlexHeight += flexHeight;
			}
			ComputedRowSpecs.Add(new GridRowSpec(num2, flexHeight));
		}
		foreach (SizedGridComponent component in Components)
		{
			if (component.RowSpan > 1)
			{
				ExpandMultiRow(component);
			}
		}
		for (int k = 0; k < rows; k++)
		{
			MinHeight += ComputedRowSpecs[k].Height;
		}
	}

	internal void CalcBaseWidths()
	{
		int columns = Columns;
		float minWidth = (TotalFlexWidth = 0f);
		MinWidth = minWidth;
		ComputedColumnSpecs.Clear();
		for (int i = 0; i < columns; i++)
		{
			GridColumnSpec gridColumnSpec = ColumnSpecs[i];
			float num2 = gridColumnSpec.Width;
			float flexWidth = gridColumnSpec.FlexWidth;
			if (num2 <= 0f)
			{
				for (int j = 0; j < Rows; j++)
				{
					num2 = Math.Max(num2, PreferredWidthAt(j, i));
				}
			}
			if (flexWidth > 0f)
			{
				TotalFlexWidth += flexWidth;
			}
			ComputedColumnSpecs.Add(new GridColumnSpec(num2, flexWidth));
		}
		foreach (SizedGridComponent component in Components)
		{
			if (component.ColumnSpan > 1)
			{
				ExpandMultiColumn(component);
			}
		}
		for (int k = 0; k < columns; k++)
		{
			MinWidth += ComputedColumnSpecs[k].Width;
		}
	}

	private void ExpandMultiColumn(SizedGridComponent component)
	{
		float num = component.HorizontalSize.preferred;
		float num2 = 0f;
		int column = component.Column;
		int num3 = column + component.ColumnSpan;
		for (int i = column; i < num3; i++)
		{
			GridColumnSpec gridColumnSpec = ComputedColumnSpecs[i];
			if (gridColumnSpec.FlexWidth > 0f)
			{
				num2 += gridColumnSpec.FlexWidth;
			}
			num -= gridColumnSpec.Width;
		}
		if (!(num > 0f) || !(num2 > 0f))
		{
			return;
		}
		for (int j = column; j < num3; j++)
		{
			GridColumnSpec gridColumnSpec2 = ComputedColumnSpecs[j];
			float flexWidth = gridColumnSpec2.FlexWidth;
			if (flexWidth > 0f)
			{
				ComputedColumnSpecs[j] = new GridColumnSpec(gridColumnSpec2.Width + flexWidth * num / num2, flexWidth);
			}
		}
	}

	private void ExpandMultiRow(SizedGridComponent component)
	{
		float num = component.VerticalSize.preferred;
		float num2 = 0f;
		int row = component.Row;
		int num3 = row + component.RowSpan;
		for (int i = row; i < num3; i++)
		{
			GridRowSpec gridRowSpec = ComputedRowSpecs[i];
			if (gridRowSpec.FlexHeight > 0f)
			{
				num2 += gridRowSpec.FlexHeight;
			}
			num -= gridRowSpec.Height;
		}
		if (!(num > 0f) || !(num2 > 0f))
		{
			return;
		}
		for (int j = row; j < num3; j++)
		{
			GridRowSpec gridRowSpec2 = ComputedRowSpecs[j];
			float flexHeight = gridRowSpec2.FlexHeight;
			if (flexHeight > 0f)
			{
				ComputedRowSpecs[j] = new GridRowSpec(gridRowSpec2.Height + flexHeight * num / num2, flexHeight);
			}
		}
	}

	private float PreferredHeightAt(int row, int column)
	{
		float num = 0f;
		ICollection<SizedGridComponent> collection = Matrix[row, column];
		if (collection != null && collection.Count > 0)
		{
			foreach (SizedGridComponent item in collection)
			{
				LayoutSizes verticalSize = item.VerticalSize;
				if (item.RowSpan < 2)
				{
					num = Math.Max(num, verticalSize.preferred);
				}
			}
		}
		return num;
	}

	private float PreferredWidthAt(int row, int column)
	{
		float num = 0f;
		ICollection<SizedGridComponent> collection = Matrix[row, column];
		if (collection != null && collection.Count > 0)
		{
			foreach (SizedGridComponent item in collection)
			{
				LayoutSizes horizontalSize = item.HorizontalSize;
				if (item.ColumnSpan < 2)
				{
					num = Math.Max(num, horizontalSize.preferred);
				}
			}
		}
		return num;
	}
}
