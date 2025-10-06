using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using KMod;
using UnityEngine;

namespace PeterHan.PLib.Core;

public static class PUtil
{
	private static volatile bool initialized;

	private static readonly object initializeLock;

	private static readonly HashSet<char> INVALID_FILE_CHARS;

	public static uint GameVersion { get; }

	static PUtil()
	{
		initialized = false;
		initializeLock = new object();
		INVALID_FILE_CHARS = new HashSet<char>(Path.GetInvalidFileNameChars());
		GameVersion = GetGameVersion();
	}

	public static IDictionary<Assembly, Mod> CreateAssemblyToModTable()
	{
		Global instance = Global.Instance;
		Dictionary<Assembly, Mod> dictionary = new Dictionary<Assembly, Mod>(32);
		if ((Object)(object)instance != (Object)null)
		{
			List<Mod> list = instance.modManager?.mods;
			if (list != null)
			{
				foreach (Mod item in list)
				{
					ICollection<Assembly> collection = item?.loaded_mod_data?.dlls;
					if (collection == null)
					{
						continue;
					}
					foreach (Assembly item2 in collection)
					{
						dictionary[item2] = item;
					}
				}
			}
		}
		return dictionary;
	}

	public static float Distance(float x1, float y1, float x2, float y2)
	{
		float num = x2 - x1;
		float num2 = y2 - y1;
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	public static double Distance(double x1, double y1, double x2, double y2)
	{
		double num = x2 - x1;
		double num2 = y2 - y1;
		return Math.Sqrt(num * num + num2 * num2);
	}

	private static uint GetGameVersion()
	{
		FieldInfo fieldSafe = typeof(KleiVersion).GetFieldSafe("ChangeList", isStatic: true);
		uint result = 0u;
		if (fieldSafe != null && fieldSafe.GetValue(null) is uint num)
		{
			result = num;
		}
		return result;
	}

	public static string GetModPath(Assembly modDLL)
	{
		if (modDLL == null)
		{
			throw new ArgumentNullException("modDLL");
		}
		string text = null;
		try
		{
			text = Directory.GetParent(modDLL.Location)?.FullName;
		}
		catch (NotSupportedException thrown)
		{
			LogExcWarn(thrown);
		}
		catch (SecurityException thrown2)
		{
			LogExcWarn(thrown2);
		}
		catch (IOException thrown3)
		{
			LogExcWarn(thrown3);
		}
		return text ?? Path.Combine(Manager.GetDirectory(), modDLL.GetName().Name ?? "");
	}

	public static void InitLibrary(bool logVersion = true)
	{
		Assembly callingAssembly = Assembly.GetCallingAssembly();
		lock (initializeLock)
		{
			if (!initialized)
			{
				initialized = true;
				if (logVersion)
				{
					Debug.LogFormat("[PLib] Mod {0} initialized, version {1}", new object[2]
					{
						callingAssembly.GetNameSafe(),
						callingAssembly.GetFileVersion() ?? "Unknown"
					});
				}
			}
		}
	}

	public static bool IsValidFileName(string file)
	{
		bool flag = file != null;
		if (flag)
		{
			int length = file.Length;
			for (int i = 0; i < length && flag; i++)
			{
				if (INVALID_FILE_CHARS.Contains(file[i]))
				{
					flag = false;
				}
			}
		}
		return flag;
	}

	public static void LogDebug(object message)
	{
		Debug.LogFormat("[PLib/{0}] {1}", new object[2]
		{
			Assembly.GetCallingAssembly().GetNameSafe(),
			message
		});
	}

	public static void LogError(object message)
	{
		Debug.LogErrorFormat("[PLib/{0}] {1}", new object[2]
		{
			Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
			message
		});
	}

	public static void LogException(Exception thrown)
	{
		Debug.LogErrorFormat("[PLib/{0}] {1} {2} {3}", new object[4]
		{
			Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
			thrown.GetType(),
			thrown.Message,
			thrown.StackTrace
		});
	}

	public static void LogExcWarn(Exception thrown)
	{
		Debug.LogWarningFormat("[PLib/{0}] {1} {2} {3}", new object[4]
		{
			Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
			thrown.GetType(),
			thrown.Message,
			thrown.StackTrace
		});
	}

	public static void LogWarning(object message)
	{
		Debug.LogWarningFormat("[PLib/{0}] {1}", new object[2]
		{
			Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
			message
		});
	}

	public static void Time(Action code, string header = "Code")
	{
		if (code == null)
		{
			throw new ArgumentNullException("code");
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		code();
		stopwatch.Stop();
		LogDebug("{1} took {0:D} us".F(stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency, header));
	}
}
