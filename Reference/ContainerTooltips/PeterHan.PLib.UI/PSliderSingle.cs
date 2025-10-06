using System;
using PeterHan.PLib.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public class PSliderSingle : IUIComponent
{
	public bool CustomTrack { get; set; }

	public Direction Direction { get; set; }

	public Vector2 FlexSize { get; set; }

	public ColorStyleSetting HandleColor { get; set; }

	public float HandleSize { get; set; }

	public float InitialValue { get; set; }

	public bool IntegersOnly { get; set; }

	public float MaxValue { get; set; }

	public float MinValue { get; set; }

	public string Name { get; }

	public PUIDelegates.OnSliderDrag OnDrag { get; set; }

	public float PreferredLength { get; set; }

	public PUIDelegates.OnSliderChanged OnValueChanged { get; set; }

	public string ToolTip { get; set; }

	public float TrackSize { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

	public static void SetCurrentValue(GameObject realized, float value)
	{
		KSlider val = default(KSlider);
		if ((Object)(object)realized != (Object)null && realized.TryGetComponent<KSlider>(ref val) && !value.IsNaNOrInfinity())
		{
			((Slider)val).value = value.InRange(((Slider)val).minValue, ((Slider)val).maxValue);
		}
	}

	public PSliderSingle()
		: this("SliderSingle")
	{
	}

	public PSliderSingle(string name)
	{
		CustomTrack = false;
		Direction = (Direction)0;
		HandleColor = PUITuning.Colors.ButtonPinkStyle;
		HandleSize = 16f;
		InitialValue = 0.5f;
		IntegersOnly = false;
		MaxValue = 1f;
		MinValue = 0f;
		Name = name;
		PreferredLength = 100f;
		TrackSize = 12f;
	}

	public PSliderSingle AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		OnRealize += onRealize;
		return this;
	}

	public GameObject Build()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Invalid comparison between Unknown and I4
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		if (MaxValue.IsNaNOrInfinity())
		{
			throw new ArgumentException("MaxValue");
		}
		if (MinValue.IsNaNOrInfinity())
		{
			throw new ArgumentException("MinValue");
		}
		if (MaxValue <= MinValue)
		{
			throw new ArgumentOutOfRangeException("MaxValue");
		}
		GameObject slider = PUIElements.CreateUI(null, Name);
		bool flag = (int)Direction == 2 || (int)Direction == 3;
		ColorStyleSetting val = HandleColor ?? PUITuning.Colors.ButtonBlueStyle;
		slider.SetActive(false);
		if (!CustomTrack)
		{
			Image obj = slider.AddComponent<Image>();
			obj.sprite = (flag ? PUITuning.Images.ScrollBorderVertical : PUITuning.Images.ScrollBorderHorizontal);
			obj.type = (Type)1;
		}
		GameObject val2 = PUIElements.CreateUI(slider, "Fill");
		if (!CustomTrack)
		{
			Image obj2 = val2.AddComponent<Image>();
			obj2.sprite = (flag ? PUITuning.Images.ScrollHandleVertical : PUITuning.Images.ScrollHandleHorizontal);
			((Graphic)obj2).color = val.inactiveColor;
			obj2.type = (Type)1;
		}
		PUIElements.SetAnchorOffsets(val2, 1f, 1f, 1f, 1f);
		KSlider ks = slider.AddComponent<KSlider>();
		((Slider)ks).maxValue = MaxValue;
		((Slider)ks).minValue = MinValue;
		((Slider)ks).value = (InitialValue.IsNaNOrInfinity() ? MinValue : InitialValue.InRange(MinValue, MaxValue));
		((Slider)ks).wholeNumbers = IntegersOnly;
		((Slider)ks).handleRect = Util.rectTransform(CreateHandle(slider));
		((Slider)ks).fillRect = Util.rectTransform(val2);
		((Slider)ks).SetDirection(Direction, true);
		if (OnValueChanged != null)
		{
			((UnityEvent<float>)(object)((Slider)ks).onValueChanged).AddListener((UnityAction<float>)delegate(float value)
			{
				OnValueChanged(slider, value);
			});
		}
		if (OnDrag != null)
		{
			ks.onDrag += delegate
			{
				OnDrag(slider, ((Slider)ks).value);
			};
		}
		string tt = ToolTip;
		if (!string.IsNullOrEmpty(tt))
		{
			ToolTip obj3 = slider.AddComponent<ToolTip>();
			obj3.OnToolTip = () => string.Format(tt, ((Slider)ks).value);
			obj3.refreshWhileHovering = true;
		}
		slider.SetActive(true);
		slider.SetMinUISize(flag ? new Vector2(TrackSize, PreferredLength) : new Vector2(PreferredLength, TrackSize));
		slider.SetFlexUISize(FlexSize);
		this.OnRealize?.Invoke(slider);
		return slider;
	}

	private GameObject CreateHandle(GameObject slider)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected I4, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(slider, "Handle", canvas: true, PUIAnchoring.Center, PUIAnchoring.Center);
		Image obj = val.AddComponent<Image>();
		obj.sprite = PUITuning.Images.SliderHandle;
		obj.preserveAspect = true;
		val.SetUISize(new Vector2(HandleSize, HandleSize));
		float num = 0f;
		Direction direction = Direction;
		switch (direction - 1)
		{
		case 2:
			num = 90f;
			break;
		case 0:
			num = 180f;
			break;
		case 1:
			num = 270f;
			break;
		}
		if (num != 0f)
		{
			val.transform.Rotate(new Vector3(0f, 0f, num));
		}
		return val;
	}

	public PSliderSingle SetKleiPinkStyle()
	{
		HandleColor = PUITuning.Colors.ButtonPinkStyle;
		return this;
	}

	public PSliderSingle SetKleiBlueStyle()
	{
		HandleColor = PUITuning.Colors.ButtonBlueStyle;
		return this;
	}

	public override string ToString()
	{
		return $"PSliderSingle[Name={Name}]";
	}
}
