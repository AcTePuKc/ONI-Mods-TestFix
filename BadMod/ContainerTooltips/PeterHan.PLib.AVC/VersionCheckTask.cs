using System;
using System.Collections.Generic;
using KMod;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.AVC;

internal sealed class VersionCheckTask
{
	private readonly IModVersionChecker method;

	private readonly Mod mod;

	private readonly ICollection<ModVersionCheckResults> results;

	internal Action Next { get; set; }

	internal VersionCheckTask(Mod mod, IModVersionChecker method, ICollection<ModVersionCheckResults> results)
	{
		this.mod = mod ?? throw new ArgumentNullException("mod");
		this.method = method ?? throw new ArgumentNullException("method");
		this.results = results ?? throw new ArgumentNullException("results");
		Next = null;
	}

	private void OnComplete(ModVersionCheckResults result)
	{
		method.OnVersionCheckCompleted -= OnComplete;
		if (result != null)
		{
			results.Add(result);
			if (!result.IsUpToDate)
			{
				PUtil.LogWarning("Mod {0} is out of date! New version: {1}".F(result.ModChecked, result.NewVersion ?? "unknown"));
			}
		}
		RunNext();
	}

	internal void Run()
	{
		bool flag = false;
		foreach (ModVersionCheckResults result in results)
		{
			if (result.ModChecked == mod.staticID)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			method.OnVersionCheckCompleted += OnComplete;
			bool flag2;
			try
			{
				flag2 = method.CheckVersion(mod);
			}
			catch (Exception thrown)
			{
				PUtil.LogWarning("Unable to check version for mod " + mod.label.title + ":");
				PUtil.LogExcWarn(thrown);
				flag2 = false;
			}
			if (!flag2)
			{
				method.OnVersionCheckCompleted -= OnComplete;
				RunNext();
			}
		}
	}

	private void RunNext()
	{
		Next?.Invoke();
	}
}
