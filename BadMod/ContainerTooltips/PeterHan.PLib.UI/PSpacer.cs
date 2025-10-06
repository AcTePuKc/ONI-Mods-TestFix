using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public class PSpacer : IUIComponent
{
	public Vector2 FlexSize { get; set; }

	public Vector2 PreferredSize { get; set; }

	public string Name { get; }

	public event PUIDelegates.OnRealize OnRealize;

	public PSpacer()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Name = "Spacer";
		FlexSize = Vector2.one;
		PreferredSize = Vector2.zero;
	}

	public GameObject Build()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(Name);
		LayoutElement obj = val.AddComponent<LayoutElement>();
		obj.flexibleHeight = FlexSize.y;
		obj.flexibleWidth = FlexSize.x;
		obj.minHeight = 0f;
		obj.minWidth = 0f;
		obj.preferredHeight = PreferredSize.y;
		obj.preferredWidth = PreferredSize.x;
		this.OnRealize?.Invoke(val);
		return val;
	}

	public override string ToString()
	{
		return "PSpacer";
	}
}
