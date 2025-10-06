using System;
using System.Collections.Generic;
using System.Reflection;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

internal static class UIDetours
{
	public delegate void PCD(ConfirmDialogScreen dialog, string text, Action on_confirm, Action on_cancel, string configurable_text, Action on_configurable_clicked, string title_text, string confirm_text, string cancel_text);

	public static readonly DetouredMethod<PCD> POPUP_CONFIRM = typeof(ConfirmDialogScreen).DetourLazy<PCD>("PopupConfirmDialog");

	public static readonly IDetouredField<DetailsScreen, List<SideScreenRef>> SIDE_SCREENS = PDetours.DetourFieldLazy<DetailsScreen, List<SideScreenRef>>("sideScreens");

	private const string SS_FIELD = "sideScreenConfigContentBody";

	public static readonly IDetouredField<DetailsScreen, GameObject> SS_CONTENT_BODY = ((typeof(DetailsScreen).GetFieldSafe("sideScreenConfigContentBody", isStatic: false) != null) ? PDetours.DetourField<DetailsScreen, GameObject>("sideScreenConfigContentBody") : null);

	public static readonly MethodInfo SS_GET_TAB = typeof(DetailsScreen).GetMethodSafe("GetTabOfType", isStatic: false, PPatchTools.AnyArguments);

	public static readonly IDetouredField<KButton, KImage[]> ADDITIONAL_K_IMAGES = PDetours.DetourFieldLazy<KButton, KImage[]>("additionalKImages");

	public static readonly IDetouredField<KButton, KImage> BG_IMAGE = PDetours.DetourFieldLazy<KButton, KImage>("bgImage");

	public static readonly IDetouredField<KButton, Image> FG_IMAGE = PDetours.DetourFieldLazy<KButton, Image>("fgImage");

	public static readonly IDetouredField<KButton, bool> IS_INTERACTABLE = PDetours.DetourFieldLazy<KButton, bool>("isInteractable");

	public static readonly IDetouredField<KButton, ButtonSoundPlayer> SOUND_PLAYER_BUTTON = PDetours.DetourFieldLazy<KButton, ButtonSoundPlayer>("soundPlayer");

	public static readonly IDetouredField<KImage, ColorStyleSetting> COLOR_STYLE_SETTING = PDetours.DetourFieldLazy<KImage, ColorStyleSetting>("colorStyleSetting");

	public static readonly DetouredMethod<Action<KImage>> APPLY_COLOR_STYLE = typeof(KImage).DetourLazy<Action<KImage>>("ApplyColorStyleSetting");

	public static readonly DetouredMethod<Action<KScreen>> ACTIVATE_KSCREEN = typeof(KScreen).DetourLazy<Action<KScreen>>("Activate");

	public static readonly DetouredMethod<Action<KScreen>> DEACTIVATE_KSCREEN = typeof(KScreen).DetourLazy<Action<KScreen>>("Deactivate");

	public static readonly IDetouredField<KToggle, KToggleArtExtensions> ART_EXTENSION = PDetours.DetourFieldLazy<KToggle, KToggleArtExtensions>("artExtension");

	public static readonly IDetouredField<KToggle, bool> IS_ON = PDetours.DetourFieldLazy<KToggle, bool>("isOn");

	public static readonly IDetouredField<KToggle, ToggleSoundPlayer> SOUND_PLAYER_TOGGLE = PDetours.DetourFieldLazy<KToggle, ToggleSoundPlayer>("soundPlayer");

	public static readonly IDetouredField<LocText, string> LOCTEXT_KEY = PDetours.DetourFieldLazy<LocText, string>("key");

	public static readonly IDetouredField<LocText, TextStyleSetting> LOCTEXT_STYLE = PDetours.DetourFieldLazy<LocText, TextStyleSetting>("textStyleSetting");

	public static readonly IDetouredField<MultiToggle, int> CURRENT_STATE = PDetours.DetourFieldLazy<MultiToggle, int>("CurrentState");

	public static readonly IDetouredField<MultiToggle, bool> PLAY_SOUND_CLICK = PDetours.DetourFieldLazy<MultiToggle, bool>("play_sound_on_click");

	public static readonly IDetouredField<MultiToggle, bool> PLAY_SOUND_RELEASE = PDetours.DetourFieldLazy<MultiToggle, bool>("play_sound_on_release");

	public static readonly DetouredMethod<Action<MultiToggle, int>> CHANGE_STATE = typeof(MultiToggle).DetourLazy<Action<MultiToggle, int>>("ChangeState");

	public static readonly IDetouredField<SideScreenContent, GameObject> SS_CONTENT_CONTAINER = PDetours.DetourFieldLazy<SideScreenContent, GameObject>("ContentContainer");

	public static readonly IDetouredField<SideScreenRef, Vector2> SS_OFFSET = PDetours.DetourFieldLazy<SideScreenRef, Vector2>("offset");

	public static readonly IDetouredField<SideScreenRef, SideScreenContent> SS_PREFAB = PDetours.DetourFieldLazy<SideScreenRef, SideScreenContent>("screenPrefab");

	public static readonly IDetouredField<SideScreenRef, SideScreenContent> SS_INSTANCE = PDetours.DetourFieldLazy<SideScreenRef, SideScreenContent>("screenInstance");
}
