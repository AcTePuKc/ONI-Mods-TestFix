using System;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

public class LogFloatOptionsEntry : SlidingBaseOptionsEntry
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
				this.value = limits.ClampToRange(num);
				Update();
			}
		}
	}

	public LogFloatOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit)
		: base(field, spec, limit)
	{
		if (limit == null)
		{
			throw new ArgumentNullException("limit");
		}
		if (limit.Minimum <= 0.0 || limit.Maximum <= 0.0)
		{
			throw new ArgumentOutOfRangeException("limit", "Logarithmic values must be positive");
		}
		textField = null;
		value = limit.ClampToRange(1f);
	}

	protected override PSliderSingle GetSlider()
	{
		return new PSliderSingle
		{
			OnValueChanged = OnSliderChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			MinValue = Mathf.Log((float)limits.Minimum),
			MaxValue = Mathf.Log((float)limits.Maximum),
			InitialValue = Mathf.Log(value)
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
		value = limits.ClampToRange(Mathf.Exp(newValue));
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
			value = limits.ClampToRange(result);
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
			PSliderSingle.SetCurrentValue(slider, Mathf.Log(Mathf.Max(float.Epsilon, value)));
		}
	}
}
