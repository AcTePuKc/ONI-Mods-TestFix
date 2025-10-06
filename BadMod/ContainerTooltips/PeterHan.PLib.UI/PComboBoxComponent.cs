using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

internal sealed class PComboBoxComponent : KMonoBehaviour
{
	private struct ComboBoxItem
	{
		public readonly IListableOption data;

		public readonly Image rowImage;

		public readonly GameObject rowInstance;

		public ComboBoxItem(IListableOption data, GameObject rowInstance)
		{
			this.data = data;
			this.rowInstance = rowInstance;
			rowImage = KMonoBehaviourExtensions.GetComponentInChildrenOnly<Image>(rowInstance);
		}
	}

	private sealed class MouseEventHandler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public bool IsOver { get; private set; }

		internal MouseEventHandler()
		{
			IsOver = true;
		}

		public void OnPointerEnter(PointerEventData data)
		{
			IsOver = true;
		}

		public void OnPointerExit(PointerEventData data)
		{
			IsOver = false;
		}
	}

	private readonly IList<ComboBoxItem> currentItems;

	private MouseEventHandler handler;

	private bool open;

	internal RectTransform ContentContainer { get; set; }

	internal Color CheckColor { get; set; }

	internal GameObject EntryPrefab { get; set; }

	internal int MaxRowsShown { get; set; }

	internal Action<PComboBoxComponent, IListableOption> OnSelectionChanged { get; set; }

	internal GameObject Pulldown { get; set; }

	internal TMP_Text SelectedLabel { get; set; }

	internal PComboBoxComponent()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		CheckColor = Color.white;
		currentItems = new List<ComboBoxItem>(32);
		handler = null;
		MaxRowsShown = 8;
		open = false;
	}

	public void Close()
	{
		GameObject pulldown = Pulldown;
		if (pulldown != null)
		{
			pulldown.SetActive(false);
		}
		open = false;
	}

	public void OnClick()
	{
		if (open)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	protected override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		handler = EntityTemplateExtensions.AddOrGet<MouseEventHandler>(((Component)this).gameObject);
	}

	public void Open()
	{
		GameObject pulldown = Pulldown;
		if ((Object)(object)pulldown != (Object)null)
		{
			float num = 0f;
			int count = currentItems.Count;
			int num2 = Math.Min(MaxRowsShown, count);
			Canvas val = EntityTemplateExtensions.AddOrGet<Canvas>(pulldown);
			pulldown.SetActive(true);
			if (count > 0)
			{
				RectTransform obj = Util.rectTransform(currentItems[0].rowInstance);
				LayoutRebuilder.ForceRebuildLayoutImmediate(obj);
				num = LayoutUtility.GetPreferredHeight(obj);
			}
			Util.rectTransform(pulldown).SetSizeWithCurrentAnchors((Axis)1, (float)num2 * num);
			ScrollRect val2 = default(ScrollRect);
			if (pulldown.TryGetComponent<ScrollRect>(ref val2))
			{
				val2.vertical = count >= MaxRowsShown;
			}
			if ((Object)(object)val != (Object)null)
			{
				val.overrideSorting = true;
				val.sortingOrder = 2;
			}
		}
		open = true;
	}

	public void SetItems(IEnumerable<IListableOption> items)
	{
		if (items == null)
		{
			return;
		}
		RectTransform contentContainer = ContentContainer;
		_ = Pulldown;
		int childCount = ((Transform)contentContainer).childCount;
		GameObject entryPrefab = EntryPrefab;
		bool flag = open;
		if (flag)
		{
			Close();
		}
		for (int i = 0; i < childCount; i++)
		{
			Object.Destroy((Object)(object)((Transform)contentContainer).GetChild(i));
		}
		currentItems.Clear();
		ToolTip val2 = default(ToolTip);
		foreach (IListableOption item in items)
		{
			string text = "";
			GameObject val = Util.KInstantiate(entryPrefab, ((Component)contentContainer).gameObject, (string)null);
			((TMP_Text)val.GetComponentInChildren<TextMeshProUGUI>()).SetText(item.GetProperName());
			KButton componentInChildren = val.GetComponentInChildren<KButton>();
			componentInChildren.ClearOnClick();
			componentInChildren.onClick += delegate
			{
				SetSelectedItem(item, fireListener: true);
				Close();
			};
			if (item is ITooltipListableOption tooltipListableOption)
			{
				text = tooltipListableOption.GetToolTipText();
			}
			if (val.TryGetComponent<ToolTip>(ref val2))
			{
				if (string.IsNullOrEmpty(text))
				{
					val2.ClearMultiStringTooltip();
				}
				else
				{
					val2.SetSimpleTooltip(text);
				}
			}
			val.SetActive(true);
			currentItems.Add(new ComboBoxItem(item, val));
		}
		TMP_Text selectedLabel = SelectedLabel;
		if (selectedLabel != null)
		{
			selectedLabel.SetText((currentItems.Count > 0) ? currentItems[0].data.GetProperName() : "");
		}
		if (flag)
		{
			Open();
		}
	}

	public void SetSelectedItem(IListableOption option, bool fireListener = false)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (option == null)
		{
			return;
		}
		TMP_Text selectedLabel = SelectedLabel;
		if (selectedLabel != null)
		{
			selectedLabel.SetText(option.GetProperName());
		}
		foreach (ComboBoxItem currentItem in currentItems)
		{
			IListableOption data = currentItem.data;
			((Graphic)currentItem.rowImage).color = ((data != null && ((object)data).Equals((object)option)) ? CheckColor : PUITuning.Colors.Transparent);
		}
		if (fireListener)
		{
			OnSelectionChanged?.Invoke(this, option);
		}
	}

	internal void Update()
	{
		if (open && (Object)(object)handler != (Object)null && !handler.IsOver && (Input.GetMouseButton(0) || Input.GetAxis("Mouse ScrollWheel") != 0f))
		{
			Close();
		}
	}
}
