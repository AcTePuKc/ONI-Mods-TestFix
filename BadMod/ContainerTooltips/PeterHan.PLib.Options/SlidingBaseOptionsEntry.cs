using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

public abstract class SlidingBaseOptionsEntry : OptionsEntry
{
	internal static readonly RectOffset ENTRY_MARGIN = new RectOffset(15, 0, 0, 5);

	internal static readonly RectOffset SLIDER_MARGIN = new RectOffset(10, 10, 0, 0);

	protected readonly LimitAttribute limits;

	protected GameObject slider;

	protected SlidingBaseOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
		: base(field, spec)
	{
		limits = limit;
		slider = null;
	}

	public override void CreateUIEntry(PGridPanel parent, ref int row)
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		base.CreateUIEntry(parent, ref row);
		double minimum;
		double maximum;
		if (limits != null && (minimum = limits.Minimum) > -3.4028234663852886E+38 && (maximum = limits.Maximum) < 3.4028234663852886E+38 && maximum > minimum)
		{
			PSliderSingle pSliderSingle = GetSlider().AddOnRealize(OnRealizeSlider);
			PLabel pLabel = new PLabel("MinValue")
			{
				TextStyle = PUITuning.Fonts.TextLightStyle,
				Text = minimum.ToString(base.Format ?? "G3"),
				TextAlignment = (TextAnchor)5
			};
			PLabel pLabel2 = new PLabel("MaxValue")
			{
				TextStyle = PUITuning.Fonts.TextLightStyle,
				Text = maximum.ToString(base.Format ?? "G3"),
				TextAlignment = (TextAnchor)3
			};
			PRelativePanel child = new PRelativePanel("Slider Grid")
			{
				FlexSize = Vector2.right,
				DynamicSize = false
			}.AddChild(pSliderSingle).AddChild(pLabel).AddChild(pLabel2)
				.AnchorYAxis(pSliderSingle)
				.AnchorYAxis(pLabel)
				.AnchorYAxis(pLabel2)
				.SetLeftEdge(pLabel, 0f)
				.SetRightEdge(pLabel2, 1f)
				.SetLeftEdge(pSliderSingle, -1f, pLabel)
				.SetRightEdge(pSliderSingle, -1f, pLabel2)
				.SetMargin(pSliderSingle, SLIDER_MARGIN);
			parent.AddRow(new GridRowSpec());
			parent.AddChild(child, new GridComponentSpec(++row, 0)
			{
				ColumnSpan = 2,
				Margin = ENTRY_MARGIN
			});
		}
	}

	protected abstract PSliderSingle GetSlider();

	protected void OnRealizeSlider(GameObject realized)
	{
		slider = realized;
		Update();
	}

	protected abstract void Update();
}
