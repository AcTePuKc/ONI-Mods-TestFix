using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public sealed class PDialog : IUIComponent
{
	private sealed class DialogButton
	{
		public readonly ColorStyleSetting backColor;

		public readonly string key;

		public readonly string text;

		public readonly TextStyleSetting textColor;

		public readonly string tooltip;

		internal DialogButton(string key, string text, string tooltip, ColorStyleSetting backColor, TextStyleSetting foreColor)
		{
			this.backColor = backColor;
			this.key = key;
			this.text = text;
			this.tooltip = tooltip;
			textColor = foreColor;
		}

		public override string ToString()
		{
			return $"DialogButton[key={key:D},text={text:D}]";
		}
	}

	private sealed class PDialogComp : KScreen
	{
		internal PDialog dialog;

		internal string key;

		internal float sortKey;

		internal PDialogComp()
		{
			key = "close";
			sortKey = 0f;
		}

		internal void DoButton(GameObject source)
		{
			key = ((Object)source).name;
			UIDetours.DEACTIVATE_KSCREEN.Invoke((KScreen)(object)this);
		}

		public override float GetSortKey()
		{
			return sortKey;
		}

		protected override void OnDeactivate()
		{
			if (dialog != null)
			{
				dialog.DialogClosed?.Invoke(key);
			}
			((KScreen)this).OnDeactivate();
			dialog = null;
		}

		public override void OnKeyDown(KButtonEvent e)
		{
			if (e.TryConsume((Action)1))
			{
				UIDetours.DEACTIVATE_KSCREEN.Invoke((KScreen)(object)this);
			}
			else
			{
				((KScreen)this).OnKeyDown(e);
			}
		}
	}

	public static readonly RectOffset BUTTON_MARGIN = new RectOffset(13, 13, 13, 13);

	private static readonly RectOffset CLOSE_ICON_MARGIN = new RectOffset(4, 4, 4, 4);

	private static readonly Vector2 CLOSE_ICON_SIZE = Vector2f.op_Implicit(new Vector2f(16f, 16f));

	public const string DIALOG_KEY_CLOSE = "close";

	private readonly ICollection<DialogButton> buttons;

	public PPanel Body { get; }

	public Color DialogBackColor { get; set; }

	public Vector2 MaxSize { get; set; }

	public string Name { get; }

	public GameObject Parent { get; set; }

	public bool RoundToNearestEven { get; set; }

	public Vector2 Size { get; set; }

	public float SortKey { get; set; }

	public string Title { get; set; }

	public PUIDelegates.OnDialogClosed DialogClosed { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

	public static GameObject GetParentObject()
	{
		GameObject result = null;
		FrontEndManager instance = FrontEndManager.Instance;
		if ((Object)(object)instance != (Object)null)
		{
			result = ((Component)instance).gameObject;
		}
		else
		{
			GameScreenManager instance2 = GameScreenManager.Instance;
			if ((Object)(object)instance2 != (Object)null)
			{
				result = instance2.ssOverlayCanvas;
			}
			else
			{
				PUIUtils.LogUIWarning("No dialog parent found!");
			}
		}
		return result;
	}

	private static float RoundUpSize(float size, float maxSize)
	{
		int num = Mathf.CeilToInt(size);
		if (num % 2 == 1)
		{
			num++;
		}
		if ((float)num > maxSize && maxSize > 0f)
		{
			num -= 2;
		}
		return num;
	}

	public PDialog(string name)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Body = new PPanel("Body")
		{
			Alignment = (TextAnchor)1,
			FlexSize = Vector2.one,
			Margin = new RectOffset(6, 6, 6, 6)
		};
		DialogBackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
		buttons = new List<DialogButton>(4);
		MaxSize = Vector2.zero;
		Name = name ?? "Dialog";
		Parent = GetParentObject();
		RoundToNearestEven = false;
		Size = Vector2.zero;
		SortKey = 0f;
		Title = "Dialog";
	}

	public PDialog AddButton(string key, string text, string tooltip = null)
	{
		buttons.Add(new DialogButton(key, text, tooltip, null, null));
		return this;
	}

	public PDialog AddButton(string key, string text, string tooltip = null, ColorStyleSetting backColor = null, TextStyleSetting foreColor = null)
	{
		buttons.Add(new DialogButton(key, text, tooltip, backColor, foreColor));
		return this;
	}

	public PDialog AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		OnRealize += onRealize;
		return this;
	}

	public GameObject Build()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		if ((Object)(object)Parent == (Object)null)
		{
			throw new InvalidOperationException("Parent for dialog may not be null");
		}
		GameObject val = PUIElements.CreateUI(Parent, Name);
		PDialogComp pDialogComp = val.AddComponent<PDialogComp>();
		val.AddComponent<Canvas>();
		val.AddComponent<GraphicRaycaster>();
		Image obj = val.AddComponent<Image>();
		((Graphic)obj).color = DialogBackColor;
		obj.sprite = PUITuning.Images.BoxBorder;
		obj.type = (Type)1;
		PGridLayoutGroup pGridLayoutGroup = val.AddComponent<PGridLayoutGroup>();
		pGridLayoutGroup.AddRow(new GridRowSpec());
		pGridLayoutGroup.AddRow(new GridRowSpec(0f, 1f));
		pGridLayoutGroup.AddRow(new GridRowSpec());
		pGridLayoutGroup.AddColumn(new GridColumnSpec(0f, 1f));
		pGridLayoutGroup.AddColumn(new GridColumnSpec());
		LayoutTitle(pGridLayoutGroup, pDialogComp.DoButton);
		pGridLayoutGroup.AddComponent(Body.Build(), new GridComponentSpec(1, 0)
		{
			ColumnSpan = 2,
			Margin = new RectOffset(10, 10, 10, 10)
		});
		CreateUserButtons(pGridLayoutGroup, pDialogComp.DoButton);
		SetDialogSize(val);
		pDialogComp.dialog = this;
		pDialogComp.sortKey = SortKey;
		this.OnRealize?.Invoke(val);
		return val;
	}

	private void CreateUserButtons(PGridLayoutGroup layout, PUIDelegates.OnButtonPressed onPressed)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		PPanel pPanel = new PPanel("Buttons")
		{
			Alignment = (TextAnchor)7,
			Spacing = 7,
			Direction = PanelDirection.Horizontal,
			Margin = new RectOffset(5, 5, 0, 10)
		};
		int num = 0;
		foreach (DialogButton button in buttons)
		{
			string key = button.key;
			ColorStyleSetting backColor = button.backColor;
			TextStyleSetting textStyle = button.textColor ?? PUITuning.Fonts.UILightStyle;
			PButton pButton = new PButton(key)
			{
				Text = button.text,
				ToolTip = button.tooltip,
				Margin = BUTTON_MARGIN,
				OnClick = onPressed,
				Color = backColor,
				TextStyle = textStyle
			};
			if ((Object)(object)backColor == (Object)null)
			{
				if (++num >= buttons.Count)
				{
					pButton.SetKleiPinkStyle();
				}
				else
				{
					pButton.SetKleiBlueStyle();
				}
			}
			pPanel.AddChild(pButton);
		}
		layout.AddComponent(pPanel.Build(), new GridComponentSpec(2, 0)
		{
			ColumnSpan = 2
		});
	}

	private void LayoutTitle(PGridLayoutGroup layout, PUIDelegates.OnButtonPressed onClose)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new PLabel("Title")
		{
			Margin = new RectOffset(3, 4, 0, 0),
			Text = Title,
			FlexSize = Vector2.one
		}.SetKleiPinkColor().Build();
		layout.AddComponent(val, new GridComponentSpec(0, 0)
		{
			Margin = new RectOffset(0, -2, 0, 0)
		});
		Image obj = EntityTemplateExtensions.AddOrGet<Image>(val);
		obj.sprite = PUITuning.Images.BoxBorder;
		obj.type = (Type)1;
		layout.AddComponent(new PButton("close")
		{
			Sprite = PUITuning.Images.Close,
			Margin = CLOSE_ICON_MARGIN,
			OnClick = onClose,
			SpriteSize = CLOSE_ICON_SIZE,
			ToolTip = LocString.op_Implicit(TOOLTIPS.CLOSETOOLTIP)
		}.SetKleiBlueStyle().Build(), new GridComponentSpec(0, 1));
	}

	private void SetDialogSize(GameObject dialog)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = Util.rectTransform(dialog);
		LayoutRebuilder.ForceRebuildLayoutImmediate(val);
		float num = Math.Max(Size.x, LayoutUtility.GetPreferredWidth(val));
		float num2 = Math.Max(Size.y, LayoutUtility.GetPreferredHeight(val));
		float x = MaxSize.x;
		float y = MaxSize.y;
		if (x > 0f)
		{
			num = Math.Min(num, x);
		}
		if (y > 0f)
		{
			num2 = Math.Min(num2, y);
		}
		if (RoundToNearestEven)
		{
			num = RoundUpSize(num, x);
			num2 = RoundUpSize(num2, y);
		}
		val.SetSizeWithCurrentAnchors((Axis)0, num);
		val.SetSizeWithCurrentAnchors((Axis)1, num2);
	}

	public void Show()
	{
		KScreen obj = default(KScreen);
		if (Build().TryGetComponent<KScreen>(ref obj))
		{
			UIDetours.ACTIVATE_KSCREEN.Invoke(obj);
		}
	}

	public override string ToString()
	{
		return $"PDialog[Name={Name},Title={Title}]";
	}
}
