using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DevLoader;

public static class CenterMini
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__3_0;

		internal void _003CEnsure_003Eb__3_0()
		{
			Debug.Log((object)("[DevLoader][MiniCenter] CLICK toggle from " + Config.Enabled));
			try
			{
				Runtime.ApplyToggle(!Config.Enabled);
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)("[DevLoader][MiniCenter] Toggle ERROR: " + ex));
			}
			UpdateIcon();
		}
	}

	private static Image _img;

	private static Sprite _onMini;

	private static Sprite _offMini;

	public static void Ensure(Transform parentHint)
	{
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Expected O, but got Unknown
		Debug.Log((object)("[DevLoader][MiniCenter] Ensure ENTER parentHint=" + (((Object)(object)parentHint != (Object)null) ? ((Object)parentHint).name : "<null>")));
		GameObject obj = GameObject.Find("DevCenterMiniButton");
		RectTransform val = ((obj != null) ? obj.GetComponent<RectTransform>() : null);
		if ((Object)(object)val != (Object)null)
		{
			Debug.Log((object)"[DevLoader][MiniCenter] Ya existe el botón mini, reubicando...");
		}
		else
		{
			_onMini = TryLoadMini("mini_dev_on.png", "mini_dev_on");
			_offMini = TryLoadMini("mini_dev_off.png", "mini_dev_off");
			Debug.Log((object)("[DevLoader][MiniCenter] Sprites: on=" + (Object.op_Implicit((Object)(object)_onMini) ? ((Object)_onMini).name : "<null>") + " off=" + (Object.op_Implicit((Object)(object)_offMini) ? ((Object)_offMini).name : "<null>")));
			Transform val2 = (((Object)(object)parentHint != (Object)null) ? parentHint.root : null);
			if ((Object)(object)val2 == (Object)null && (Object)(object)parentHint != (Object)null)
			{
				val2 = parentHint;
			}
			Debug.Log((object)("[DevLoader][MiniCenter] Parent picked: " + (((Object)(object)val2 != (Object)null) ? ((Object)val2).name : "<null>")));
			if ((Object)(object)val2 == (Object)null)
			{
				Debug.LogWarning((object)"[DevLoader][MiniCenter] parent NULL, abort");
				return;
			}
			GameObject val3 = new GameObject("DevCenterMiniButton", new Type[3]
			{
				typeof(RectTransform),
				typeof(Image),
				typeof(Button)
			});
			val3.transform.SetParent(val2, false);
			val = val3.GetComponent<RectTransform>();
			_img = val3.GetComponent<Image>();
			_img.preserveAspect = true;
			((Graphic)_img).color = Color.white;
			_img.sprite = (Config.Enabled ? (_onMini ?? _offMini) : (_offMini ?? _onMini));
			Debug.Log((object)("[DevLoader][MiniCenter] Image sprite set -> " + (Object.op_Implicit((Object)(object)_img.sprite) ? ((Object)_img.sprite).name : "<null>")));
			Button component = val3.GetComponent<Button>();
			((Selectable)component).transition = (Transition)0;
			((UnityEventBase)component.onClick).RemoveAllListeners();
			ButtonClickedEvent onClick = component.onClick;
			object obj2 = _003C_003Ec._003C_003E9__3_0;
			if (obj2 == null)
			{
				UnityAction val4 = delegate
				{
					Debug.Log((object)("[DevLoader][MiniCenter] CLICK toggle from " + Config.Enabled));
					try
					{
						Runtime.ApplyToggle(!Config.Enabled);
					}
					catch (Exception ex)
					{
						Debug.LogWarning((object)("[DevLoader][MiniCenter] Toggle ERROR: " + ex));
					}
					UpdateIcon();
				};
				_003C_003Ec._003C_003E9__3_0 = val4;
				obj2 = (object)val4;
			}
			((UnityEvent)onClick).AddListener((UnityAction)obj2);
			Runtime.Toggled -= OnToggled;
			Runtime.Toggled += OnToggled;
			UpdateIcon();
		}
		RectTransform obj3 = val;
		RectTransform obj4 = val;
		RectTransform obj5 = val;
		Vector2 val5 = default(Vector2);
		((Vector2)(ref val5))._002Ector(1f, 1f);
		obj5.pivot = val5;
		Vector2 anchorMin = (obj4.anchorMax = val5);
		obj3.anchorMin = anchorMin;
		val.anchoredPosition = new Vector2(-1100f, -18f);
	}

	private static void OnToggled(bool enabled)
	{
		Debug.Log((object)("[DevLoader][MiniCenter] Runtime.Toggled => " + enabled));
		UpdateIcon();
	}

	private static void UpdateIcon()
	{
		if ((Object)(object)_img == (Object)null)
		{
			Debug.LogWarning((object)"[DevLoader][MiniCenter] UpdateIcon with _img NULL");
			return;
		}
		Sprite val = (Config.Enabled ? (_onMini ?? _offMini) : (_offMini ?? _onMini));
		_img.sprite = val;
		Debug.Log((object)("[DevLoader][MiniCenter] UpdateIcon -> " + (Config.Enabled ? "ON" : "OFF") + " sprite=" + (Object.op_Implicit((Object)(object)val) ? ((Object)val).name : "<null>")));
	}

	private static Sprite TryLoadMini(string file, string atlasKey)
	{
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string directoryName = Path.GetDirectoryName(typeof(CenterMini).Assembly.Location);
			string text = Path.Combine(directoryName, "Images", file);
			Debug.Log((object)("[DevLoader][MiniCenter] TryLoadMini file=" + text));
			if (File.Exists(text))
			{
				byte[] array = File.ReadAllBytes(text);
				Debug.Log((object)("[DevLoader][MiniCenter] Bytes=" + ((array != null) ? array.Length.ToString() : "<null>")));
				Texture2D val = new Texture2D(2, 2, (TextureFormat)5, false);
				if (!ImageConversion.LoadImage(val, array))
				{
					Debug.LogWarning((object)"[DevLoader][MiniCenter] LoadImage FALSE");
				}
				((Texture)val).filterMode = (FilterMode)1;
				Sprite result = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f), 100f);
				Debug.Log((object)("[DevLoader][MiniCenter] Sprite OK " + file + " size=" + ((Texture)val).width + "x" + ((Texture)val).height));
				return result;
			}
			Debug.LogWarning((object)("[DevLoader][MiniCenter] File NOT FOUND " + text));
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[DevLoader][MiniCenter] File load ERROR: " + ex));
		}
		try
		{
			Sprite sprite = Assets.GetSprite(HashedString.op_Implicit(atlasKey));
			Debug.Log((object)("[DevLoader][MiniCenter] Atlas  + atlasKey +  -> " + (Object.op_Implicit((Object)(object)sprite) ? ((Object)sprite).name : "<null>")));
			return sprite;
		}
		catch (Exception ex2)
		{
			Debug.LogWarning((object)("[DevLoader][MiniCenter] Atlas ERROR: " + ex2));
		}
		return null;
	}
}
