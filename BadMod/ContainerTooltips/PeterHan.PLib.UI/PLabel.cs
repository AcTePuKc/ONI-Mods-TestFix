using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public class PLabel : PTextComponent
{
	public Color BackColor { get; set; }

	public PLabel()
		: this(null)
	{
	}

	public PLabel(string name)
		: base(name ?? "Label")
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		BackColor = PUITuning.Colors.Transparent;
	}

	public PLabel AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		base.OnRealize += onRealize;
		return this;
	}

	public override GameObject Build()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(null, base.Name);
		GameObject sprite = null;
		GameObject text = null;
		if (BackColor.a > 0f)
		{
			((Graphic)val.AddComponent<Image>()).color = BackColor;
		}
		if ((Object)(object)base.Sprite != (Object)null)
		{
			sprite = ((Component)PTextComponent.ImageChildHelper(val, this)).gameObject;
		}
		if (!string.IsNullOrEmpty(base.Text))
		{
			text = ((Component)PTextComponent.TextChildHelper(val, base.TextStyle ?? PUITuning.Fonts.UILightStyle, base.Text)).gameObject;
		}
		PUIElements.SetToolTip(val, base.ToolTip).SetActive(true);
		RelativeLayoutGroup relativeLayoutGroup = val.AddComponent<RelativeLayoutGroup>();
		relativeLayoutGroup.Margin = base.Margin;
		PTextComponent.ArrangeComponent(relativeLayoutGroup, WrapTextAndSprite(text, sprite), base.TextAlignment);
		if (!base.DynamicSize)
		{
			relativeLayoutGroup.LockLayout();
		}
		relativeLayoutGroup.flexibleWidth = base.FlexSize.x;
		relativeLayoutGroup.flexibleHeight = base.FlexSize.y;
		DestroyLayoutIfPossible(val);
		InvokeRealize(val);
		return val;
	}

	public PLabel SetKleiBlueColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
		return this;
	}

	public PLabel SetKleiPinkColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
		return this;
	}
}
