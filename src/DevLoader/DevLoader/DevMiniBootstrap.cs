using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DevLoader;

internal sealed class DevMiniBootstrap : MonoBehaviour
{
	private bool started;

	private bool installed;

	private void Awake()
	{
		Object.DontDestroyOnLoad((Object)((Component)this).gameObject);
		((Object)this).name = "DevMiniBootstrap";
		Debug.Log((object)"[DevLoader][MiniCenter] Bootstrap Awake");
	}

	private void Start()
	{
		if (!started)
		{
			started = true;
			Debug.Log((object)"[DevLoader][MiniCenter] Bootstrap Start -> hook scene events + loop until found");
			SceneManager.activeSceneChanged += OnActiveSceneChanged;
			((MonoBehaviour)this).StartCoroutine(InstallUntilFound());
		}
	}

	private void OnDestroy()
	{
		try
		{
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		}
		catch
		{
		}
		Debug.Log((object)"[DevLoader][MiniCenter] Bootstrap Destroy");
	}

        private void OnActiveSceneChanged(Scene oldS, Scene newS)
        {
                Debug.Log((object)("[DevLoader][MiniCenter] SceneChanged: " + oldS.name + " -> " + newS.name));
                TryInstallNow();
        }

	private IEnumerator InstallUntilFound()
	{
		while (!installed)
		{
			TryInstallNow();
			if (!installed)
			{
				yield return (object)new WaitForSecondsRealtime(1f);
			}
		}
		Object.Destroy((Object)((Component)this).gameObject);
	}

	private void TryInstallNow()
	{
		if (installed)
		{
			return;
		}
		try
		{
			Transform val = FindIngameParent();
                        Debug.Log((object)("[DevLoader][MiniCenter] TryInstallNow parent=" + (((Object)val != null) ? ((Object)val).name : "<null>")));
			if ((Object)val != null)
			{
				CenterMini.Ensure(val);
				installed = true;
				Debug.Log((object)"[DevLoader][MiniCenter] Installed OK");
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[DevLoader][MiniCenter] TryInstallNow ERROR: " + ex));
		}
	}

	private static Transform FindIngameParent()
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		string[] array = new string[8] { "OverlayScreen", "OverlayMenu", "ToolMenu", "HUD", "ManagementMenu", "BuildMenu", "PlanScreen", "TopRightInfoPanelButtons" };
		MonoBehaviour[] array2 = Object.FindObjectsOfType<MonoBehaviour>(true);
		MonoBehaviour[] array3 = array2;
		foreach (MonoBehaviour val in array3)
		{
			string name = ((object)val).GetType().Name;
			for (int j = 0; j < array.Length; j++)
			{
				if (name == array[j])
				{
					return ((Component)val).transform;
				}
			}
		}
                Scene activeScene = SceneManager.GetActiveScene();
                GameObject[] rootGameObjects = activeScene.GetRootGameObjects();
		GameObject[] array4 = rootGameObjects;
		foreach (GameObject val2 in array4)
		{
			Transform transform = val2.transform;
			if ((Object)transform != null && ((Object)transform).name.IndexOf("Canvas", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return transform;
			}
		}
		return null;
	}
}
