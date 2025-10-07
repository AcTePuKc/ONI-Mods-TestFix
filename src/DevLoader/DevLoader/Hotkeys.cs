using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevLoader;

public class Hotkeys : MonoBehaviour
{
	private bool CtrlHeld => Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305);

	private void Update()
	{
		if (CtrlHeld && Input.GetKeyDown((KeyCode)282))
		{
			Runtime.ApplyToggle(!Config.Enabled);
		}
		if (!CtrlHeld || !Input.GetKeyDown((KeyCode)49))
		{
			return;
		}
                if ((Object)Game.Instance != null && (Object)PauseScreen.Instance != null)
		{
			Debug.Log((object)"[DevLoader] Ctrl+1 → Saliendo al menú sin guardar");
                        LoadingOverlay.Load((System.Action)delegate
			{
				((KScreen)PauseScreen.Instance).Deactivate();
				PauseScreen.TriggerQuitGame();
			});
			return;
		}
		Debug.Log((object)"[DevLoader] Ctrl+1 → Reanudando última partida");
		string latestSaveForCurrentDLC = SaveLoader.GetLatestSaveForCurrentDLC();
		if (!string.IsNullOrEmpty(latestSaveForCurrentDLC))
		{
			SaveLoader.SetActiveSaveFilePath(latestSaveForCurrentDLC);
                        LoadingOverlay.Load((System.Action)delegate
			{
				App.LoadScene("backend");
			});
		}
		else
		{
			Debug.LogWarning((object)"[DevLoader] No se encontró partida para reanudar.");
		}
	}
}
