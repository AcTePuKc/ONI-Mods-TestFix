using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

public class FloatOptionsEntry : SlidingBaseOptionsEntry
{
	public const string DEFAULT_FORMAT = "F2";

	private GameObject textField;

	private float value;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value is float num)
			{
				this.value = num;
				Update();
			}
		}
	}

	public FloatOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
		: base(field, spec, limit)
	{
		textField = null;
		value = 0f;
	}

	protected override PSliderSingle GetSlider()
	{
		return new PSliderSingle
		{
			OnValueChanged = OnSliderChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			MinValue = (float)limits.Minimum,
			MaxValue = (float)limits.Maximum,
			InitialValue = value
		};
	}

	public override GameObject GetUIComponent()
	{
		textField = new PTextField
		{
			OnTextChanged = OnTextChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			Text = value.ToString(base.Format ?? "F2"),
			MinWidth = 64,
			MaxLength = 16,
			Type = PTextField.FieldType.Float
		}.Build();
		Update();
		return textField;
	}

	private void OnSliderChanged(GameObject _, float newValue)
	{
		if (limits != null)
		{
			value = limits.ClampToRange(newValue);
		}
		else
		{
			value = newValue;
		}
		Update();
	}

	private void OnTextChanged(GameObject _, string text)
	{
		if (float.TryParse(text, out var result))
		{
			if (base.Format != null && base.Format.ToUpperInvariant().IndexOf('P') >= 0)
			{
				result *= 0.01f;
			}
			if (limits != null)
			{
				result = limits.ClampToRange(result);
			}
			value = result;
		}
		Update();
	}

	protected override void Update()
	{
		GameObject obj = textField;
		TMP_InputField val = ((obj != null) ? obj.GetComponentInChildren<TMP_InputField>() : null);
		if ((Object)(object)val != (Object)null)
		{
			val.text = value.ToString(base.Format ?? "F2");
		}
		if ((Object)(object)slider != (Object)null)
		{
			PSliderSingle.SetCurrentValue(slider, value);
		}
	}
}
