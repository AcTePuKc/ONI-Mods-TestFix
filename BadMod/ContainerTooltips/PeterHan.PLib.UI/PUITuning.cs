using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public static class PUITuning
{
	public static class Images
	{
		private static readonly IDictionary<string, Sprite> SPRITES;

		public static Sprite Arrow { get; }

		public static Sprite BoxBorder { get; }

		public static Sprite BoxBorderWhite { get; }

		public static Sprite ButtonBorder { get; }

		public static Sprite CheckBorder { get; }

		public static Sprite Checked { get; }

		public static Sprite Close { get; }

		public static Sprite Contract { get; }

		public static Sprite Expand { get; }

		public static Sprite Partial { get; }

		public static Sprite ScrollBorderHorizontal { get; }

		public static Sprite ScrollHandleHorizontal { get; }

		public static Sprite ScrollBorderVertical { get; }

		public static Sprite ScrollHandleVertical { get; }

		public static Sprite SliderHandle { get; }

		static Images()
		{
			SPRITES = new Dictionary<string, Sprite>(512);
			Sprite[] array = Resources.FindObjectsOfTypeAll<Sprite>();
			foreach (Sprite val in array)
			{
				string text = ((val != null) ? ((Object)val).name : null);
				if (!string.IsNullOrEmpty(text) && !SPRITES.ContainsKey(text))
				{
					SPRITES.Add(text, val);
				}
			}
			Arrow = GetSpriteByName("game_speed_play");
			BoxBorder = GetSpriteByName("web_box");
			BoxBorderWhite = GetSpriteByName("web_border");
			ButtonBorder = GetSpriteByName("web_button");
			CheckBorder = GetSpriteByName("overview_jobs_skill_box");
			Checked = GetSpriteByName("overview_jobs_icon_checkmark");
			Close = GetSpriteByName("cancel");
			Contract = GetSpriteByName("iconDown");
			Expand = GetSpriteByName("iconRight");
			Partial = GetSpriteByName("overview_jobs_icon_mixed");
			ScrollBorderHorizontal = GetSpriteByName("build_menu_scrollbar_frame_horizontal");
			ScrollHandleHorizontal = GetSpriteByName("build_menu_scrollbar_inner_horizontal");
			ScrollBorderVertical = GetSpriteByName("build_menu_scrollbar_frame");
			ScrollHandleVertical = GetSpriteByName("build_menu_scrollbar_inner");
			SliderHandle = GetSpriteByName("game_speed_selected_med");
		}

		public static Sprite GetSpriteByName(string name)
		{
			if (!SPRITES.TryGetValue(name, out var value))
			{
				return null;
			}
			return value;
		}
	}

	public static class Colors
	{
		public static Color BackgroundLight { get; }

		public static ColorStyleSetting ButtonPinkStyle { get; }

		public static ColorStyleSetting ButtonBlueStyle { get; }

		public static ColorStyleSetting ComponentDarkStyle { get; }

		public static ColorStyleSetting ComponentLightStyle { get; }

		public static Color DialogBackground { get; }

		public static Color DialogDarkBackground { get; }

		public static Color OptionsBackground { get; }

		public static ColorBlock ScrollbarColors { get; }

		public static Color SelectionBackground { get; }

		public static Color SelectionForeground { get; }

		public static Color Transparent { get; }

		public static Color UITextDark { get; }

		public static Color UITextLight { get; }

		static Colors()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0307: Unknown result type (might be due to invalid IL or missing references)
			//IL_030c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0325: Unknown result type (might be due to invalid IL or missing references)
			//IL_032a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0343: Unknown result type (might be due to invalid IL or missing references)
			//IL_0348: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			//IL_037e: Unknown result type (might be due to invalid IL or missing references)
			//IL_039e: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03de: Unknown result type (might be due to invalid IL or missing references)
			//IL_03df: Unknown result type (might be due to invalid IL or missing references)
			BackgroundLight = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			DialogBackground = Color32.op_Implicit(new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue));
			DialogDarkBackground = Color32.op_Implicit(new Color32((byte)48, (byte)52, (byte)67, byte.MaxValue));
			OptionsBackground = Color32.op_Implicit(new Color32((byte)31, (byte)34, (byte)43, byte.MaxValue));
			SelectionBackground = Color32.op_Implicit(new Color32((byte)189, (byte)218, byte.MaxValue, byte.MaxValue));
			SelectionForeground = Color32.op_Implicit(new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue));
			Transparent = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)0));
			UITextLight = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			UITextDark = Color32.op_Implicit(new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue));
			Color val = default(Color);
			((Color)(ref val))._002Ector(0f, 0f, 0f);
			Color val2 = default(Color);
			((Color)(ref val2))._002Ector(0.784f, 0.784f, 0.784f, 1f);
			ComponentLightStyle = ScriptableObject.CreateInstance<ColorStyleSetting>();
			ComponentLightStyle.activeColor = val;
			ComponentLightStyle.inactiveColor = val;
			ComponentLightStyle.hoverColor = val;
			ComponentLightStyle.disabledActiveColor = val2;
			ComponentLightStyle.disabledColor = val2;
			ComponentLightStyle.disabledhoverColor = val2;
			((Color)(ref val))._002Ector(1f, 1f, 1f);
			ComponentDarkStyle = ScriptableObject.CreateInstance<ColorStyleSetting>();
			ComponentDarkStyle.activeColor = val;
			ComponentDarkStyle.inactiveColor = val;
			ComponentDarkStyle.hoverColor = val;
			ComponentDarkStyle.disabledActiveColor = val2;
			ComponentDarkStyle.disabledColor = val2;
			ComponentDarkStyle.disabledhoverColor = val2;
			ButtonPinkStyle = ScriptableObject.CreateInstance<ColorStyleSetting>();
			ButtonPinkStyle.activeColor = new Color(27f / 34f, 0.4496107f, 0.6242238f);
			ButtonPinkStyle.inactiveColor = new Color(0.5294118f, 0.2724914f, 0.4009516f);
			ButtonPinkStyle.disabledColor = new Color(0.4156863f, 0.4117647f, 0.4f);
			ButtonPinkStyle.disabledActiveColor = Transparent;
			ButtonPinkStyle.hoverColor = new Color(0.6176471f, 0.3315311f, 0.4745891f);
			ButtonPinkStyle.disabledhoverColor = new Color(0.5f, 0.5f, 0.5f);
			ButtonBlueStyle = ScriptableObject.CreateInstance<ColorStyleSetting>();
			ButtonBlueStyle.activeColor = new Color(0.5033521f, 0.5444419f, 95f / 136f);
			ButtonBlueStyle.inactiveColor = new Color(0.2431373f, 0.2627451f, 0.3411765f);
			ButtonBlueStyle.disabledColor = new Color(0.4156863f, 0.4117647f, 0.4f);
			ButtonBlueStyle.disabledActiveColor = new Color(0.625f, 0.6158088f, 0.5882353f);
			ButtonBlueStyle.hoverColor = new Color(0.3461289f, 0.3739619f, 33f / 68f);
			ButtonBlueStyle.disabledhoverColor = new Color(0.5f, 0.4898898f, 0.4595588f);
			ColorBlock val3 = default(ColorBlock);
			((ColorBlock)(ref val3)).colorMultiplier = 1f;
			((ColorBlock)(ref val3)).fadeDuration = 0.1f;
			((ColorBlock)(ref val3)).disabledColor = new Color(0.392f, 0.392f, 0.392f);
			((ColorBlock)(ref val3)).highlightedColor = Color32.op_Implicit(new Color32((byte)161, (byte)163, (byte)174, byte.MaxValue));
			((ColorBlock)(ref val3)).normalColor = Color32.op_Implicit(new Color32((byte)161, (byte)163, (byte)174, byte.MaxValue));
			((ColorBlock)(ref val3)).pressedColor = BackgroundLight;
			ScrollbarColors = val3;
		}
	}

	public static class Fonts
	{
		private const string DEFAULT_FONT_TEXT = "NotoSans-Regular";

		private const string DEFAULT_FONT_UI = "GRAYSTROKE REGULAR SDF";

		private static readonly TMP_FontAsset DefaultTextFont;

		private static readonly TMP_FontAsset DefaultUIFont;

		private static readonly IDictionary<string, TMP_FontAsset> FONTS;

		public static int DefaultSize { get; }

		internal static TMP_FontAsset Text
		{
			get
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				TMP_FontAsset val = null;
				if ((int)Localization.GetSelectedLanguageType() != 0)
				{
					val = Localization.FontAsset;
				}
				return val ?? DefaultTextFont;
			}
		}

		public static TextStyleSetting TextDarkStyle { get; }

		public static TextStyleSetting TextLightStyle { get; }

		internal static TMP_FontAsset UI
		{
			get
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				TMP_FontAsset val = null;
				if ((int)Localization.GetSelectedLanguageType() != 0)
				{
					val = Localization.FontAsset;
				}
				return val ?? DefaultUIFont;
			}
		}

		public static TextStyleSetting UIDarkStyle { get; }

		public static TextStyleSetting UILightStyle { get; }

		static Fonts()
		{
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			FONTS = new Dictionary<string, TMP_FontAsset>(16);
			TMP_FontAsset[] array = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
			foreach (TMP_FontAsset val in array)
			{
				string text = ((val != null) ? ((Object)val).name : null);
				if (!string.IsNullOrEmpty(text) && !FONTS.ContainsKey(text))
				{
					FONTS.Add(text, val);
				}
			}
			if ((Object)(object)(DefaultTextFont = GetFontByName("NotoSans-Regular")) == (Object)null)
			{
				PUIUtils.LogUIWarning("Unable to find font NotoSans-Regular");
			}
			if ((Object)(object)(DefaultUIFont = GetFontByName("GRAYSTROKE REGULAR SDF")) == (Object)null)
			{
				PUIUtils.LogUIWarning("Unable to find font GRAYSTROKE REGULAR SDF");
			}
			DefaultSize = 14;
			TextDarkStyle = ScriptableObject.CreateInstance<TextStyleSetting>();
			TextDarkStyle.enableWordWrapping = false;
			TextDarkStyle.fontSize = DefaultSize;
			TextDarkStyle.sdfFont = Text;
			TextDarkStyle.style = (FontStyles)0;
			TextDarkStyle.textColor = Colors.UITextDark;
			TextLightStyle = TextDarkStyle.DeriveStyle(0, Colors.UITextLight);
			UIDarkStyle = ScriptableObject.CreateInstance<TextStyleSetting>();
			UIDarkStyle.enableWordWrapping = false;
			UIDarkStyle.fontSize = DefaultSize;
			UIDarkStyle.sdfFont = UI;
			UIDarkStyle.style = (FontStyles)0;
			UIDarkStyle.textColor = Colors.UITextDark;
			UILightStyle = UIDarkStyle.DeriveStyle(0, Colors.UITextLight);
		}

		internal static TMP_FontAsset GetFontByName(string name)
		{
			if (!FONTS.TryGetValue(name, out var value))
			{
				return null;
			}
			return value;
		}
	}

	internal static ButtonSoundPlayer ButtonSounds { get; }

	internal static ToggleSoundPlayer ToggleSounds { get; }

	static PUITuning()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		ButtonSounds = new ButtonSoundPlayer
		{
			Enabled = true
		};
		ToggleSounds = new ToggleSoundPlayer();
	}
}
