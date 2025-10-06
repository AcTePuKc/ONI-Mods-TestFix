using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

public class IntOptionsEntry : SlidingBaseOptionsEntry
{
	private GameObject textField;

	private int value;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value is int num)
			{
				this.value = num;
				Update();
			}
		}
	}

	public IntOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
		: base(field, spec, limit)
	{
		textField = null;
		value = 0;
	}

	protected override PSliderSingle GetSlider()
	{
		return new PSliderSingle
		{
			OnValueChanged = OnSliderChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			MinValue = (float)limits.Minimum,
			MaxValue = (float)limits.Maximum,
			InitialValue = value,
			IntegersOnly = true
		};
	}

	public override GameObject GetUIComponent()
	{
		textField = new PTextField
		{
			OnTextChanged = OnTextChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			Text = value.ToString(base.Format ?? "D"),
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
		if (int.TryParse(text, out var result))
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
			val.text = value.ToString(base.Format ?? "D");
		}
		if ((Object)(object)slider != (Object)null)
		{
			PSliderSingle.SetCurrentValue(slider, value);
		}
	}
}
