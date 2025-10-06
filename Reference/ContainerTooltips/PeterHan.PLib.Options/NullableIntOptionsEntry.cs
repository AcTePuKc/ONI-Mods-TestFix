using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

public class NullableIntOptionsEntry : SlidingBaseOptionsEntry
{
	private GameObject textField;

	private int? value;

	protected virtual string FieldText => value?.ToString(base.Format ?? "D") ?? string.Empty;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value == null)
			{
				this.value = null;
				Update();
			}
			else if (value is int num)
			{
				this.value = num;
				Update();
			}
		}
	}

	public NullableIntOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
		: base(field, spec, limit)
	{
		textField = null;
		value = null;
	}

	protected override PSliderSingle GetSlider()
	{
		float num = (float)limits.Minimum;
		float maxValue = (float)limits.Maximum;
		return new PSliderSingle
		{
			OnValueChanged = OnSliderChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			MinValue = num,
			MaxValue = maxValue,
			InitialValue = num,
			IntegersOnly = true
		};
	}

	public override GameObject GetUIComponent()
	{
		textField = new PTextField
		{
			OnTextChanged = OnTextChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			Text = FieldText,
			MinWidth = 64,
			MaxLength = 10,
			Type = PTextField.FieldType.Integer
		}.Build();
		Update();
		return textField;
	}

	private void OnSliderChanged(GameObject _, float newValue)
	{
		int num = Mathf.RoundToInt(newValue);
		if (limits != null)
		{
			num = limits.ClampToRange(num);
		}
		value = num;
		Update();
	}

	private void OnTextChanged(GameObject _, string text)
	{
		int result;
		if (string.IsNullOrWhiteSpace(text))
		{
			value = null;
		}
		else if (int.TryParse(text, out result))
		{
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
			val.text = FieldText;
		}
		if ((Object)(object)slider != (Object)null && value.HasValue)
		{
			PSliderSingle.SetCurrentValue(slider, value.Value);
		}
	}
}
