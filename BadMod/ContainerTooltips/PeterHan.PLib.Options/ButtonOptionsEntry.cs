using System;
using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

public class ButtonOptionsEntry : OptionsEntry
{
	private Action<object> value;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value is Action<object> action)
			{
				this.value = action;
			}
		}
	}

	public ButtonOptionsEntry(string field, IOptionSpec spec)
		: base(field, spec)
	{
	}

	public override void CreateUIEntry(PGridPanel parent, ref int row)
	{
		parent.AddChild(new PButton(base.Field)
		{
			Text = OptionsEntry.LookInStrings(base.Title),
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			OnClick = OnButtonClicked
		}.SetKleiPinkStyle(), new GridComponentSpec(row, 0)
		{
			Margin = OptionsEntry.CONTROL_MARGIN,
			Alignment = (TextAnchor)4,
			ColumnSpan = 2
		});
	}

	private void OnButtonClicked(GameObject _)
	{
		value?.Invoke(null);
	}

	public override GameObject GetUIComponent()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		return new GameObject("Empty");
	}
}
