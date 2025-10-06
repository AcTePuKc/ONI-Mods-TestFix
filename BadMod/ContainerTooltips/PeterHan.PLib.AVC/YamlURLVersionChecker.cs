using System;
using System.Collections.Generic;
using Klei;
using KMod;
using UnityEngine;
using UnityEngine.Networking;

namespace PeterHan.PLib.AVC;

public sealed class YamlURLVersionChecker : IModVersionChecker
{
	public string YamlVersionURL { get; }

	public event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

	public YamlURLVersionChecker(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			throw new ArgumentNullException("url");
		}
		YamlVersionURL = url;
	}

	public bool CheckVersion(Mod mod)
	{
		if (mod == null)
		{
			throw new ArgumentNullException("mod");
		}
		UnityWebRequest request = UnityWebRequest.Get(YamlVersionURL);
		request.SetRequestHeader("Content-Type", "application/x-yaml");
		request.SetRequestHeader("User-Agent", "PLib AVC");
		request.timeout = 8;
		((AsyncOperation)request.SendWebRequest()).completed += delegate
		{
			OnRequestFinished(request, mod);
		};
		return true;
	}

	private void OnRequestFinished(UnityWebRequest request, Mod mod)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		ModVersionCheckResults result = null;
		if ((int)request.result == 1)
		{
			PackagedModInfo obj = YamlIO.Parse<PackagedModInfo>(request.downloadHandler.text, default(FileHandle), (ErrorHandler)null, (List<Tuple<string, Type>>)null);
			string text = ((obj != null) ? obj.version : null);
			if (obj != null && !string.IsNullOrEmpty(text))
			{
				string currentVersion = PVersionCheck.GetCurrentVersion(mod);
				result = new ModVersionCheckResults(mod.staticID, text == currentVersion, text);
			}
		}
		request.Dispose();
		this.OnVersionCheckCompleted?.Invoke(result);
	}
}
