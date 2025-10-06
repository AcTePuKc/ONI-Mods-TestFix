using System;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public class PCheckBox : PTextComponent
{
	private const float CHECKBOX_MARGIN = 2f;

	public const int STATE_UNCHECKED = 0;

	public const int STATE_CHECKED = 1;

	public const int STATE_PARTIAL = 2;

	public ColorStyleSetting CheckColor { get; set; }

	public Color BackColor { get; set; }

	public Vector2 CheckSize { get; set; }

	public Color ComponentBackColor { get; set; }

	public int InitialState { get; set; }

	public PUIDelegates.OnChecked OnChecked { get; set; }

	private static ToggleState[] GenerateStates(ColorStyleSetting imageColor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		StatePresentationSetting val = new StatePresentationSetting
		{
			color = imageColor.activeColor,
			use_color_on_hover = true,
			color_on_hover = imageColor.hoverColor,
			image_target = null,
			name = "Partial"
		};
		return (ToggleState[])(object)new ToggleState[3]
		{
			new ToggleState
			{
				color = PUITuning.Colors.Transparent,
				color_on_hover = PUITuning.Colors.Transparent,
				sprite = null,
				use_color_on_hover = false,
				additional_display_settings = (StatePresentationSetting[])(object)new StatePresentationSetting[1]
				{
					new StatePresentationSetting
					{
						color = imageColor.activeColor,
						use_color_on_hover = false,
						image_target = null,
						name = "Unchecked"
					}
				}
			},
			new ToggleState
			{
				color = imageColor.activeColor,
				color_on_hover = imageColor.hoverColor,
				sprite = PUITuning.Images.Checked,
				use_color_on_hover = true,
				additional_display_settings = (StatePresentationSetting[])(object)new StatePresentationSetting[1] { val }
			},
			new ToggleState
			{
				color = imageColor.activeColor,
				color_on_hover = imageColor.hoverColor,
				sprite = PUITuning.Images.Partial,
				use_color_on_hover = true,
				additional_display_settings = (StatePresentationSetting[])(object)new StatePresentationSetting[1] { val }
			}
		};
	}

	public static int GetCheckState(GameObject realized)
	{
		int result = 0;
		if ((Object)(object)realized == (Object)null)
		{
			throw new ArgumentNullException("realized");
		}
		MultiToggle componentInChildren = realized.GetComponentInChildren<MultiToggle>();
		if ((Object)(object)componentInChildren != (Object)null)
		{
			result = UIDetours.CURRENT_STATE.Get(componentInChildren);
		}
		return result;
	}

	public static void SetCheckState(GameObject realized, int state)
	{
		if ((Object)(object)realized == (Object)null)
		{
			throw new ArgumentNullException("realized");
		}
		MultiToggle componentInChildren = realized.GetComponentInChildren<MultiToggle>();
		if ((Object)(object)componentInChildren != (Object)null && UIDetours.CURRENT_STATE.Get(componentInChildren) != state)
		{
			UIDetours.CHANGE_STATE.Invoke(componentInChildren, state);
		}
	}

	public PCheckBox()
		: this(null)
	{
	}

	public PCheckBox(string name)
		: base(name ?? "CheckBox")
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		BackColor = PUITuning.Colors.BackgroundLight;
		CheckColor = null;
		CheckSize = new Vector2(16f, 16f);
		ComponentBackColor = PUITuning.Colors.Transparent;
		base.IconSpacing = 3;
		InitialState = 0;
		base.Sprite = null;
		base.Text = null;
		base.ToolTip = "";
	}

	public PCheckBox AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		base.OnRealize += onRealize;
		return this;
	}

	public override GameObject Build()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		GameObject checkbox = PUIElements.CreateUI(null, base.Name);
		Vector2 actualSize = CheckSize;
		GameObject sprite = null;
		GameObject text = null;
		if (ComponentBackColor.a > 0f)
		{
			((Graphic)checkbox.AddComponent<Image>()).color = ComponentBackColor;
		}
		ColorStyleSetting val = CheckColor ?? PUITuning.Colors.ComponentLightStyle;
		GameObject val2 = PUIElements.CreateUI(checkbox, "CheckBox");
		((Graphic)val2.AddComponent<Image>()).color = BackColor;
		Image toggle_image = CreateCheckImage(val2, val, ref actualSize);
		val2.SetUISize(new Vector2(actualSize.x + 4f, actualSize.y + 4f), addLayout: true);
		if ((Object)(object)base.Sprite != (Object)null)
		{
			sprite = ((Component)PTextComponent.ImageChildHelper(checkbox, this)).gameObject;
		}
		if (!string.IsNullOrEmpty(base.Text))
		{
			text = ((Component)PTextComponent.TextChildHelper(checkbox, base.TextStyle ?? PUITuning.Fonts.UILightStyle, base.Text)).gameObject;
		}
		MultiToggle mToggle = checkbox.AddComponent<MultiToggle>();
		PUIDelegates.OnChecked evt = OnChecked;
		if (evt != null)
		{
			MultiToggle obj = mToggle;
			obj.onClick = (Action)Delegate.Combine(obj.onClick, (Action)delegate
			{
				evt(checkbox, mToggle.CurrentState);
			});
		}
		UIDetours.PLAY_SOUND_CLICK.Set(mToggle, arg2: true);
		UIDetours.PLAY_SOUND_RELEASE.Set(mToggle, arg2: false);
		mToggle.states = GenerateStates(val);
		mToggle.toggle_image = toggle_image;
		UIDetours.CHANGE_STATE.Invoke(mToggle, InitialState);
		PUIElements.SetToolTip(checkbox, base.ToolTip).SetActive(true);
		GameObject text2 = WrapTextAndSprite(text, sprite);
		RelativeLayoutGroup relativeLayoutGroup = checkbox.AddComponent<RelativeLayoutGroup>();
		relativeLayoutGroup.Margin = base.Margin;
		PTextComponent.ArrangeComponent(relativeLayoutGroup, WrapTextAndSprite(text2, val2), base.TextAlignment);
		if (!base.DynamicSize)
		{
			relativeLayoutGroup.LockLayout();
		}
		relativeLayoutGroup.flexibleWidth = base.FlexSize.x;
		relativeLayoutGroup.flexibleHeight = base.FlexSize.y;
		DestroyLayoutIfPossible(checkbox);
		InvokeRealize(checkbox);
		return checkbox;
	}

	private Image CreateCheckImage(GameObject checkbox, ColorStyleSetting color, ref Vector2 actualSize)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		Image obj = PUIElements.CreateUI(checkbox, "CheckBorder").AddComponent<Image>();
		obj.sprite = PUITuning.Images.CheckBorder;
		((Graphic)obj).color = color.activeColor;
		obj.type = (Type)1;
		GameObject val = PUIElements.CreateUI(checkbox, "CheckMark", canvas: true, PUIAnchoring.Center, PUIAnchoring.Center);
		Image obj2 = val.AddComponent<Image>();
		obj2.sprite = PUITuning.Images.Checked;
		obj2.preserveAspect = true;
		if (actualSize.x <= 0f || actualSize.y <= 0f)
		{
			RectTransform val2 = Util.rectTransform(val);
			actualSize.x = LayoutUtility.GetPreferredWidth(val2);
			actualSize.y = LayoutUtility.GetPreferredHeight(val2);
		}
		val.SetUISize(CheckSize);
		return obj2;
	}

	public PCheckBox SetKleiPinkStyle()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.TextStyle = PUITuning.Fonts.UILightStyle;
		BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
		CheckColor = PUITuning.Colors.ComponentDarkStyle;
		return this;
	}

	public PCheckBox SetKleiBlueStyle()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.TextStyle = PUITuning.Fonts.UILightStyle;
		BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
		CheckColor = PUITuning.Colors.ComponentDarkStyle;
		return this;
	}
}
