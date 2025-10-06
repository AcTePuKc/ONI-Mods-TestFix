using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.PatchManager;

internal sealed class PLibMethodInstance : IPatchMethodInstance
{
	public PLibMethodAttribute Descriptor { get; }

	public MethodInfo Method { get; }

	public PLibMethodInstance(PLibMethodAttribute attribute, MethodInfo method)
	{
		Descriptor = attribute ?? throw new ArgumentNullException("attribute");
		Method = method ?? throw new ArgumentNullException("method");
	}

	public void Run(Harmony instance)
	{
		if (!PPatchManager.CheckConditions(Descriptor.RequireAssembly, Descriptor.RequireType, out var requiredType))
		{
			return;
		}
		Type[] parameterTypes = Method.GetParameterTypes();
		int num = parameterTypes.Length;
		if (num <= 0)
		{
			Method.Invoke(null, null);
		}
		else if (parameterTypes[0] == typeof(Harmony))
		{
			switch (num)
			{
			case 1:
				Method.Invoke(null, new object[1] { instance });
				break;
			case 2:
				if (parameterTypes[1] == typeof(Type))
				{
					Method.Invoke(null, new object[2] { instance, requiredType });
				}
				break;
			}
		}
		else
		{
			PUtil.LogWarning("Invalid signature for PLibMethod - must have (), (Harmony), or (Harmony, Type)");
		}
	}
}
