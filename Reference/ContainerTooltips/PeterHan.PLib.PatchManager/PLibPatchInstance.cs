using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.PatchManager;

internal sealed class PLibPatchInstance : IPatchMethodInstance
{
	public PLibPatchAttribute Descriptor { get; }

	public MethodInfo Method { get; }

	public PLibPatchInstance(PLibPatchAttribute attribute, MethodInfo method)
	{
		Descriptor = attribute ?? throw new ArgumentNullException("attribute");
		Method = method ?? throw new ArgumentNullException("method");
	}

	private unsafe HarmonyPatchType GetPatchType()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		HarmonyPatchType val = Descriptor.PatchType;
		if ((int)val == 0)
		{
			string name = Method.Name;
			foreach (object value in Enum.GetValues(typeof(HarmonyPatchType)))
			{
				if (value is HarmonyPatchType val2 && val2 != val && name.EndsWith(((object)(*(HarmonyPatchType*)(&val2))/*cast due to .constrained prefix*/).ToString(), StringComparison.Ordinal))
				{
					val = val2;
					break;
				}
			}
		}
		return val;
	}

	private MethodBase GetTargetConstructor(Type targetType, Type[] argumentTypes)
	{
		if (argumentTypes == null)
		{
			ConstructorInfo[] constructors = targetType.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (constructors == null || constructors.Length != 1)
			{
				throw new InvalidOperationException("No constructor for {0} found".F(targetType.FullName));
			}
			return constructors[0];
		}
		return targetType.GetConstructor(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, argumentTypes, null);
	}

	private MethodBase GetTargetMethod(Type requiredType)
	{
		Type type = Descriptor.TargetType;
		Type[] argumentTypes = Descriptor.ArgumentTypes;
		string methodName = Descriptor.MethodName;
		if (type == null)
		{
			type = requiredType;
		}
		if (type == null)
		{
			throw new InvalidOperationException("No type specified to patch");
		}
		MethodBase methodBase = ((!string.IsNullOrEmpty(methodName) && !(methodName == ".ctor")) ? ((argumentTypes == null) ? type.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : type.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, argumentTypes, null)) : GetTargetConstructor(type, argumentTypes));
		if (methodBase == null)
		{
			throw new InvalidOperationException("Method {0}.{1} not found".F(type.FullName, methodName));
		}
		return methodBase;
	}

	private bool LogIgnoreOnFail(Exception e)
	{
		bool ignoreOnFail = Descriptor.IgnoreOnFail;
		if (ignoreOnFail)
		{
			PUtil.LogDebug("Patch for {0} not applied: {1}".F(Descriptor.MethodName, e.Message));
		}
		return ignoreOnFail;
	}

	public void Run(Harmony instance)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected I4, but got Unknown
		if (!PPatchManager.CheckConditions(Descriptor.RequireAssembly, Descriptor.RequireType, out var requiredType))
		{
			return;
		}
		HarmonyMethod val = new HarmonyMethod(Method);
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		try
		{
			MethodBase targetMethod = GetTargetMethod(requiredType);
			HarmonyPatchType patchType = GetPatchType();
			switch (patchType - 1)
			{
			case 1:
				instance.Patch(targetMethod, (HarmonyMethod)null, val, (HarmonyMethod)null, (HarmonyMethod)null);
				break;
			case 0:
				instance.Patch(targetMethod, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				break;
			case 2:
				instance.Patch(targetMethod, (HarmonyMethod)null, (HarmonyMethod)null, val, (HarmonyMethod)null);
				break;
			default:
				throw new ArgumentOutOfRangeException("HarmonyPatchType");
			}
		}
		catch (AmbiguousMatchException e)
		{
			if (!LogIgnoreOnFail(e))
			{
				throw;
			}
		}
		catch (InvalidOperationException e2)
		{
			if (!LogIgnoreOnFail(e2))
			{
				throw;
			}
		}
	}
}
