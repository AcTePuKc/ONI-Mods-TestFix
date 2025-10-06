using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.Options;

internal abstract class ColorBaseOptionsEntry : OptionsEntry
{
	protected static readonly RectOffset ENTRY_MARGIN = new RectOffset(10, 10, 2, 5);

	protected static readonly RectOffset SLIDER_MARGIN = new RectOffset(10, 0, 2, 2);

	protected const float SWATCH_SIZE = 32f;

	protected ColorGradient hueGradient;

	protected KSlider hueSlider;

	protected TMP_InputField blue;

	protected TMP_InputField green;

	protected TMP_InputField red;

	protected ColorGradient satGradient;

	protected KSlider satSlider;

	protected Image swatch;

	protected ColorGradient valGradient;

	protected KSlider valSlider;

	protected Color value;

	protected ColorBaseOptionsEntry(string field, IOptionSpec spec)
		: base(field, spec)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		value = Color.white;
		blue = null;
		green = null;
		red = null;
		hueGradient = null;
		hueSlider = null;
		satGradient = null;
		satSlider = null;
		valGradient = null;
		valSlider = null;
		swatch = null;
	}

	public override void CreateUIEntry(PGridPanel parent, ref int row)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		base.CreateUIEntry(parent, ref row);
		parent.AddRow(new GridRowSpec());
		PSliderSingle pSliderSingle = new PSliderSingle("Hue")
		{
			ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_HUE),
			MinValue = 0f,
			MaxValue = 1f,
			CustomTrack = true,
			FlexSize = Vector2.right,
			OnValueChanged = OnHueChanged
		}.AddOnRealize(OnHueRealized);
		PSliderSingle pSliderSingle2 = new PSliderSingle("Saturation")
		{
			ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_SATURATION),
			MinValue = 0f,
			MaxValue = 1f,
			CustomTrack = true,
			FlexSize = Vector2.right,
			OnValueChanged = OnSatChanged
		}.AddOnRealize(OnSatRealized);
		PSliderSingle pSliderSingle3 = new PSliderSingle("Value")
		{
			ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_VALUE),
			MinValue = 0f,
			MaxValue = 1f,
			CustomTrack = true,
			FlexSize = Vector2.right,
			OnValueChanged = OnValChanged
		}.AddOnRealize(OnValRealized);
		PLabel pLabel = new PLabel("Swatch")
		{
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			DynamicSize = false,
			Sprite = PUITuning.Images.BoxBorder,
			SpriteMode = (Type)1,
			SpriteSize = new Vector2(32f, 32f)
		}.AddOnRealize(OnSwatchRealized);
		PRelativePanel child = new PRelativePanel("ColorPicker")
		{
			FlexSize = Vector2.right,
			DynamicSize = false
		}.AddChild(pSliderSingle).AddChild(pSliderSingle2).AddChild(pSliderSingle3)
			.AddChild(pLabel)
			.SetRightEdge(pSliderSingle, 1f)
			.SetRightEdge(pSliderSingle2, 1f)
			.SetRightEdge(pSliderSingle3, 1f)
			.SetLeftEdge(pLabel, 0f)
			.SetMargin(pSliderSingle, SLIDER_MARGIN)
			.SetMargin(pSliderSingle2, SLIDER_MARGIN)
			.SetMargin(pSliderSingle3, SLIDER_MARGIN)
			.AnchorYAxis(pLabel)
			.SetLeftEdge(pSliderSingle, -1f, pLabel)
			.SetLeftEdge(pSliderSingle2, -1f, pLabel)
			.SetLeftEdge(pSliderSingle3, -1f, pLabel)
			.SetTopEdge(pSliderSingle, 1f)
			.SetBottomEdge(pSliderSingle3, 0f)
			.SetTopEdge(pSliderSingle2, -1f, pSliderSingle)
			.SetTopEdge(pSliderSingle3, -1f, pSliderSingle2);
		parent.AddChild(child, new GridComponentSpec(++row, 0)
		{
			ColumnSpan = 2,
			Margin = ENTRY_MARGIN
		});
	}

	public override GameObject GetUIComponent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Color32 val = Color32.op_Implicit(value);
		GameObject result = new PPanel("RGB")
		{
			DynamicSize = false,
			Alignment = (TextAnchor)5,
			Spacing = 5,
			Direction = PanelDirection.Horizontal
		}.AddChild(new PLabel("Red")
		{
			TextStyle = PUITuning.Fonts.TextLightStyle,
			Text = LocString.op_Implicit(PLibStrings.LABEL_R)
		}).AddChild(new PTextField("RedValue")
		{
			OnTextChanged = OnRGBChanged,
			ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_RED),
			Text = val.r.ToString(),
			MinWidth = 32,
			MaxLength = 3,
			Type = PTextField.FieldType.Integer
		}.AddOnRealize(OnRedRealized)).AddChild(new PLabel("Green")
		{
			TextStyle = PUITuning.Fonts.TextLightStyle,
			Text = LocString.op_Implicit(PLibStrings.LABEL_G)
		})
			.AddChild(new PTextField("GreenValue")
			{
				OnTextChanged = OnRGBChanged,
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_GREEN),
				Text = val.g.ToString(),
				MinWidth = 32,
				MaxLength = 3,
				Type = PTextField.FieldType.Integer
			}.AddOnRealize(OnGreenRealized))
			.AddChild(new PLabel("Blue")
			{
				TextStyle = PUITuning.Fonts.TextLightStyle,
				Text = LocString.op_Implicit(PLibStrings.LABEL_B)
			})
			.AddChild(new PTextField("BlueValue")
			{
				OnTextChanged = OnRGBChanged,
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_BLUE),
				Text = val.b.ToString(),
				MinWidth = 32,
				MaxLength = 3,
				Type = PTextField.FieldType.Integer
			}.AddOnRealize(OnBlueRealized))
			.Build();
		UpdateAll();
		return result;
	}

	private void OnBlueRealized(GameObject realized)
	{
		blue = realized.GetComponentInChildren<TMP_InputField>();
	}

	private void OnGreenRealized(GameObject realized)
	{
		green = realized.GetComponentInChildren<TMP_InputField>();
	}

	private void OnHueChanged(GameObject _, float newHue)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hueGradient != (Object)null && (Object)(object)hueSlider != (Object)null)
		{
			float a = value.a;
			hueGradient.Position = ((Slider)hueSlider).value;
			value = hueGradient.SelectedColor;
			value.a = a;
			UpdateRGB();
			UpdateSat(moveSlider: false);
			UpdateVal(moveSlider: false);
		}
	}

	private void OnHueRealized(GameObject realized)
	{
		hueGradient = EntityTemplateExtensions.AddOrGet<ColorGradient>(realized);
		realized.TryGetComponent<KSlider>(ref hueSlider);
	}

	private void OnRedRealized(GameObject realized)
	{
		red = realized.GetComponentInChildren<TMP_InputField>();
	}

	protected void OnRGBChanged(GameObject _, string text)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Color32 val = Color32.op_Implicit(value);
		if (byte.TryParse(red.text, out var result))
		{
			val.r = result;
		}
		if (byte.TryParse(green.text, out var result2))
		{
			val.g = result2;
		}
		if (byte.TryParse(blue.text, out var result3))
		{
			val.b = result3;
		}
		value = Color32.op_Implicit(val);
		UpdateAll();
	}

	private void OnSatChanged(GameObject _, float newSat)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)satGradient != (Object)null && (Object)(object)satSlider != (Object)null)
		{
			float a = value.a;
			satGradient.Position = ((Slider)satSlider).value;
			value = satGradient.SelectedColor;
			value.a = a;
			UpdateRGB();
			UpdateHue(moveSlider: false);
			UpdateVal(moveSlider: false);
		}
	}

	private void OnSatRealized(GameObject realized)
	{
		satGradient = EntityTemplateExtensions.AddOrGet<ColorGradient>(realized);
		realized.TryGetComponent<KSlider>(ref satSlider);
	}

	private void OnSwatchRealized(GameObject realized)
	{
		swatch = realized.GetComponentInChildren<Image>();
	}

	private void OnValChanged(GameObject _, float newValue)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)valGradient != (Object)null && (Object)(object)valSlider != (Object)null)
		{
			float a = value.a;
			valGradient.Position = ((Slider)valSlider).value;
			value = valGradient.SelectedColor;
			value.a = a;
			UpdateRGB();
			UpdateHue(moveSlider: false);
			UpdateSat(moveSlider: false);
		}
	}

	private void OnValRealized(GameObject realized)
	{
		valGradient = EntityTemplateExtensions.AddOrGet<ColorGradient>(realized);
		realized.TryGetComponent<KSlider>(ref valSlider);
	}

	protected void UpdateAll()
	{
		UpdateRGB();
		UpdateHue(moveSlider: true);
		UpdateSat(moveSlider: true);
		UpdateVal(moveSlider: true);
	}

	protected void UpdateHue(bool moveSlider)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hueGradient != (Object)null && (Object)(object)hueSlider != (Object)null)
		{
			float num = default(float);
			float num2 = default(float);
			float num3 = default(float);
			Color.RGBToHSV(value, ref num, ref num2, ref num3);
			hueGradient.SetRange(0f, 1f, num2, num2, num3, num3);
			hueGradient.SelectedColor = value;
			if (moveSlider)
			{
				((Slider)hueSlider).value = hueGradient.Position;
			}
		}
	}

	protected void UpdateRGB()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		Color32 val = Color32.op_Implicit(value);
		if ((Object)(object)red != (Object)null)
		{
			red.text = val.r.ToString();
		}
		if ((Object)(object)green != (Object)null)
		{
			green.text = val.g.ToString();
		}
		if ((Object)(object)blue != (Object)null)
		{
			blue.text = val.b.ToString();
		}
		if ((Object)(object)swatch != (Object)null)
		{
			((Graphic)swatch).color = value;
		}
	}

	protected void UpdateSat(bool moveSlider)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)satGradient != (Object)null && (Object)(object)satSlider != (Object)null)
		{
			float num = default(float);
			float num2 = default(float);
			float num3 = default(float);
			Color.RGBToHSV(value, ref num, ref num2, ref num3);
			satGradient.SetRange(num, num, 0f, 1f, num3, num3);
			satGradient.SelectedColor = value;
			if (moveSlider)
			{
				((Slider)satSlider).value = satGradient.Position;
			}
		}
	}

	protected void UpdateVal(bool moveSlider)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)valGradient != (Object)null && (Object)(object)valSlider != (Object)null)
		{
			float num = default(float);
			float num2 = default(float);
			float num3 = default(float);
			Color.RGBToHSV(value, ref num, ref num2, ref num3);
			valGradient.SetRange(num, num, num2, num2, 0f, 1f);
			valGradient.SelectedColor = value;
			if (moveSlider)
			{
				((Slider)valSlider).value = valGradient.Position;
			}
		}
	}
}
