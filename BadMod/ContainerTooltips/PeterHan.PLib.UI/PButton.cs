using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public class PButton : PTextComponent
{
	internal static readonly RectOffset BUTTON_MARGIN = new RectOffset(7, 7, 5, 5);

	public ColorStyleSetting Color { get; set; }

	public PUIDelegates.OnButtonPressed OnClick { get; set; }

	internal static void SetupButton(KButton button, KImage bgImage)
	{
		UIDetours.ADDITIONAL_K_IMAGES.Set(button, (KImage[])(object)new KImage[0]);
		UIDetours.SOUND_PLAYER_BUTTON.Set(button, PUITuning.ButtonSounds);
		UIDetours.BG_IMAGE.Set(button, bgImage);
	}

	internal static void SetupButtonBackground(KImage bgImage)
	{
		UIDetours.APPLY_COLOR_STYLE.Invoke(bgImage);
		((Image)bgImage).sprite = PUITuning.Images.ButtonBorder;
		((Image)bgImage).type = (Type)1;
	}

	public static void SetButtonEnabled(GameObject obj, bool enabled)
	{
		KButton arg = default(KButton);
		if ((Object)(object)obj != (Object)null && obj.TryGetComponent<KButton>(ref arg))
		{
			UIDetours.IS_INTERACTABLE.Set(arg, enabled);
		}
	}

	public PButton()
		: this(null)
	{
	}

	public PButton(string name)
		: base(name ?? "Button")
	{
		base.Margin = BUTTON_MARGIN;
		base.Sprite = null;
		base.Text = null;
		base.ToolTip = "";
	}

	public PButton AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		base.OnRealize += onRealize;
		return this;
	}

	public override GameObject Build()
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		GameObject button = PUIElements.CreateUI(null, base.Name);
		GameObject sprite = null;
		GameObject text = null;
		KImage val = button.AddComponent<KImage>();
		ColorStyleSetting arg = Color ?? PUITuning.Colors.ButtonPinkStyle;
		UIDetours.COLOR_STYLE_SETTING.Set(val, arg);
		SetupButtonBackground(val);
		KButton val2 = button.AddComponent<KButton>();
		PUIDelegates.OnButtonPressed evt = OnClick;
		if (evt != null)
		{
			val2.onClick += delegate
			{
				evt?.Invoke(button);
			};
		}
		SetupButton(val2, val);
		if ((Object)(object)base.Sprite != (Object)null)
		{
			Image val3 = PTextComponent.ImageChildHelper(button, this);
			UIDetours.FG_IMAGE.Set(val2, val3);
			sprite = ((Component)val3).gameObject;
		}
		if (!string.IsNullOrEmpty(base.Text))
		{
			text = ((Component)PTextComponent.TextChildHelper(button, base.TextStyle ?? PUITuning.Fonts.UILightStyle, base.Text)).gameObject;
		}
		PUIElements.SetToolTip(button, base.ToolTip).SetActive(true);
		RelativeLayoutGroup relativeLayoutGroup = button.AddComponent<RelativeLayoutGroup>();
		relativeLayoutGroup.Margin = base.Margin;
		PTextComponent.ArrangeComponent(relativeLayoutGroup, WrapTextAndSprite(text, sprite), base.TextAlignment);
		if (!base.DynamicSize)
		{
			relativeLayoutGroup.LockLayout();
		}
		relativeLayoutGroup.flexibleWidth = base.FlexSize.x;
		relativeLayoutGroup.flexibleHeight = base.FlexSize.y;
		DestroyLayoutIfPossible(button);
		InvokeRealize(button);
		return button;
	}

	public PButton SetImageLeftArrow()
	{
		base.Sprite = PUITuning.Images.Arrow;
		base.SpriteTransform = ImageTransform.FlipHorizontal;
		return this;
	}

	public PButton SetImageRightArrow()
	{
		base.Sprite = PUITuning.Images.Arrow;
		base.SpriteTransform = ImageTransform.None;
		return this;
	}

	public PButton SetKleiPinkStyle()
	{
		base.TextStyle = PUITuning.Fonts.UILightStyle;
		Color = PUITuning.Colors.ButtonPinkStyle;
		return this;
	}

	public PButton SetKleiBlueStyle()
	{
		base.TextStyle = PUITuning.Fonts.UILightStyle;
		Color = PUITuning.Colors.ButtonBlueStyle;
		return this;
	}
}
