using System;
using PeterHan.PLib.Detours;
using UnityEngine;

namespace PeterHan.PLib.Core;

public static class PGameUtils
{
	private delegate void CreateSoundDelegate(AudioSheets instance, string file_name, string anim_name, string type, float min_interval, string sound_name, int frame, string dlcId);

	private delegate void InfoRefreshFunction(SimpleInfoScreen instance, bool force);

	private delegate bool IsDLCEnabledFunction(string name);

	private static readonly DetouredMethod<CreateSoundDelegate> CREATE_SOUND = typeof(AudioSheets).DetourLazy<CreateSoundDelegate>("CreateSound");

	private static readonly DetouredMethod<InfoRefreshFunction> REFRESH_INFO_SCREEN = typeof(SimpleInfoScreen).DetourLazy<InfoRefreshFunction>("Refresh");

	private static readonly DetouredMethod<IsDLCEnabledFunction> CHECK_SUBSCRIPTION = typeof(DlcManager).DetourLazy<IsDLCEnabledFunction>("CheckPlatformSubscription");

	private static readonly DetouredMethod<IsDLCEnabledFunction> IS_CONTENT_ENABLED = typeof(DlcManager).DetourLazy<IsDLCEnabledFunction>("IsContentSettingEnabled");

	public static void CenterAndSelect(KMonoBehaviour entity)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		KSelectable val = default(KSelectable);
		if ((Object)(object)entity != (Object)null && ((Component)entity).TryGetComponent<KSelectable>(ref val))
		{
			SelectTool.Instance.SelectAndFocus(entity.transform.position, val, Vector3.zero);
		}
	}

	public static void CopySoundsToAnim(string dstAnim, string srcAnim)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(dstAnim))
		{
			throw new ArgumentNullException("dstAnim");
		}
		if (string.IsNullOrEmpty(srcAnim))
		{
			throw new ArgumentNullException("srcAnim");
		}
		if ((Object)(object)Assets.GetAnim(HashedString.op_Implicit(dstAnim)) != (Object)null)
		{
			GameAudioSheets val = GameAudioSheets.Get();
			try
			{
				foreach (AudioSheet sheet in ((AudioSheets)val).sheets)
				{
					SoundInfo[] soundInfos = sheet.soundInfos;
					int num = soundInfos.Length;
					for (int i = 0; i < num; i++)
					{
						SoundInfo val2 = soundInfos[i];
						if (IsDLCOwned(val2.RequiredDlcId) && val2.File == srcAnim)
						{
							CreateAllSounds((AudioSheets)(object)val, dstAnim, val2, sheet.defaultType);
						}
					}
				}
				return;
			}
			catch (Exception thrown)
			{
				PUtil.LogWarning("Unable to copy sound files from {0} to {1}:".F(srcAnim, dstAnim));
				PUtil.LogExcWarn(thrown);
				return;
			}
		}
		PUtil.LogWarning("Destination animation \"{0}\" not found!".F(dstAnim));
	}

	private static int CreateSound(AudioSheets sheet, string file, string type, SoundInfo info, string sound, int frame)
	{
		int result = 0;
		if (!string.IsNullOrEmpty(sound) && CREATE_SOUND != null)
		{
			CREATE_SOUND.Invoke(sheet, file, info.Anim, type, info.MinInterval, sound, frame, info.RequiredDlcId);
			result = 1;
		}
		return result;
	}

	private static void CreateAllSounds(AudioSheets sheet, string animFile, SoundInfo info, string defaultType)
	{
		string text = info.Type;
		if (string.IsNullOrEmpty(text))
		{
			text = defaultType;
		}
		_ = CreateSound(sheet, animFile, text, info, info.Name0, info.Frame0) + CreateSound(sheet, animFile, text, info, info.Name1, info.Frame1) + CreateSound(sheet, animFile, text, info, info.Name2, info.Frame2) + CreateSound(sheet, animFile, text, info, info.Name3, info.Frame3) + CreateSound(sheet, animFile, text, info, info.Name4, info.Frame4) + CreateSound(sheet, animFile, text, info, info.Name5, info.Frame5) + CreateSound(sheet, animFile, text, info, info.Name6, info.Frame6) + CreateSound(sheet, animFile, text, info, info.Name7, info.Frame7) + CreateSound(sheet, animFile, text, info, info.Name8, info.Frame8) + CreateSound(sheet, animFile, text, info, info.Name9, info.Frame9) + CreateSound(sheet, animFile, text, info, info.Name10, info.Frame10);
		CreateSound(sheet, animFile, text, info, info.Name11, info.Frame11);
	}

	public static void CreatePopup(Sprite image, string text, int cell)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		CreatePopup(image, text, Grid.CellToPosCBC(cell, (SceneLayer)25));
	}

	public static void CreatePopup(Sprite image, string text, Vector3 position)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		PopFXManager.Instance.SpawnFX(image, text, (Transform)null, position, 1.5f, false, false);
	}

	public static IntraObjectHandler<T> CreateUserMenuHandler<T>() where T : Component, IRefreshUserMenu
	{
		return IntraObjectHandler<T>.op_Implicit((Action<T, object>)delegate(T target, object ignore)
		{
			target.OnRefreshUserMenu();
		});
	}

	public static ObjectLayer GetObjectLayer(string name, ObjectLayer defValue)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (!Enum.TryParse<ObjectLayer>(name, out ObjectLayer result))
		{
			return defValue;
		}
		return result;
	}

	public static void HighlightEntity(Component entity, Color highlightColor)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		KAnimControllerBase val = default(KAnimControllerBase);
		if ((Object)(object)entity != (Object)null && entity.TryGetComponent<KAnimControllerBase>(ref val))
		{
			val.HighlightColour = Color32.op_Implicit(highlightColor);
		}
	}

	public static bool IsDLCOwned(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			if (CHECK_SUBSCRIPTION.Invoke(name))
			{
				return IS_CONTENT_ENABLED.Invoke(name);
			}
			return false;
		}
		return true;
	}

	public static void PlaySound(string name, Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		SoundEvent.PlayOneShot(GlobalAssets.GetSound(name, false), position, 1f);
	}

	public static void RefreshInfoScreen(this SimpleInfoScreen screen, bool force = false)
	{
		if ((Object)(object)screen != (Object)null)
		{
			REFRESH_INFO_SCREEN.Invoke(screen, force);
		}
	}

	public static void SaveMods()
	{
		Global.Instance.modManager.Save();
	}
}
