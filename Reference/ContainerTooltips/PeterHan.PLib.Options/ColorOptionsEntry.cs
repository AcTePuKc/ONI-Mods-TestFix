using UnityEngine;

namespace PeterHan.PLib.Options;

internal class ColorOptionsEntry : ColorBaseOptionsEntry
{
	public override object Value
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return value;
		}
		set
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			if (value is Color val)
			{
				base.value = val;
				UpdateAll();
			}
		}
	}

	public ColorOptionsEntry(string field, IOptionSpec spec)
		: base(field, spec)
	{
	}
}
