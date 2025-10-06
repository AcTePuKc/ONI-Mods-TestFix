using System;
using UnityEngine;

namespace PeterHan.PLib.UI;

[Serializable]
public sealed class BoxLayoutParams
{
	public TextAnchor Alignment { get; set; }

	public PanelDirection Direction { get; set; }

	public RectOffset Margin { get; set; }

	public float Spacing { get; set; }

	public BoxLayoutParams()
	{
		Alignment = (TextAnchor)4;
		Direction = PanelDirection.Horizontal;
		Margin = null;
		Spacing = 0f;
	}

	public override string ToString()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return $"BoxLayoutParams[Alignment={Alignment},Direction={Direction},Spacing={Spacing:F2}]";
	}
}
