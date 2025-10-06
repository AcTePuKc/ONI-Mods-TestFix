using System;
using System.Collections;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace PeterHan.PLib.Core;

public static class ExtensionMethods
{
	public static string F(this string message, params object[] args)
	{
		return string.Format(message, args);
	}

	public static T GetComponentSafe<T>(this GameObject obj) where T : Component
	{
		if (!((Object)(object)obj == (Object)null))
		{
			return obj.GetComponent<T>();
		}
		return default(T);
	}

	public static string GetNameSafe(this Assembly assembly)
	{
		return assembly?.GetName()?.Name;
	}

	public static string GetFileVersion(this Assembly assembly)
	{
		object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), inherit: true);
		string result = null;
		if (customAttributes != null && customAttributes.Length != 0)
		{
			AssemblyFileVersionAttribute assemblyFileVersionAttribute = (AssemblyFileVersionAttribute)customAttributes[0];
			if (assemblyFileVersionAttribute != null)
			{
				result = assemblyFileVersionAttribute.Version;
			}
		}
		return result;
	}

	public static double InRange(this double value, double min, double max)
	{
		double num = value;
		if (num < min)
		{
			num = min;
		}
		if (num > max)
		{
			num = max;
		}
		return num;
	}

	public static float InRange(this float value, float min, float max)
	{
		float num = value;
		if (num < min)
		{
			num = min;
		}
		if (num > max)
		{
			num = max;
		}
		return num;
	}

	public static int InRange(this int value, int min, int max)
	{
		int num = value;
		if (num < min)
		{
			num = min;
		}
		if (num > max)
		{
			num = max;
		}
		return num;
	}

	public static bool IsFalling(this GameObject obj)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		int num = Grid.PosToCell(obj);
		Navigator val = default(Navigator);
		if (obj.TryGetComponent<Navigator>(ref val) && !val.IsMoving() && Grid.IsValidCell(num) && Grid.IsValidCell(Grid.CellBelow(num)))
		{
			return !val.NavGrid.NavTable.IsValid(num, val.CurrentNavType);
		}
		return false;
	}

	public static bool IsNaNOrInfinity(this double value)
	{
		if (!double.IsNaN(value))
		{
			return double.IsInfinity(value);
		}
		return true;
	}

	public static bool IsNaNOrInfinity(this float value)
	{
		if (!float.IsNaN(value))
		{
			return float.IsInfinity(value);
		}
		return true;
	}

	public static bool IsUsable(this GameObject building)
	{
		Operational val = default(Operational);
		if (building.TryGetComponent<Operational>(ref val))
		{
			return val.IsFunctional;
		}
		return false;
	}

	public static string Join(this IEnumerable values, string delimiter = ",")
	{
		StringBuilder stringBuilder = new StringBuilder(128);
		bool flag = true;
		foreach (object value in values)
		{
			if (!flag)
			{
				stringBuilder.Append(delimiter);
			}
			stringBuilder.Append(value);
			flag = false;
		}
		return stringBuilder.ToString();
	}

	public static void Patch(this Harmony instance, Type type, string methodName, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(methodName))
		{
			throw new ArgumentNullException("methodName");
		}
		try
		{
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				instance.Patch((MethodBase)method, prefix, postfix, (HarmonyMethod)null, (HarmonyMethod)null);
				return;
			}
			PUtil.LogWarning("Unable to find method {0} on type {1}".F(methodName, type.FullName));
		}
		catch (AmbiguousMatchException thrown)
		{
			PUtil.LogException(thrown);
		}
	}

	public static void PatchConstructor(this Harmony instance, Type type, Type[] arguments, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		try
		{
			ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, arguments, null);
			if (constructor != null)
			{
				instance.Patch((MethodBase)constructor, prefix, postfix, (HarmonyMethod)null, (HarmonyMethod)null);
				return;
			}
			PUtil.LogWarning("Unable to find constructor on type {0}".F(type.FullName));
		}
		catch (ArgumentException thrown)
		{
			PUtil.LogException(thrown);
		}
	}

	public static void PatchTranspile(this Harmony instance, Type type, string methodName, HarmonyMethod transpiler)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(methodName))
		{
			throw new ArgumentNullException("methodName");
		}
		try
		{
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				instance.Patch((MethodBase)method, (HarmonyMethod)null, (HarmonyMethod)null, transpiler, (HarmonyMethod)null);
				return;
			}
			PUtil.LogWarning("Unable to find method {0} on type {1}".F(methodName, type.FullName));
		}
		catch (AmbiguousMatchException thrown)
		{
			PUtil.LogException(thrown);
		}
		catch (FormatException ex)
		{
			PUtil.LogWarning("Unable to transpile method {0}: {1}".F(methodName, ex.Message));
		}
	}

	public static GameObject SetParent(this GameObject child, GameObject parent)
	{
		if ((Object)(object)child == (Object)null)
		{
			throw new ArgumentNullException("child");
		}
		child.transform.SetParent(((Object)(object)parent == (Object)null) ? null : parent.transform, false);
		return child;
	}
}
