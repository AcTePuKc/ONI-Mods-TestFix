using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public sealed class PComboBox<T> : IUIComponent, IDynamicSizable where T : class, IListableOption
{
	private static readonly RectOffset DEFAULT_ITEM_MARGIN = new RectOffset(3, 3, 3, 3);

	public Vector2 ArrowSize { get; set; }

	public ColorStyleSetting BackColor { get; set; }

	public Vector2 CheckSize { get; set; }

	public IEnumerable<T> Content { get; set; }

	public bool DynamicSize { get; set; }

	public ColorStyleSetting EntryColor { get; set; }

	public Vector2 FlexSize { get; set; }

	public T InitialItem { get; set; }

	public RectOffset ItemMargin { get; set; }

	public RectOffset Margin { get; set; }

	public int MaxRowsShown { get; set; }

	public int MinWidth { get; set; }

	public string Name { get; }

	public PUIDelegates.OnDropdownChanged<T> OnOptionSelected { get; set; }

	public TextAnchor TextAlignment { get; set; }

	public TextStyleSetting TextStyle { get; set; }

	public string ToolTip { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

	public static void SetSelectedItem(GameObject realized, IListableOption option, bool fireListener = false)
	{
		PComboBoxComponent pComboBoxComponent = default(PComboBoxComponent);
		if (option != null && (Object)(object)realized != (Object)null && realized.TryGetComponent<PComboBoxComponent>(ref pComboBoxComponent))
		{
			pComboBoxComponent.SetSelectedItem(option, fireListener);
		}
	}

	public PComboBox()
		: this("Dropdown")
	{
	}

	public PComboBox(string name)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		ArrowSize = new Vector2(8f, 8f);
		BackColor = null;
		CheckSize = new Vector2(12f, 12f);
		Content = null;
		DynamicSize = false;
		FlexSize = Vector2.zero;
		InitialItem = null;
		ItemMargin = DEFAULT_ITEM_MARGIN;
		Margin = PButton.BUTTON_MARGIN;
		MaxRowsShown = 6;
		MinWidth = 0;
		Name = name;
		TextAlignment = (TextAnchor)3;
		TextStyle = null;
		ToolTip = null;
	}

	public PComboBox<T> AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		OnRealize += onRealize;
		return this;
	}

	public GameObject Build()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Expected O, but got Unknown
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(null, Name);
		TextStyleSetting val2 = TextStyle ?? PUITuning.Fonts.UILightStyle;
		ColorStyleSetting val3 = EntryColor ?? PUITuning.Colors.ButtonBlueStyle;
		RectOffset margin = Margin;
		RectOffset itemMargin = ItemMargin;
		KImage val4 = val.AddComponent<KImage>();
		ColorStyleSetting arg = BackColor ?? PUITuning.Colors.ButtonBlueStyle;
		UIDetours.COLOR_STYLE_SETTING.Set(val4, arg);
		PButton.SetupButtonBackground(val4);
		GameObject val5 = PUIElements.CreateUI(val, "SelectedItem");
		if (MinWidth > 0)
		{
			val5.SetMinUISize(new Vector2((float)MinWidth, 0f));
		}
		LocText selectedLabel = PUIElements.AddLocText(val5, val2);
		GameObject val6 = PUIElements.CreateUI(null, "Content");
		((HorizontalOrVerticalLayoutGroup)val6.AddComponent<VerticalLayoutGroup>()).childForceExpandWidth = true;
		GameObject val7 = new PScrollPane("PullDown")
		{
			ScrollHorizontal = false,
			ScrollVertical = true,
			AlwaysShowVertical = true,
			FlexSize = Vector2.right,
			TrackSize = 8f,
			BackColor = val3.inactiveColor
		}.BuildScrollPane(val, val6);
		Util.rectTransform(val7).pivot = new Vector2(0.5f, 1f);
		PComboBoxComponent pComboBoxComponent = val.AddComponent<PComboBoxComponent>();
		pComboBoxComponent.CheckColor = val2.textColor;
		pComboBoxComponent.ContentContainer = Util.rectTransform(val6);
		pComboBoxComponent.EntryPrefab = BuildRowPrefab(val2, val3);
		pComboBoxComponent.MaxRowsShown = MaxRowsShown;
		pComboBoxComponent.Pulldown = val7;
		pComboBoxComponent.SelectedLabel = (TMP_Text)(object)selectedLabel;
		pComboBoxComponent.SetItems((IEnumerable<IListableOption>)Content);
		pComboBoxComponent.SetSelectedItem((IListableOption)(object)InitialItem);
		pComboBoxComponent.OnSelectionChanged = delegate(PComboBoxComponent obj, IListableOption item)
		{
			OnOptionSelected?.Invoke(((Component)obj).gameObject, item as T);
		};
		GameObject val8 = PUIElements.CreateUI(val, "OpenImage");
		Image val9 = val8.AddComponent<Image>();
		val9.sprite = PUITuning.Images.Contract;
		((Graphic)val9).color = val2.textColor;
		KButton val10 = val.AddComponent<KButton>();
		PButton.SetupButton(val10, val4);
		UIDetours.FG_IMAGE.Set(val10, val9);
		val10.onClick += pComboBoxComponent.OnClick;
		PUIElements.SetToolTip(val5, ToolTip);
		val.SetActive(true);
		RelativeLayoutGroup relativeLayoutGroup = val.AddComponent<RelativeLayoutGroup>();
		relativeLayoutGroup.AnchorYAxis(val5).SetLeftEdge(val5, 0f).SetRightEdge(val5, -1f, val8)
			.AnchorYAxis(val8)
			.SetRightEdge(val8, 1f)
			.SetMargin(val5, new RectOffset(margin.left, itemMargin.right, margin.top, margin.bottom))
			.SetMargin(val8, new RectOffset(0, margin.right, margin.top, margin.bottom))
			.OverrideSize(val8, ArrowSize)
			.AnchorYAxis(val7, 0f)
			.OverrideSize(val7, Vector2.up);
		relativeLayoutGroup.LockLayout();
		if (DynamicSize)
		{
			relativeLayoutGroup.UnlockLayout();
		}
		EntityTemplateExtensions.AddOrGet<LayoutElement>(val7).ignoreLayout = true;
		val7.SetActive(false);
		relativeLayoutGroup.flexibleWidth = FlexSize.x;
		relativeLayoutGroup.flexibleHeight = FlexSize.y;
		this.OnRealize?.Invoke(val);
		return val;
	}

	private GameObject BuildRowPrefab(TextStyleSetting style, ColorStyleSetting entryColor)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected O, but got Unknown
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		RectOffset itemMargin = ItemMargin;
		GameObject obj = PUIElements.CreateUI(null, "RowEntry");
		KImage val = obj.AddComponent<KImage>();
		UIDetours.COLOR_STYLE_SETTING.Set(val, entryColor);
		UIDetours.APPLY_COLOR_STYLE.Invoke(val);
		GameObject val2 = PUIElements.CreateUI(obj, "Selected");
		Image val3 = val2.AddComponent<Image>();
		((Graphic)val3).color = style.textColor;
		val3.preserveAspect = true;
		val3.sprite = PUITuning.Images.Checked;
		KButton val4 = obj.AddComponent<KButton>();
		PButton.SetupButton(val4, val);
		UIDetours.FG_IMAGE.Set(val4, val3);
		obj.AddComponent<ToolTip>();
		GameObject val5 = PUIElements.CreateUI(obj, "Text");
		((TMP_Text)PUIElements.AddLocText(val5, style)).SetText(" ");
		obj.AddComponent<RelativeLayoutGroup>().AnchorYAxis(val2).OverrideSize(val2, CheckSize)
			.SetLeftEdge(val2, 0f)
			.SetMargin(val2, itemMargin)
			.AnchorYAxis(val5)
			.SetLeftEdge(val5, -1f, val2)
			.SetRightEdge(val5, 1f)
			.SetMargin(val5, new RectOffset(0, itemMargin.right, itemMargin.top, itemMargin.bottom))
			.LockLayout();
		obj.SetActive(false);
		return obj;
	}

	public PComboBox<T> SetKleiPinkStyle()
	{
		TextStyle = PUITuning.Fonts.UILightStyle;
		BackColor = PUITuning.Colors.ButtonPinkStyle;
		return this;
	}

	public PComboBox<T> SetKleiBlueStyle()
	{
		TextStyle = PUITuning.Fonts.UILightStyle;
		BackColor = PUITuning.Colors.ButtonBlueStyle;
		return this;
	}

	public PComboBox<T> SetMinWidthInCharacters(int chars)
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
		return $"PComboBox[Name={Name}]";
	}
}
