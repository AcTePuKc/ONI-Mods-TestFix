using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.PLib.UI;

public class PGridPanel : PContainer, IDynamicSizable, IUIComponent
{
	private readonly ICollection<GridComponent<IUIComponent>> children;

	private readonly IList<GridColumnSpec> columns;

	private readonly IList<GridRowSpec> rows;

	public int Columns => columns.Count;

	public bool DynamicSize { get; set; }

	public int Rows => rows.Count;

	public PGridPanel()
		: this(null)
	{
	}

	public PGridPanel(string name)
		: base(name ?? "GridPanel")
	{
		children = new List<GridComponent<IUIComponent>>(16);
		columns = new List<GridColumnSpec>(16);
		rows = new List<GridRowSpec>(16);
		DynamicSize = true;
		base.Margin = null;
	}

	public PGridPanel AddChild(IUIComponent child, GridComponentSpec spec)
	{
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		if (spec == null)
		{
			throw new ArgumentNullException("spec");
		}
		children.Add(new GridComponent<IUIComponent>(spec, child));
		return this;
	}

	public PGridPanel AddColumn(GridColumnSpec column)
	{
		if (column == null)
		{
			throw new ArgumentNullException("column");
		}
		columns.Add(column);
		return this;
	}

	public PGridPanel AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		base.OnRealize += onRealize;
		return this;
	}

	public PGridPanel AddRow(GridRowSpec row)
	{
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		rows.Add(row);
		return this;
	}

	public override GameObject Build()
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (Columns < 1)
		{
			throw new InvalidOperationException("At least one column must be defined");
		}
		if (Rows < 1)
		{
			throw new InvalidOperationException("At least one row must be defined");
		}
		GameObject val = PUIElements.CreateUI(null, base.Name);
		SetImage(val);
		PGridLayoutGroup pGridLayoutGroup = val.AddComponent<PGridLayoutGroup>();
		pGridLayoutGroup.Margin = base.Margin;
		foreach (GridColumnSpec column in columns)
		{
			pGridLayoutGroup.AddColumn(column);
		}
		foreach (GridRowSpec row in rows)
		{
			pGridLayoutGroup.AddRow(row);
		}
		foreach (GridComponent<IUIComponent> child in children)
		{
			pGridLayoutGroup.AddComponent(child.Item.Build(), child);
		}
		if (!DynamicSize)
		{
			pGridLayoutGroup.LockLayout();
		}
		pGridLayoutGroup.flexibleWidth = base.FlexSize.x;
		pGridLayoutGroup.flexibleHeight = base.FlexSize.y;
		InvokeRealize(val);
		return val;
	}

	public override string ToString()
	{
		return $"PGridPanel[Name={base.Name},Rows={Rows:D},Columns={Columns:D}]";
	}
}
