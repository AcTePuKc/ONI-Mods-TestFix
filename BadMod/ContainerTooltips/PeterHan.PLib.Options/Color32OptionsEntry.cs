using UnityEngine;

namespace PeterHan.PLib.Options;

internal class Color32OptionsEntry : ColorBaseOptionsEntry
{
	public override object Value
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Color32.op_Implicit(value);
		}
		set
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value is Color32 val)
			{
				base.value = Color32.op_Implicit(val);
				UpdateAll();
			}
		}
	}

	public Color32OptionsEntry(string field, IOptionSpec spec)
		: base(field, spec)
	{
	}
}
