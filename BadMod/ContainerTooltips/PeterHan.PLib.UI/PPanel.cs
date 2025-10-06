using System;
using System.Collections.Generic;
using PeterHan.PLib.Core;
using UnityEngine;

namespace PeterHan.PLib.UI;

public class PPanel : PContainer, IDynamicSizable, IUIComponent
{
	protected readonly ICollection<IUIComponent> children;

	public TextAnchor Alignment { get; set; }

	public PanelDirection Direction { get; set; }

	public bool DynamicSize { get; set; }

	public int Spacing { get; set; }

	public PPanel()
		: this(null)
	{
	}

	public PPanel(string name)
		: base(name ?? "Panel")
	{
		Alignment = (TextAnchor)4;
		children = new List<IUIComponent>();
		Direction = PanelDirection.Vertical;
		DynamicSize = true;
		Spacing = 0;
	}

	public PPanel AddChild(IUIComponent child)
	{
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		children.Add(child);
		return this;
	}

	public PPanel AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		base.OnRealize += onRealize;
		return this;
	}

	public override GameObject Build()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return Build(default(Vector2), DynamicSize);
	}

	private GameObject Build(Vector2 size, bool dynamic)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(null, base.Name);
		SetImage(val);
		foreach (IUIComponent child in children)
		{
			GameObject obj = child.Build();
			obj.SetParent(val);
			PUIElements.SetAnchors(obj, PUIAnchoring.Stretch, PUIAnchoring.Stretch);
		}
		BoxLayoutGroup boxLayoutGroup = val.AddComponent<BoxLayoutGroup>();
		boxLayoutGroup.Params = new BoxLayoutParams
		{
			Direction = Direction,
			Alignment = Alignment,
			Spacing = Spacing,
			Margin = base.Margin
		};
		if (!dynamic)
		{
			boxLayoutGroup.LockLayout();
			val.SetMinUISize(size);
		}
		boxLayoutGroup.flexibleWidth = base.FlexSize.x;
		boxLayoutGroup.flexibleHeight = base.FlexSize.y;
		InvokeRealize(val);
		return val;
	}

	public GameObject BuildWithFixedSize(Vector2 size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Build(size, dynamic: false);
	}

	public PPanel RemoveChild(IUIComponent child)
	{
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		children.Remove(child);
		return this;
	}

	public PPanel SetKleiBlueColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		base.BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
		return this;
	}

	public PPanel SetKleiPinkColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		base.BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
		return this;
	}

	public override string ToString()
	{
		return $"PPanel[Name={base.Name},Direction={Direction}]";
	}
}
