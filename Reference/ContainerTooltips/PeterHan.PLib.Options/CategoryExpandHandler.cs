using PeterHan.PLib.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PeterHan.PLib.Options;

internal sealed class CategoryExpandHandler
{
	private GameObject contents;

	private readonly bool initialState;

	private GameObject toggle;

	public CategoryExpandHandler(bool initialState = true)
	{
		this.initialState = initialState;
	}

	public void OnExpandContract(GameObject _, bool on)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = (on ? Vector3.one : Vector3.zero);
		if ((Object)(object)contents != (Object)null)
		{
			RectTransform val = Util.rectTransform(contents);
			((Transform)val).localScale = localScale;
			if ((Object)(object)val != (Object)null)
			{
				LayoutRebuilder.MarkLayoutForRebuild(val);
			}
		}
	}

	private void OnHeaderClicked()
	{
		if ((Object)(object)toggle != (Object)null)
		{
			bool toggleState = PToggle.GetToggleState(toggle);
			PToggle.SetToggleState(toggle, !toggleState);
		}
	}

	public void OnRealizeHeader(GameObject header)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		Button obj = header.AddComponent<Button>();
		((UnityEvent)obj.onClick).AddListener(new UnityAction(OnHeaderClicked));
		((Selectable)obj).interactable = true;
	}

	public void OnRealizePanel(GameObject panel)
	{
		contents = panel;
		OnExpandContract(null, initialState);
	}

	public void OnRealizeToggle(GameObject toggle)
	{
		this.toggle = toggle;
	}
}
