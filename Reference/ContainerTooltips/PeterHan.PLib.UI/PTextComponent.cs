using PeterHan.PLib.Core;
using PeterHan.PLib.UI.Layouts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public abstract class PTextComponent : IDynamicSizable, IUIComponent
{
	private static readonly Vector2 CENTER = new Vector2(0.5f, 0.5f);

	public bool DynamicSize { get; set; }

	public Vector2 FlexSize { get; set; }

	public int IconSpacing { get; set; }

	public bool MaintainSpriteAspect { get; set; }

	public RectOffset Margin { get; set; }

	public string Name { get; }

	public Sprite Sprite { get; set; }

	public Type SpriteMode { get; set; }

	public TextAnchor SpritePosition { get; set; }

	public Vector2 SpriteSize { get; set; }

	public Color SpriteTint { get; set; }

	public ImageTransform SpriteTransform { get; set; }

	public string Text { get; set; }

	public TextAnchor TextAlignment { get; set; }

	public TextStyleSetting TextStyle { get; set; }

	public string ToolTip { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

	protected static void ArrangeComponent(RelativeLayoutGroup layout, GameObject target, TextAnchor alignment)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected I4, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		switch ((int)alignment)
		{
		case 0:
		case 3:
		case 6:
			layout.SetLeftEdge(target, 0f);
			break;
		case 2:
		case 5:
		case 8:
			layout.SetRightEdge(target, 1f);
			break;
		default:
			layout.AnchorXAxis(target);
			break;
		}
		if ((int)alignment > 2)
		{
			if (alignment - 6 <= 2)
			{
				layout.SetBottomEdge(target, 0f);
			}
			else
			{
				layout.AnchorYAxis(target);
			}
		}
		else
		{
			layout.SetTopEdge(target, 1f);
		}
	}

	protected static Image ImageChildHelper(GameObject parent, PTextComponent settings)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(parent, "Image", canvas: true, PUIAnchoring.Beginning, PUIAnchoring.Beginning);
		Util.rectTransform(val).pivot = CENTER;
		Image obj = val.AddComponent<Image>();
		((Graphic)obj).color = settings.SpriteTint;
		obj.sprite = settings.Sprite;
		obj.type = settings.SpriteMode;
		obj.preserveAspect = settings.MaintainSpriteAspect;
		Vector3 one = Vector3.one;
		float num = 0f;
		ImageTransform spriteTransform = settings.SpriteTransform;
		if ((spriteTransform & ImageTransform.FlipHorizontal) != ImageTransform.None)
		{
			one.x = -1f;
		}
		if ((spriteTransform & ImageTransform.FlipVertical) != ImageTransform.None)
		{
			one.y = -1f;
		}
		if ((spriteTransform & ImageTransform.Rotate90) != ImageTransform.None)
		{
			num = 90f;
		}
		if ((spriteTransform & ImageTransform.Rotate180) != ImageTransform.None)
		{
			num += 180f;
		}
		RectTransform obj2 = Util.rectTransform(val);
		((Transform)obj2).localScale = one;
		((Transform)obj2).Rotate(new Vector3(0f, 0f, num));
		Vector2 spriteSize = settings.SpriteSize;
		if (spriteSize.x > 0f && spriteSize.y > 0f)
		{
			val.SetUISize(spriteSize, addLayout: true);
		}
		return obj;
	}

	protected static LocText TextChildHelper(GameObject parent, TextStyleSetting style, string contents = "")
	{
		LocText obj = PUIElements.AddLocText(PUIElements.CreateUI(parent, "Text"), style);
		((TMP_Text)obj).alignment = (TextAlignmentOptions)514;
		((TMP_Text)obj).text = contents;
		return obj;
	}

	protected PTextComponent(string name)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		DynamicSize = false;
		FlexSize = Vector2.zero;
		IconSpacing = 0;
		MaintainSpriteAspect = true;
		Margin = null;
		Name = name;
		Sprite = null;
		SpriteMode = (Type)0;
		SpritePosition = (TextAnchor)3;
		SpriteSize = Vector2.zero;
		SpriteTint = Color.white;
		SpriteTransform = ImageTransform.None;
		Text = null;
		TextAlignment = (TextAnchor)4;
		TextStyle = null;
		ToolTip = "";
	}

	public abstract GameObject Build();

	protected void DestroyLayoutIfPossible(GameObject component)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (FlexSize.x == 0f && FlexSize.y == 0f && !DynamicSize)
		{
			AbstractLayoutGroup.DestroyAndReplaceLayout(component);
		}
	}

	protected void InvokeRealize(GameObject obj)
	{
		this.OnRealize?.Invoke(obj);
	}

	public override string ToString()
	{
		return string.Format("{3}[Name={0},Text={1},Sprite={2}]", Name, Text, Sprite, GetType().Name);
	}

	protected GameObject WrapTextAndSprite(GameObject text, GameObject sprite)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected I4, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Invalid comparison between Unknown and I4
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Invalid comparison between Unknown and I4
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = null;
		if ((Object)(object)text != (Object)null && (Object)(object)sprite != (Object)null)
		{
			val = PUIElements.CreateUI(text.GetParent(), "AlignmentWrapper");
			text.SetParent(val);
			sprite.SetParent(val);
			RelativeLayoutGroup relativeLayoutGroup = EntityTemplateExtensions.AddOrGet<RelativeLayoutGroup>(val);
			TextAnchor spritePosition = SpritePosition;
			switch ((int)spritePosition)
			{
			case 0:
			case 3:
			case 6:
				relativeLayoutGroup.SetLeftEdge(sprite, 0f).SetLeftEdge(text, -1f, sprite).SetMargin(sprite, new RectOffset(0, IconSpacing, 0, 0));
				break;
			case 2:
			case 5:
			case 8:
				relativeLayoutGroup.SetRightEdge(sprite, 1f).SetRightEdge(text, -1f, sprite).SetMargin(sprite, new RectOffset(IconSpacing, 0, 0, 0));
				break;
			default:
				relativeLayoutGroup.AnchorXAxis(text).AnchorXAxis(sprite);
				break;
			}
			spritePosition = SpritePosition;
			if ((int)spritePosition > 2)
			{
				if (spritePosition - 6 <= 2)
				{
					relativeLayoutGroup.SetBottomEdge(sprite, 0f).SetBottomEdge(text, -1f, sprite).SetMargin(sprite, new RectOffset(0, 0, IconSpacing, 0));
				}
				else
				{
					relativeLayoutGroup.AnchorYAxis(text).AnchorYAxis(sprite);
				}
			}
			else
			{
				relativeLayoutGroup.SetTopEdge(sprite, 1f).SetTopEdge(text, -1f, sprite).SetMargin(sprite, new RectOffset(0, 0, 0, IconSpacing));
			}
			if (!DynamicSize)
			{
				relativeLayoutGroup.LockLayout();
			}
		}
		else if ((Object)(object)text != (Object)null)
		{
			val = text;
		}
		else if ((Object)(object)sprite != (Object)null)
		{
			val = sprite;
		}
		return val;
	}
}
