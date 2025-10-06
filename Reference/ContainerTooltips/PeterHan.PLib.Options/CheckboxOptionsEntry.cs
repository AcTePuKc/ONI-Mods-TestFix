using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

public class CheckboxOptionsEntry : OptionsEntry
{
	private bool check;

	private GameObject checkbox;

	public override object Value
	{
		get
		{
			return check;
		}
		set
		{
			if (value is bool flag)
			{
				check = flag;
				Update();
			}
		}
	}

	public CheckboxOptionsEntry(string field, IOptionSpec spec)
		: base(field, spec)
	{
		check = false;
		checkbox = null;
	}

	public override GameObject GetUIComponent()
	{
		checkbox = new PCheckBox
		{
			OnChecked = delegate(GameObject source, int state)
			{
				check = state == 0;
				Update();
			},
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip)
		}.SetKleiBlueStyle().Build();
		Update();
		return checkbox;
	}

	private void Update()
	{
		PCheckBox.SetCheckState(checkbox, check ? 1 : 0);
	}
}
