using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DevLoader;

public static class UI
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__6_0;

		internal void _003CAttachBadge_003Eb__6_0()
		{
			Runtime.ApplyToggle(!Config.Enabled);
		}
	}

	private static Sprite _on;

	private static Sprite _off;

	private static Image _img;

	private static bool _badgeForcedOnce;

        public static void EnsureSprites()
        {
                if ((Object)_on == null || (Object)_off == null)
                {
                        _on = LoadSprite("dev_on.png");
                        _off = LoadSprite("dev_off.png");
                }
        }

	internal static Sprite LoadSprite(string file)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string directoryName = Path.GetDirectoryName(typeof(Mod).Assembly.Location);
			string path = Path.Combine(directoryName, "Images", file);
			byte[] array = File.ReadAllBytes(path);
			Texture2D val = new Texture2D(2, 2, (TextureFormat)5, false);
			ImageConversion.LoadImage(val, array);
			((Texture)val).filterMode = (FilterMode)1;
			return Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f), 100f);
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[DevLoader] No pude cargar " + file + ": " + ex.Message));
			return null;
		}
	}

	public static void AttachBadge(Transform parent)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		EnsureSprites();
		GameObject val = new GameObject("DevLoaderBadge", new Type[3]
		{
			typeof(RectTransform),
			typeof(Image),
			typeof(Button)
		});
		val.transform.SetParent(parent, false);
		_img = val.GetComponent<Image>();
		_img.preserveAspect = true;
		_img.sprite = ((!_badgeForcedOnce) ? _on : (Config.Enabled ? _on : _off));
		if (!_badgeForcedOnce)
		{
			_badgeForcedOnce = true;
		}
		((Graphic)_img).SetNativeSize();
		LayoutElement val2 = val.AddComponent<LayoutElement>();
                Rect rect = _img.sprite.rect;
                val2.minWidth = rect.width;
                val2.minHeight = rect.height;
                val2.preferredWidth = rect.width;
                val2.preferredHeight = rect.height;
		Button component = val.GetComponent<Button>();
                ((Selectable)component).transition = Selectable.Transition.None;
                Button.ButtonClickedEvent onClick = component.onClick;
		object obj = _003C_003Ec._003C_003E9__6_0;
		if (obj == null)
		{
			UnityAction val3 = delegate
			{
				Runtime.ApplyToggle(!Config.Enabled);
			};
			_003C_003Ec._003C_003E9__6_0 = val3;
			obj = (object)val3;
		}
		((UnityEvent)onClick).AddListener((UnityAction)obj);
		Runtime.Toggled -= UpdateBadge;
		Runtime.Toggled += UpdateBadge;
	}

        public static void UpdateBadge(bool enabled)
        {
                if ((Object)_img != null)
                {
                        _img.sprite = (enabled ? _on : _off);
                        ((Graphic)_img).SetNativeSize();
                }
        }
}
