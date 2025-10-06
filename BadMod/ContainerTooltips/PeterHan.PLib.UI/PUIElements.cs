using System;
using PeterHan.PLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public sealed class PUIElements
{
	internal static LocText AddLocText(GameObject parent, TextStyleSetting setting = null)
	{
		if ((Object)(object)parent == (Object)null)
		{
			throw new ArgumentNullException("parent");
		}
		bool activeSelf = parent.activeSelf;
		parent.SetActive(false);
		LocText val = parent.AddComponent<LocText>();
		UIDetours.LOCTEXT_KEY.Set(val, string.Empty);
		UIDetours.LOCTEXT_STYLE.Set(val, setting ?? PUITuning.Fonts.UIDarkStyle);
		parent.SetActive(activeSelf);
		return val;
	}

	public static GameObject AddSizeFitter(GameObject uiElement, bool dynamic = false, FitMode modeHoriz = (FitMode)2, FitMode modeVert = (FitMode)2)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)uiElement == (Object)null)
		{
			throw new ArgumentNullException("uiElement");
		}
		if (dynamic)
		{
			ContentSizeFitter obj = EntityTemplateExtensions.AddOrGet<ContentSizeFitter>(uiElement);
			obj.horizontalFit = modeHoriz;
			obj.verticalFit = modeVert;
			((Behaviour)obj).enabled = true;
		}
		else
		{
			FitSizeNow(uiElement, modeHoriz, modeVert);
		}
		return uiElement;
	}

	public static GameObject CreateUI(GameObject parent, string name, bool canvas = true, PUIAnchoring horizAnchor = PUIAnchoring.Stretch, PUIAnchoring vertAnchor = PUIAnchoring.Stretch)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(name);
		if ((Object)(object)parent != (Object)null)
		{
			val.SetParent(parent);
		}
		((Transform)EntityTemplateExtensions.AddOrGet<RectTransform>(val)).localScale = Vector3.one;
		SetAnchors(val, horizAnchor, vertAnchor);
		if (canvas)
		{
			val.AddComponent<CanvasRenderer>();
		}
		val.layer = LayerMask.NameToLayer("UI");
		return val;
	}

	private static void DoNothing()
	{
	}

	private static void FitSizeNow(GameObject uiElement, FitMode modeHoriz, FitMode modeVert)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Invalid comparison between Unknown and I4
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Invalid comparison between Unknown and I4
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Invalid comparison between Unknown and I4
		float val = 0f;
		float val2 = 0f;
		if ((Object)(object)uiElement == (Object)null)
		{
			throw new ArgumentNullException("uiElement");
		}
		ILayoutElement[] components = uiElement.GetComponents<ILayoutElement>();
		LayoutElement val3 = EntityTemplateExtensions.AddOrGet<LayoutElement>(uiElement);
		RectTransform val4 = EntityTemplateExtensions.AddOrGet<RectTransform>(uiElement);
		ILayoutElement[] array = components;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CalculateLayoutInputHorizontal();
		}
		if ((int)modeHoriz != 0)
		{
			array = components;
			foreach (ILayoutElement val5 in array)
			{
				if ((int)modeHoriz != 1)
				{
					if ((int)modeHoriz == 2)
					{
						val = Math.Max(val, val5.preferredWidth);
					}
				}
				else
				{
					val = Math.Max(val, val5.minWidth);
				}
			}
			val = Math.Max(val, val3.minWidth);
			val4.SetSizeWithCurrentAnchors((Axis)0, val);
			val3.minWidth = val;
			val3.flexibleWidth = 0f;
		}
		array = components;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CalculateLayoutInputVertical();
		}
		if ((int)modeVert == 0)
		{
			return;
		}
		array = components;
		foreach (ILayoutElement val6 in array)
		{
			if ((int)modeVert != 1)
			{
				if ((int)modeVert == 2)
				{
					val2 = Math.Max(val2, val6.preferredHeight);
				}
			}
			else
			{
				val2 = Math.Max(val2, val6.minHeight);
			}
		}
		val2 = Math.Max(val2, val3.minHeight);
		val4.SetSizeWithCurrentAnchors((Axis)1, val2);
		val3.minHeight = val2;
		val3.flexibleHeight = 0f;
	}

	public static GameObject SetAnchors(GameObject uiElement, PUIAnchoring horizAnchor, PUIAnchoring vertAnchor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchorMax = default(Vector2);
		Vector2 anchorMin = default(Vector2);
		Vector2 pivot = default(Vector2);
		if ((Object)(object)uiElement == (Object)null)
		{
			throw new ArgumentNullException("uiElement");
		}
		RectTransform val = Util.rectTransform(uiElement);
		switch (horizAnchor)
		{
		case PUIAnchoring.Center:
			anchorMin.x = 0.5f;
			anchorMax.x = 0.5f;
			pivot.x = 0.5f;
			break;
		case PUIAnchoring.End:
			anchorMin.x = 1f;
			anchorMax.x = 1f;
			pivot.x = 1f;
			break;
		case PUIAnchoring.Stretch:
			anchorMin.x = 0f;
			anchorMax.x = 1f;
			pivot.x = 0.5f;
			break;
		default:
			anchorMin.x = 0f;
			anchorMax.x = 0f;
			pivot.x = 0f;
			break;
		}
		switch (vertAnchor)
		{
		case PUIAnchoring.Center:
			anchorMin.y = 0.5f;
			anchorMax.y = 0.5f;
			pivot.y = 0.5f;
			break;
		case PUIAnchoring.End:
			anchorMin.y = 1f;
			anchorMax.y = 1f;
			pivot.y = 1f;
			break;
		case PUIAnchoring.Stretch:
			anchorMin.y = 0f;
			anchorMax.y = 1f;
			pivot.y = 0.5f;
			break;
		default:
			anchorMin.y = 0f;
			anchorMax.y = 0f;
			pivot.y = 0f;
			break;
		}
		val.anchorMax = anchorMax;
		val.anchorMin = anchorMin;
		val.pivot = pivot;
		val.anchoredPosition = Vector2.zero;
		val.offsetMax = Vector2.zero;
		val.offsetMin = Vector2.zero;
		return uiElement;
	}

	public static GameObject SetAnchorOffsets(GameObject uiElement, RectOffset border)
	{
		return SetAnchorOffsets(uiElement, border.left, border.right, border.top, border.bottom);
	}

	public static GameObject SetAnchorOffsets(GameObject uiElement, float left, float right, float top, float bottom)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)uiElement == (Object)null)
		{
			throw new ArgumentNullException("uiElement");
		}
		RectTransform obj = Util.rectTransform(uiElement);
		obj.offsetMin = new Vector2(left, bottom);
		obj.offsetMax = new Vector2(0f - right, 0f - top);
		return uiElement;
	}

	public static GameObject SetText(GameObject uiElement, string text)
	{
		if ((Object)(object)uiElement == (Object)null)
		{
			throw new ArgumentNullException("uiElement");
		}
		TextMeshProUGUI componentInChildren = uiElement.GetComponentInChildren<TextMeshProUGUI>();
		if ((Object)(object)componentInChildren != (Object)null)
		{
			((TMP_Text)componentInChildren).SetText(text ?? string.Empty);
		}
		return uiElement;
	}

	public static GameObject SetToolTip(GameObject uiElement, string tooltip)
	{
		if ((Object)(object)uiElement == (Object)null)
		{
			throw new ArgumentNullException("uiElement");
		}
		if (!string.IsNullOrEmpty(tooltip))
		{
			EntityTemplateExtensions.AddOrGet<ToolTip>(uiElement).toolTip = tooltip;
		}
		return uiElement;
	}

	public static ConfirmDialogScreen ShowConfirmDialog(GameObject parent, string message, Action onConfirm, Action onCancel = null, string confirmText = null, string cancelText = null)
	{
		if ((Object)(object)parent == (Object)null)
		{
			parent = PDialog.GetParentObject();
		}
		GameObject val = Util.KInstantiateUI(((Component)ScreenPrefabs.Instance.ConfirmDialogScreen).gameObject, parent, false);
		ConfirmDialogScreen val2 = default(ConfirmDialogScreen);
		if (val.TryGetComponent<ConfirmDialogScreen>(ref val2))
		{
			UIDetours.POPUP_CONFIRM.Invoke(val2, message, onConfirm, onCancel ?? new Action(DoNothing), null, null, null, confirmText, cancelText);
			val.SetActive(true);
			return val2;
		}
		return null;
	}

	public static ConfirmDialogScreen ShowMessageDialog(GameObject parent, string message)
	{
		return ShowConfirmDialog(parent, message, DoNothing);
	}

	private PUIElements()
	{
	}
}
