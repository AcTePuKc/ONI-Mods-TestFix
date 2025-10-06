using System;
using System.Collections.Generic;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI.Layouts;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public sealed class PGridLayoutGroup : AbstractLayoutGroup
{
	[SerializeField]
	private IList<GridComponent<GameObject>> children;

	[SerializeField]
	private IList<GridColumnSpec> columns;

	[SerializeField]
	private RectOffset margin;

	private GridLayoutResults results;

	[SerializeField]
	private IList<GridRowSpec> rows;

	public RectOffset Margin
	{
		get
		{
			return margin;
		}
		set
		{
			margin = value;
		}
	}

	private static float[] GetColumnWidths(GridLayoutResults results, float width, RectOffset margin)
	{
		int num = results.Columns;
		float num2 = ((margin != null) ? margin.left : 0);
		float num3 = ((margin != null) ? margin.right : 0);
		float num4 = width - num2 - num3;
		float totalFlexWidth = results.TotalFlexWidth;
		float num5 = ((totalFlexWidth > 0f) ? ((num4 - results.MinWidth) / totalFlexWidth) : 0f);
		float[] array = new float[num + 1];
		for (int i = 0; i < num; i++)
		{
			GridColumnSpec gridColumnSpec = results.ComputedColumnSpecs[i];
			array[i] = num2;
			num2 += gridColumnSpec.Width + gridColumnSpec.FlexWidth * num5;
		}
		array[num] = num2;
		return array;
	}

	private static float[] GetRowHeights(GridLayoutResults results, float height, RectOffset margin)
	{
		int num = results.Rows;
		float num2 = ((margin != null) ? margin.bottom : 0);
		float num3 = ((margin != null) ? margin.top : 0);
		float num4 = height - num2 - num3;
		float totalFlexHeight = results.TotalFlexHeight;
		float num5 = ((totalFlexHeight > 0f) ? ((num4 - results.MinHeight) / totalFlexHeight) : 0f);
		float[] array = new float[num + 1];
		for (int i = 0; i < num; i++)
		{
			GridRowSpec gridRowSpec = results.ComputedRowSpecs[i];
			array[i] = num2;
			num2 += gridRowSpec.Height + gridRowSpec.FlexHeight * num5;
		}
		array[num] = num2;
		return array;
	}

	private static bool SetFinalHeight(SizedGridComponent component, float[] rowY)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		RectOffset val = component.Margin;
		LayoutSizes verticalSize = component.VerticalSize;
		GameObject source = verticalSize.source;
		int num;
		if (!verticalSize.ignore)
		{
			num = (((Object)(object)source != (Object)null) ? 1 : 0);
			if (num != 0)
			{
				int num2 = rowY.Length - 1;
				int row = component.Row;
				int value = row + component.RowSpan;
				row = row.InRange(0, num2 - 1);
				value = value.InRange(1, num2);
				float num3 = rowY[row];
				float num4 = rowY[value] - num3;
				if (val != null)
				{
					float num5 = val.top + val.bottom;
					num3 += (float)val.top;
					num4 -= num5;
					verticalSize.min -= num5;
					verticalSize.preferred -= num5;
				}
				float properSize = PUIUtils.GetProperSize(verticalSize, num4);
				num3 += PUIUtils.GetOffset(component.Alignment, PanelDirection.Vertical, num4 - properSize);
				Util.rectTransform(source).SetInsetAndSizeFromParentEdge((Edge)2, num3, properSize);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private static bool SetFinalWidth(SizedGridComponent component, float[] colX)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		RectOffset val = component.Margin;
		LayoutSizes horizontalSize = component.HorizontalSize;
		GameObject source = horizontalSize.source;
		int num;
		if (!horizontalSize.ignore)
		{
			num = (((Object)(object)source != (Object)null) ? 1 : 0);
			if (num != 0)
			{
				int num2 = colX.Length - 1;
				int column = component.Column;
				int value = column + component.ColumnSpan;
				column = column.InRange(0, num2 - 1);
				value = value.InRange(1, num2);
				float num3 = colX[column];
				float num4 = colX[value] - num3;
				if (val != null)
				{
					float num5 = val.left + val.right;
					num3 += (float)val.left;
					num4 -= num5;
					horizontalSize.min -= num5;
					horizontalSize.preferred -= num5;
				}
				float properSize = PUIUtils.GetProperSize(horizontalSize, num4);
				num3 += PUIUtils.GetOffset(component.Alignment, PanelDirection.Horizontal, num4 - properSize);
				Util.rectTransform(source).SetInsetAndSizeFromParentEdge((Edge)0, num3, properSize);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	internal PGridLayoutGroup()
	{
		children = new List<GridComponent<GameObject>>(16);
		columns = new List<GridColumnSpec>(16);
		rows = new List<GridRowSpec>(16);
		base.layoutPriority = 1;
		Margin = null;
		results = null;
	}

	public void AddColumn(GridColumnSpec column)
	{
		if (column == null)
		{
			throw new ArgumentNullException("column");
		}
		columns.Add(column);
	}

	public void AddComponent(GameObject child, GridComponentSpec spec)
	{
		if ((Object)(object)child == (Object)null)
		{
			throw new ArgumentNullException("child");
		}
		if (spec == null)
		{
			throw new ArgumentNullException("spec");
		}
		children.Add(new GridComponent<GameObject>(spec, child));
		child.SetParent(((Component)this).gameObject);
	}

	public void AddRow(GridRowSpec row)
	{
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		rows.Add(row);
	}

	public override void CalculateLayoutInputHorizontal()
	{
		if (locked)
		{
			return;
		}
		results = new GridLayoutResults(rows, columns, children);
		PooledList<Component, PGridLayoutGroup> val = ListPool<Component, PGridLayoutGroup>.Allocate();
		foreach (SizedGridComponent component in results.Components)
		{
			GameObject source = component.HorizontalSize.source;
			RectOffset val2 = component.Margin;
			((List<Component>)(object)val).Clear();
			source.GetComponents<Component>((List<Component>)(object)val);
			LayoutSizes horizontalSize = PUIUtils.CalcSizes(source, PanelDirection.Horizontal, (IEnumerable<Component>)val);
			if (!horizontalSize.ignore)
			{
				int num = ((val2 != null) ? (val2.left + val2.right) : 0);
				horizontalSize.min += num;
				horizontalSize.preferred += num;
			}
			component.HorizontalSize = horizontalSize;
		}
		val.Recycle();
		results.CalcBaseWidths();
		float num2 = results.MinWidth;
		if (Margin != null)
		{
			num2 += (float)(Margin.left + Margin.right);
		}
		float num3 = (base.preferredWidth = num2);
		base.minWidth = num3;
		base.flexibleWidth = ((results.TotalFlexWidth > 0f) ? 1f : 0f);
	}

	public override void CalculateLayoutInputVertical()
	{
		if (results == null || locked)
		{
			return;
		}
		PooledList<Component, PGridLayoutGroup> val = ListPool<Component, PGridLayoutGroup>.Allocate();
		foreach (SizedGridComponent component in results.Components)
		{
			GameObject source = component.VerticalSize.source;
			RectOffset val2 = component.Margin;
			((List<Component>)(object)val).Clear();
			source.GetComponents<Component>((List<Component>)(object)val);
			LayoutSizes verticalSize = PUIUtils.CalcSizes(source, PanelDirection.Vertical, (IEnumerable<Component>)val);
			if (!verticalSize.ignore)
			{
				int num = ((val2 != null) ? (val2.top + val2.bottom) : 0);
				verticalSize.min += num;
				verticalSize.preferred += num;
			}
			component.VerticalSize = verticalSize;
		}
		val.Recycle();
		results.CalcBaseHeights();
		float num2 = results.MinHeight;
		if (Margin != null)
		{
			num2 += (float)(Margin.bottom + Margin.top);
		}
		float num3 = (base.preferredHeight = num2);
		base.minHeight = num3;
		base.flexibleHeight = ((results.TotalFlexHeight > 0f) ? 1f : 0f);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		results = null;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		results = null;
	}

	public override void SetLayoutHorizontal()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = ((Component)this).gameObject;
		if (results == null || !((Object)(object)gameObject != (Object)null) || results.Columns <= 0 || locked)
		{
			return;
		}
		GridLayoutResults gridLayoutResults = results;
		Rect rect = base.rectTransform.rect;
		float[] columnWidths = GetColumnWidths(gridLayoutResults, ((Rect)(ref rect)).width, Margin);
		PooledList<ILayoutController, PGridLayoutGroup> val = ListPool<ILayoutController, PGridLayoutGroup>.Allocate();
		foreach (SizedGridComponent component in results.Components)
		{
			if (!SetFinalWidth(component, columnWidths))
			{
				continue;
			}
			((List<ILayoutController>)(object)val).Clear();
			component.HorizontalSize.source.GetComponents<ILayoutController>((List<ILayoutController>)(object)val);
			foreach (ILayoutController item in (List<ILayoutController>)(object)val)
			{
				item.SetLayoutHorizontal();
			}
		}
		val.Recycle();
	}

	public override void SetLayoutVertical()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = ((Component)this).gameObject;
		if (results == null || !((Object)(object)gameObject != (Object)null) || results.Rows <= 0 || locked)
		{
			return;
		}
		GridLayoutResults gridLayoutResults = results;
		Rect rect = base.rectTransform.rect;
		float[] rowHeights = GetRowHeights(gridLayoutResults, ((Rect)(ref rect)).height, Margin);
		PooledList<ILayoutController, PGridLayoutGroup> val = ListPool<ILayoutController, PGridLayoutGroup>.Allocate();
		foreach (SizedGridComponent component in results.Components)
		{
			if (SetFinalHeight(component, rowHeights))
			{
				continue;
			}
			((List<ILayoutController>)(object)val).Clear();
			component.VerticalSize.source.GetComponents<ILayoutController>((List<ILayoutController>)(object)val);
			foreach (ILayoutController item in (List<ILayoutController>)(object)val)
			{
				item.SetLayoutVertical();
			}
		}
		val.Recycle();
	}
}
