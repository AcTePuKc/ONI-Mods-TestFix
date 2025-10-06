using System;
using System.Collections.Generic;
using System.IO;
using KMod;
using Newtonsoft.Json;
using PeterHan.PLib.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace PeterHan.PLib.AVC;

public sealed class JsonURLVersionChecker : IModVersionChecker
{
	[JsonObject(/*Could not decode attribute arguments.*/)]
	public sealed class ModVersions
	{
		[JsonProperty]
		public List<ModVersion> mods;

		public ModVersions()
		{
			mods = new List<ModVersion>(16);
		}
	}

	public sealed class ModVersion
	{
		public string staticID { get; set; }

		public string version { get; set; }

		public override string ToString()
		{
			return "{0}: version={1}".F(staticID, version);
		}
	}

	public const int REQUEST_TIMEOUT = 8;

	public string JsonVersionURL { get; }

	public event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

	public JsonURLVersionChecker(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			throw new ArgumentNullException("url");
		}
		JsonVersionURL = url;
	}

	public bool CheckVersion(Mod mod)
	{
		if (mod == null)
		{
			throw new ArgumentNullException("mod");
		}
		UnityWebRequest request = UnityWebRequest.Get(JsonVersionURL);
		request.SetRequestHeader("Content-Type", "application/json");
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
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		ModVersionCheckResults result = null;
		if ((int)request.result == 1)
		{
			ModVersions modVersions;
			using (StreamReader streamReader = new StreamReader(new MemoryStream(request.downloadHandler.data)))
			{
				modVersions = new JsonSerializer
				{
					MaxDepth = 4,
					DateTimeZoneHandling = (DateTimeZoneHandling)1,
					ReferenceLoopHandling = (ReferenceLoopHandling)1
				}.Deserialize<ModVersions>((JsonReader)new JsonTextReader((TextReader)streamReader));
			}
			if (modVersions != null)
			{
				result = ParseModVersion(mod, modVersions);
			}
		}
		request.Dispose();
		this.OnVersionCheckCompleted?.Invoke(result);
	}

	private ModVersionCheckResults ParseModVersion(Mod mod, ModVersions versions)
	{
		ModVersionCheckResults result = null;
		string staticID = mod.staticID;
		if (versions.mods != null)
		{
			foreach (ModVersion mod2 in versions.mods)
			{
				if (mod2 != null && mod2.staticID == staticID)
				{
					string text = mod2.version?.Trim();
					result = ((!string.IsNullOrEmpty(text)) ? new ModVersionCheckResults(staticID, text == PVersionCheck.GetCurrentVersion(mod), text) : new ModVersionCheckResults(staticID, updated: true));
					break;
				}
			}
		}
		return result;
	}
}
