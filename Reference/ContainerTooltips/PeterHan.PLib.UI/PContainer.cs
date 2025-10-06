using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public abstract class PContainer : IUIComponent
{
	public Color BackColor { get; set; }

	public Sprite BackImage { get; set; }

	public Vector2 FlexSize { get; set; }

	public Type ImageMode { get; set; }

	public RectOffset Margin { get; set; }

	public string Name { get; protected set; }

	public event PUIDelegates.OnRealize OnRealize;

	protected PContainer(string name)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		BackColor = PUITuning.Colors.Transparent;
		BackImage = null;
		FlexSize = Vector2.zero;
		ImageMode = (Type)0;
		Margin = null;
		Name = name ?? "Container";
	}

	public abstract GameObject Build();

	protected void InvokeRealize(GameObject obj)
	{
		this.OnRealize?.Invoke(obj);
	}

	protected void SetImage(GameObject panel)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (BackColor.a > 0f || (Object)(object)BackImage != (Object)null)
		{
			Image val = panel.AddComponent<Image>();
			((Graphic)val).color = BackColor;
			if ((Object)(object)BackImage != (Object)null)
			{
				val.sprite = BackImage;
				val.type = ImageMode;
			}
		}
	}

	public override string ToString()
	{
		return $"PContainer[Name={Name}]";
	}
}
