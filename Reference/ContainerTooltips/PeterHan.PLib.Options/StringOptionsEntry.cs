using System;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

public class StringOptionsEntry : OptionsEntry
{
	private readonly int maxLength;

	private GameObject textField;

	private string value;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = ((value == null) ? "" : value.ToString());
			Update();
		}
	}

	public StringOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
		: base(field, spec)
	{
		if (limit != null)
		{
			maxLength = Math.Max(2, (int)Math.Round(limit.Maximum));
		}
		else
		{
			maxLength = 256;
		}
		textField = null;
		value = "";
	}

	public override GameObject GetUIComponent()
	{
		textField = new PTextField
		{
			OnTextChanged = OnTextChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			Text = value.ToString(),
			MinWidth = 128,
			Type = PTextField.FieldType.Text,
			MaxLength = maxLength
		}.Build();
		Update();
		return textField;
	}

	private void OnTextChanged(GameObject _, string text)
	{
		value = text;
		Update();
	}

	private void Update()
	{
		TMP_InputField componentInChildren;
		if ((Object)(object)textField != (Object)null && (Object)(object)(componentInChildren = textField.GetComponentInChildren<TMP_InputField>()) != (Object)null)
		{
			componentInChildren.text = value;
		}
	}
}
