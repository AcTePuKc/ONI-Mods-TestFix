using System;
using System.Collections.Generic;
using System.Reflection;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;
using UnityEngine;

namespace PeterHan.PLib.Options;

public static class OptionsHandlers
{
	private delegate IOptionsEntry CreateOption(string field, IOptionSpec spec);

	private delegate IOptionsEntry CreateOptionType(string field, IOptionSpec spec, Type fieldType);

	private delegate IOptionsEntry CreateOptionLimit(string field, IOptionSpec spec, LimitAttribute limit);

	private static readonly IDictionary<Type, Delegate> OPTIONS_HANDLERS = new Dictionary<Type, Delegate>(64);

	public static void AddOptionClass(Type optionType, Type handlerType)
	{
		if (!(optionType != null) || !(handlerType != null) || OPTIONS_HANDLERS.ContainsKey(optionType) || !typeof(IOptionsEntry).IsAssignableFrom(handlerType))
		{
			return;
		}
		ConstructorInfo[] constructors = handlerType.GetConstructors();
		int num = constructors.Length;
		for (int i = 0; i < num; i++)
		{
			Delegate obj = CreateDelegate(handlerType, constructors[i]);
			if ((object)obj != null)
			{
				OPTIONS_HANDLERS[optionType] = obj;
				break;
			}
		}
	}

	private static Delegate CreateDelegate(Type handlerType, ConstructorInfo constructor)
	{
		ParameterInfo[] parameters = constructor.GetParameters();
		int num = parameters.Length;
		Delegate result = null;
		if (num > 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(string)) && parameters[1].ParameterType.IsAssignableFrom(typeof(IOptionSpec)))
		{
			switch (num)
			{
			case 2:
				result = constructor.Detour<CreateOption>();
				break;
			case 3:
			{
				Type parameterType = parameters[2].ParameterType;
				if (parameterType.IsAssignableFrom(typeof(LimitAttribute)))
				{
					result = constructor.Detour<CreateOptionLimit>();
				}
				else if (parameterType.IsAssignableFrom(typeof(Type)))
				{
					result = constructor.Detour<CreateOptionType>();
				}
				break;
			}
			default:
				PUtil.LogWarning("Constructor on options handler type " + handlerType?.ToString() + " cannot be constructed by OptionsHandlers");
				break;
			}
		}
		return result;
	}

	public static IOptionsEntry FindOptionClass(IOptionSpec spec, PropertyInfo info)
	{
		IOptionsEntry result = null;
		if (spec != null && info != null)
		{
			Type propertyType = info.PropertyType;
			string name = info.Name;
			Delegate value;
			if (propertyType.IsEnum)
			{
				result = new SelectOneOptionsEntry(name, spec, propertyType);
			}
			else if (OPTIONS_HANDLERS.TryGetValue(propertyType, out value))
			{
				if (value is CreateOption createOption)
				{
					result = createOption(name, spec);
				}
				else if (value is CreateOptionLimit createOptionLimit)
				{
					result = createOptionLimit(name, spec, info.GetCustomAttribute<LimitAttribute>());
				}
				else if (value is CreateOptionType createOptionType)
				{
					result = createOptionType(name, spec, propertyType);
				}
			}
		}
		return result;
	}

	internal static void InitPredefinedOptions()
	{
		if (OPTIONS_HANDLERS.Count < 1)
		{
			AddOptionClass(typeof(bool), typeof(CheckboxOptionsEntry));
			AddOptionClass(typeof(int), typeof(IntOptionsEntry));
			AddOptionClass(typeof(int?), typeof(NullableIntOptionsEntry));
			AddOptionClass(typeof(float), typeof(FloatOptionsEntry));
			AddOptionClass(typeof(float?), typeof(NullableFloatOptionsEntry));
			AddOptionClass(typeof(Color32), typeof(Color32OptionsEntry));
			AddOptionClass(typeof(Color), typeof(ColorOptionsEntry));
			AddOptionClass(typeof(string), typeof(StringOptionsEntry));
			AddOptionClass(typeof(Action<object>), typeof(ButtonOptionsEntry));
			AddOptionClass(typeof(LocText), typeof(TextBlockOptionsEntry));
		}
	}
}
