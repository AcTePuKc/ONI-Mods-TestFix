using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public sealed class PToggle : IDynamicSizable, IUIComponent
{
	internal static readonly RectOffset TOGGLE_MARGIN = new RectOffset(1, 1, 1, 1);

	public Sprite ActiveSprite { get; set; }

	public ColorStyleSetting Color { get; set; }

	public bool DynamicSize { get; set; }

	public Vector2 FlexSize { get; set; }

	public Sprite InactiveSprite { get; set; }

	public bool InitialState { get; set; }

	public RectOffset Margin { get; set; }

	public string Name { get; }

	public PUIDelegates.OnToggleButton OnStateChanged { get; set; }

	public Vector2 Size { get; set; }

	public string ToolTip { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

	public static bool GetToggleState(GameObject realized)
	{
		KToggle arg = default(KToggle);
		if ((Object)(object)realized != (Object)null && realized.TryGetComponent<KToggle>(ref arg))
		{
			return UIDetours.IS_ON.Get(arg);
		}
		return false;
	}

	public static void SetToggleState(GameObject realized, bool on)
	{
		KToggle arg = default(KToggle);
		if ((Object)(object)realized != (Object)null && realized.TryGetComponent<KToggle>(ref arg))
		{
			UIDetours.IS_ON.Set(arg, on);
		}
	}

	public PToggle()
		: this(null)
	{
	}

	public PToggle(string name)
	{
		ActiveSprite = PUITuning.Images.Contract;
		Color = PUITuning.Colors.ComponentDarkStyle;
		InitialState = false;
		Margin = TOGGLE_MARGIN;
		Name = name ?? "Toggle";
		InactiveSprite = PUITuning.Images.Expand;
		ToolTip = "";
	}

	public PToggle AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		OnRealize += onRealize;
		return this;
	}

	public GameObject Build()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		GameObject toggle = PUIElements.CreateUI(null, Name);
		KToggle val = toggle.AddComponent<KToggle>();
		PUIDelegates.OnToggleButton evt = OnStateChanged;
		if (evt != null)
		{
			val.onValueChanged += delegate(bool on)
			{
				evt?.Invoke(toggle, on);
			};
		}
		UIDetours.ART_EXTENSION.Set(val, new KToggleArtExtensions());
		UIDetours.SOUND_PLAYER_TOGGLE.Set(val, PUITuning.ToggleSounds);
		Image val2 = toggle.AddComponent<Image>();
		((Graphic)val2).color = Color.activeColor;
		val2.sprite = InactiveSprite;
		toggle.SetActive(false);
		ImageToggleState obj = toggle.AddComponent<ImageToggleState>();
		obj.TargetImage = val2;
		obj.useSprites = true;
		obj.InactiveSprite = InactiveSprite;
		obj.ActiveSprite = ActiveSprite;
		obj.startingState = (State)((!InitialState) ? 1 : 2);
		obj.useStartingState = true;
		obj.ActiveColour = Color.activeColor;
		obj.DisabledActiveColour = Color.disabledActiveColor;
		obj.InactiveColour = Color.inactiveColor;
		obj.DisabledColour = Color.disabledColor;
		obj.HoverColour = Color.hoverColor;
		obj.DisabledHoverColor = Color.disabledhoverColor;
		UIDetours.IS_ON.Set(val, InitialState);
		toggle.SetActive(true);
		if (Size.x > 0f && Size.y > 0f)
		{
			toggle.SetUISize(Size, addLayout: true);
		}
		else
		{
			PUIElements.AddSizeFitter(toggle, DynamicSize, (FitMode)2, (FitMode)2);
		}
		PUIElements.SetToolTip(toggle, ToolTip).SetFlexUISize(FlexSize).SetActive(true);
		this.OnRealize?.Invoke(toggle);
		return toggle;
	}

	public override string ToString()
	{
		return $"PToggle[Name={Name}]";
	}
}
