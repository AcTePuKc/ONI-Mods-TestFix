using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public sealed class PTextArea : IUIComponent
{
	public Color BackColor { get; set; }

	public Vector2 FlexSize { get; set; }

	public int LineCount { get; set; }

	public int MaxLength { get; set; }

	public string Name { get; }

	public int MinWidth { get; set; }

	public TextAlignmentOptions TextAlignment { get; set; }

	public string Text { get; set; }

	public TextStyleSetting TextStyle { get; set; }

	public string ToolTip { get; set; }

	public PUIDelegates.OnTextChanged OnTextChanged { get; set; }

	public OnValidateInput OnValidate { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

	public PTextArea()
		: this(null)
	{
	}

	public PTextArea(string name)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		BackColor = PUITuning.Colors.BackgroundLight;
		FlexSize = Vector2.one;
		LineCount = 4;
		MaxLength = 1024;
		MinWidth = 64;
		Name = name ?? "TextArea";
		Text = null;
		TextAlignment = (TextAlignmentOptions)257;
		TextStyle = PUITuning.Fonts.TextDarkStyle;
		ToolTip = "";
	}

	public PTextArea AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		OnRealize += onRealize;
		return this;
	}

	public GameObject Build()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(null, Name);
		TextStyleSetting val2 = TextStyle ?? PUITuning.Fonts.TextLightStyle;
		Image obj = val.AddComponent<Image>();
		obj.sprite = PUITuning.Images.BoxBorderWhite;
		obj.type = (Type)1;
		((Graphic)obj).color = val2.textColor;
		GameObject val3 = PUIElements.CreateUI(val, "Text Area", canvas: false);
		((Graphic)val3.AddComponent<Image>()).color = BackColor;
		RectMask2D obj2 = val3.AddComponent<RectMask2D>();
		GameObject val4 = PUIElements.CreateUI(val3, "Text");
		TextMeshProUGUI val5 = PTextField.ConfigureField(val4.AddComponent<TextMeshProUGUI>(), val2, TextAlignment);
		((TMP_Text)val5).enableWordWrapping = true;
		((Graphic)val5).raycastTarget = true;
		val.SetActive(false);
		TMP_InputField val6 = val.AddComponent<TMP_InputField>();
		val6.textComponent = (TMP_Text)(object)val5;
		val6.textViewport = Util.rectTransform(val3);
		val6.text = Text ?? "";
		((TMP_Text)val5).text = Text ?? "";
		ConfigureTextEntry(val6);
		PTextFieldEvents pTextFieldEvents = val.AddComponent<PTextFieldEvents>();
		pTextFieldEvents.OnTextChanged = OnTextChanged;
		pTextFieldEvents.OnValidate = OnValidate;
		pTextFieldEvents.TextObject = val4;
		PUIElements.SetToolTip(val, ToolTip);
		((Behaviour)obj2).enabled = true;
		PUIElements.SetAnchorOffsets(val4, new RectOffset());
		val.SetActive(true);
		LayoutElement obj3 = EntityTemplateExtensions.AddOrGet<LayoutElement>(PUIUtils.InsetChild(val, val3, Vector2.one, new Vector2((float)MinWidth, (float)Math.Max(LineCount, 1) * PUIUtils.GetLineHeight(val2))));
		obj3.flexibleWidth = FlexSize.x;
		obj3.flexibleHeight = FlexSize.y;
		this.OnRealize?.Invoke(val);
		return val;
	}

	private void ConfigureTextEntry(TMP_InputField textEntry)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		textEntry.characterLimit = Math.Max(1, MaxLength);
		((Behaviour)textEntry).enabled = true;
		textEntry.inputType = (InputType)0;
		((Selectable)textEntry).interactable = true;
		textEntry.isRichTextEditingAllowed = false;
		textEntry.keyboardType = (TouchScreenKeyboardType)0;
		textEntry.lineType = (LineType)2;
		((Selectable)textEntry).navigation = Navigation.defaultNavigation;
		textEntry.richText = false;
		textEntry.selectionColor = PUITuning.Colors.SelectionBackground;
		((Selectable)textEntry).transition = (Transition)0;
		textEntry.restoreOriginalTextOnEscape = true;
	}

	public PTextArea SetKleiPinkStyle()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		TextStyle = PUITuning.Fonts.UILightStyle;
		BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
		return this;
	}

	public PTextArea SetKleiBlueStyle()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		TextStyle = PUITuning.Fonts.UILightStyle;
		BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
		return this;
	}

	public PTextArea SetMinWidthInCharacters(int chars)
	{
		int num = Mathf.RoundToInt((float)chars * PUIUtils.GetEmWidth(TextStyle));
		if (num > 0)
		{
			MinWidth = num;
		}
		return this;
	}

	public override string ToString()
	{
		return $"PTextArea[Name={Name}]";
	}
}
