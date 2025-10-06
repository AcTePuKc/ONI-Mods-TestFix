using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

public class TextBlockOptionsEntry : OptionsEntry
{
	private static readonly TextStyleSetting WRAP_TEXT_STYLE;

	private LocText ignore;

	public override object Value
	{
		get
		{
			return ignore;
		}
		set
		{
			LocText val = (LocText)((value is LocText) ? value : null);
			if (val != null)
			{
				ignore = val;
			}
		}
	}

	static TextBlockOptionsEntry()
	{
		WRAP_TEXT_STYLE = PUITuning.Fonts.TextLightStyle.DeriveStyle();
		WRAP_TEXT_STYLE.enableWordWrapping = true;
	}

	public TextBlockOptionsEntry(string field, IOptionSpec spec)
		: base(field, spec)
	{
	}

	public override void CreateUIEntry(PGridPanel parent, ref int row)
	{
		parent.AddChild(new PLabel(base.Field)
		{
			Text = OptionsEntry.LookInStrings(base.Title),
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			TextStyle = WRAP_TEXT_STYLE
		}, new GridComponentSpec(row, 0)
		{
			Margin = OptionsEntry.CONTROL_MARGIN,
			Alignment = (TextAnchor)4,
			ColumnSpan = 2
		});
	}

	public override GameObject GetUIComponent()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		return new GameObject("Empty");
	}
}
