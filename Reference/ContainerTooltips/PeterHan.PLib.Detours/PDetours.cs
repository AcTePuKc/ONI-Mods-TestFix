using System;
using System.Reflection;
using System.Reflection.Emit;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Detours;

public static class PDetours
{
	private sealed class DelegateInfo
	{
		private readonly Type delegateType;

		public readonly Type[] parameterTypes;

		public readonly Type returnType;

		public static DelegateInfo Create(Type delegateType)
		{
			if (delegateType == null)
			{
				throw new ArgumentNullException("delegateType");
			}
			MethodInfo methodSafe = delegateType.GetMethodSafe("Invoke", isStatic: false, PPatchTools.AnyArguments);
			if (methodSafe == null)
			{
				throw new ArgumentException("Invalid delegate type: " + delegateType);
			}
			return new DelegateInfo(delegateType, methodSafe.GetParameterTypes(), methodSafe.ReturnType);
		}

		private DelegateInfo(Type delegateType, Type[] parameterTypes, Type returnType)
		{
			this.delegateType = delegateType;
			this.parameterTypes = parameterTypes;
			this.returnType = returnType;
		}

		public override string ToString()
		{
			return "DelegateInfo[delegate={0},return={1},parameters={2}]".F(delegateType, returnType, parameterTypes.Join());
		}
	}

	public static D Detour<D>(this Type type) where D : Delegate
	{
		return type.Detour<D>(typeof(D).Name);
	}

	public static D Detour<D>(this Type type, string name) where D : Delegate
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		DelegateInfo expected = DelegateInfo.Create(typeof(D));
		MethodInfo methodInfo = null;
		int num = int.MaxValue;
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo2 in array)
		{
			if (!(methodInfo2.Name == name))
			{
				continue;
			}
			try
			{
				int num2 = ValidateDelegate(expected, methodInfo2, methodInfo2.ReturnType).Length;
				if (num2 < num)
				{
					num = num2;
					methodInfo = methodInfo2;
				}
			}
			catch (DetourException)
			{
			}
		}
		if (methodInfo == null)
		{
			throw new DetourException("No match found for {1}.{0}".F(name, type.FullName));
		}
		return methodInfo.Detour<D>();
	}

	public static D DetourConstructor<D>(this Type type) where D : Delegate
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		DelegateInfo expected = DelegateInfo.Create(typeof(D));
		ConstructorInfo constructorInfo = null;
		int num = int.MaxValue;
		ConstructorInfo[] array = constructors;
		foreach (ConstructorInfo constructorInfo2 in array)
		{
			try
			{
				int num2 = ValidateDelegate(expected, constructorInfo2, type).Length;
				if (num2 < num)
				{
					num = num2;
					constructorInfo = constructorInfo2;
				}
			}
			catch (DetourException)
			{
			}
		}
		if (constructorInfo == null)
		{
			throw new DetourException("No match found for {0} constructor".F(type.FullName));
		}
		return constructorInfo.Detour<D>();
	}

	public static DetouredMethod<D> DetourLazy<D>(this Type type, string name) where D : Delegate
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		return new DetouredMethod<D>(type, name);
	}

	public static D Detour<D>(this MethodInfo target) where D : Delegate
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (target.ContainsGenericParameters)
		{
			throw new ArgumentException("Generic types must have all parameters defined");
		}
		DelegateInfo delegateInfo = DelegateInfo.Create(typeof(D));
		Type declaringType = target.DeclaringType;
		Type[] parameterTypes = delegateInfo.parameterTypes;
		ParameterInfo[] actualParams = ValidateDelegate(delegateInfo, target, target.ReturnType);
		int offset = ((!target.IsStatic) ? 1 : 0);
		if (declaringType == null)
		{
			throw new ArgumentException("Method is not declared by an actual type");
		}
		DynamicMethod dynamicMethod = new DynamicMethod(target.Name + "_Detour", delegateInfo.returnType, parameterTypes, declaringType, skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		LoadParameters(iLGenerator, actualParams, parameterTypes, offset);
		if (declaringType.IsValueType || target.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Call, target);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Callvirt, target);
		}
		iLGenerator.Emit(OpCodes.Ret);
		FinishDynamicMethod(dynamicMethod, actualParams, parameterTypes, offset);
		return dynamicMethod.CreateDelegate(typeof(D)) as D;
	}

	public static D Detour<D>(this ConstructorInfo target) where D : Delegate
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (target.ContainsGenericParameters)
		{
			throw new ArgumentException("Generic types must have all parameters defined");
		}
		if (target.IsStatic)
		{
			throw new ArgumentException("Static constructors cannot be called manually");
		}
		DelegateInfo delegateInfo = DelegateInfo.Create(typeof(D));
		Type declaringType = target.DeclaringType;
		Type[] parameterTypes = delegateInfo.parameterTypes;
		ParameterInfo[] actualParams = ValidateDelegate(delegateInfo, target, declaringType);
		if (declaringType == null)
		{
			throw new ArgumentException("Method is not declared by an actual type");
		}
		DynamicMethod dynamicMethod = new DynamicMethod("Constructor_Detour", delegateInfo.returnType, parameterTypes, declaringType, skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		LoadParameters(iLGenerator, actualParams, parameterTypes, 0);
		iLGenerator.Emit(OpCodes.Newobj, target);
		iLGenerator.Emit(OpCodes.Ret);
		FinishDynamicMethod(dynamicMethod, actualParams, parameterTypes, 0);
		return dynamicMethod.CreateDelegate(typeof(D)) as D;
	}

	public static IDetouredField<P, T> DetourField<P, T>(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		Type typeFromHandle = typeof(P);
		FieldInfo field = typeFromHandle.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (field == null)
		{
			try
			{
				PropertyInfo property = typeFromHandle.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (property == null)
				{
					throw new DetourException("Unable to find {0} on type {1}".F(name, typeof(P).FullName));
				}
				return DetourProperty<P, T>(property);
			}
			catch (AmbiguousMatchException)
			{
				throw new DetourException("Unable to find {0} on type {1}".F(name, typeof(P).FullName));
			}
		}
		if (!typeFromHandle.IsValueType)
		{
			if (typeFromHandle.IsByRef)
			{
				Type elementType = typeFromHandle.GetElementType();
				if ((object)elementType != null && elementType.IsValueType)
				{
					goto IL_00ca;
				}
			}
			return DetourField<P, T>(field);
		}
		goto IL_00ca;
		IL_00ca:
		throw new ArgumentException("For accessing struct fields, use DetourStructField");
	}

	public static IDetouredField<P, T> DetourFieldLazy<P, T>(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		return new LazyDetouredField<P, T>(typeof(P), name);
	}

	private static IDetouredField<P, T> DetourField<P, T>(FieldInfo target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		Type declaringType = target.DeclaringType;
		string name = target.Name;
		if (declaringType != typeof(P))
		{
			throw new ArgumentException("Parent type does not match delegate to be created");
		}
		DynamicMethod dynamicMethod = new DynamicMethod(name + "_Detour_Get", typeof(T), new Type[1] { typeof(P) }, restrictedSkipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		if (target.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldsfld, target);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, target);
		}
		iLGenerator.Emit(OpCodes.Ret);
		DynamicMethod dynamicMethod2;
		if (target.IsInitOnly)
		{
			dynamicMethod2 = null;
		}
		else
		{
			dynamicMethod2 = new DynamicMethod(name + "_Detour_Set", null, new Type[2]
			{
				typeof(P),
				typeof(T)
			}, restrictedSkipVisibility: true);
			iLGenerator = dynamicMethod2.GetILGenerator();
			if (target.IsStatic)
			{
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Stsfld, target);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Stfld, target);
			}
			iLGenerator.Emit(OpCodes.Ret);
		}
		return new DetouredField<P, T>(name, dynamicMethod.CreateDelegate(typeof(Func<P, T>)) as Func<P, T>, dynamicMethod2?.CreateDelegate(typeof(Action<P, T>)) as Action<P, T>);
	}

	private static IDetouredField<P, T> DetourProperty<P, T>(PropertyInfo target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		Type declaringType = target.DeclaringType;
		string name = target.Name;
		if (declaringType != typeof(P))
		{
			throw new ArgumentException("Parent type does not match delegate to be created");
		}
		ParameterInfo[] indexParameters = target.GetIndexParameters();
		if (indexParameters != null && indexParameters.Length != 0)
		{
			throw new DetourException("Cannot detour on properties with index arguments");
		}
		MethodInfo getMethod = target.GetGetMethod(nonPublic: true);
		DynamicMethod dynamicMethod;
		if (target.CanRead && getMethod != null)
		{
			dynamicMethod = new DynamicMethod(name + "_Detour_Get", typeof(T), new Type[1] { typeof(P) }, restrictedSkipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			if (!getMethod.IsStatic)
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
			}
			iLGenerator.Emit(OpCodes.Call, getMethod);
			iLGenerator.Emit(OpCodes.Ret);
		}
		else
		{
			dynamicMethod = null;
		}
		MethodInfo setMethod = target.GetSetMethod(nonPublic: true);
		DynamicMethod dynamicMethod2;
		if (target.CanWrite && setMethod != null)
		{
			dynamicMethod2 = new DynamicMethod(name + "_Detour_Set", null, new Type[2]
			{
				typeof(P),
				typeof(T)
			}, restrictedSkipVisibility: true);
			ILGenerator iLGenerator2 = dynamicMethod2.GetILGenerator();
			if (!setMethod.IsStatic)
			{
				iLGenerator2.Emit(OpCodes.Ldarg_0);
			}
			iLGenerator2.Emit(OpCodes.Ldarg_1);
			iLGenerator2.Emit(OpCodes.Call, setMethod);
			iLGenerator2.Emit(OpCodes.Ret);
		}
		else
		{
			dynamicMethod2 = null;
		}
		return new DetouredField<P, T>(name, dynamicMethod?.CreateDelegate(typeof(Func<P, T>)) as Func<P, T>, dynamicMethod2?.CreateDelegate(typeof(Action<P, T>)) as Action<P, T>);
	}

	public static IDetouredField<object, T> DetourStructField<T>(this Type parentType, string name)
	{
		if (parentType == null)
		{
			throw new ArgumentNullException("parentType");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		FieldInfo field = parentType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field == null)
		{
			throw new DetourException("Unable to find {0} on type {1}".F(name, parentType.FullName));
		}
		DynamicMethod dynamicMethod = new DynamicMethod(name + "_Detour_Get", typeof(T), new Type[1] { typeof(object) }, restrictedSkipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Unbox, parentType);
		iLGenerator.Emit(OpCodes.Ldfld, field);
		iLGenerator.Emit(OpCodes.Ret);
		DynamicMethod dynamicMethod2;
		if (field.IsInitOnly)
		{
			dynamicMethod2 = null;
		}
		else
		{
			dynamicMethod2 = new DynamicMethod(name + "_Detour_Set", null, new Type[2]
			{
				typeof(object),
				typeof(T)
			}, restrictedSkipVisibility: true);
			ILGenerator iLGenerator2 = dynamicMethod2.GetILGenerator();
			iLGenerator2.Emit(OpCodes.Ldarg_0);
			iLGenerator2.Emit(OpCodes.Unbox, parentType);
			iLGenerator2.Emit(OpCodes.Ldarg_1);
			iLGenerator2.Emit(OpCodes.Stfld, field);
			iLGenerator2.Emit(OpCodes.Ret);
		}
		return new DetouredField<object, T>(name, dynamicMethod.CreateDelegate(typeof(Func<object, T>)) as Func<object, T>, dynamicMethod2?.CreateDelegate(typeof(Action<object, T>)) as Action<object, T>);
	}

	private static void FinishDynamicMethod(DynamicMethod caller, ParameterInfo[] actualParams, Type[] expectedParams, int offset)
	{
		int num = expectedParams.Length;
		if (offset > 0)
		{
			caller.DefineParameter(1, ParameterAttributes.None, "this");
		}
		for (int i = offset; i < num; i++)
		{
			ParameterInfo parameterInfo = actualParams[i - offset];
			caller.DefineParameter(i + 1, parameterInfo.Attributes, parameterInfo.Name);
		}
	}

	private static void LoadParameters(ILGenerator generator, ParameterInfo[] actualParams, Type[] expectedParams, int offset)
	{
		int num = expectedParams.Length;
		int num2 = actualParams.Length + offset;
		if (num > 0)
		{
			generator.Emit(OpCodes.Ldarg_0);
		}
		if (num > 1)
		{
			generator.Emit(OpCodes.Ldarg_1);
		}
		if (num > 2)
		{
			generator.Emit(OpCodes.Ldarg_2);
		}
		if (num > 3)
		{
			generator.Emit(OpCodes.Ldarg_3);
		}
		for (int i = 4; i < num; i++)
		{
			generator.Emit(OpCodes.Ldarg_S, i);
		}
		for (int j = num; j < num2; j++)
		{
			ParameterInfo parameterInfo = actualParams[j - offset];
			PTranspilerTools.GenerateDefaultLoad(generator, parameterInfo.ParameterType, parameterInfo.DefaultValue);
		}
	}

	private static ParameterInfo[] ValidateDelegate(DelegateInfo expected, MethodBase actual, Type actualReturn)
	{
		Type[] parameterTypes = expected.parameterTypes;
		Type returnType = expected.returnType;
		if (!returnType.IsAssignableFrom(actualReturn))
		{
			throw new DetourException("Return type {0} cannot be converted to type {1}".F(actualReturn.FullName, returnType.FullName));
		}
		Type declaringType = actual.DeclaringType;
		if (declaringType == null)
		{
			throw new ArgumentException("Method is not declared by an actual type");
		}
		if (declaringType.ContainsGenericParameters)
		{
			throw new DetourException("Method parent type {0} must have all generic parameters defined".F(declaringType.FullName));
		}
		string text = declaringType.FullName + "." + actual.Name;
		ParameterInfo[] parameters = actual.GetParameters();
		int num = parameters.Length;
		int num2 = parameterTypes.Length;
		Type[] array = new Type[num];
		bool flag = actual.IsStatic || actual.IsConstructor;
		for (int i = 0; i < num; i++)
		{
			array[i] = parameters[i].ParameterType;
		}
		Type[] array2;
		if (flag)
		{
			array2 = array;
		}
		else
		{
			array2 = PTranspilerTools.PushDeclaringType(array, declaringType);
			num++;
		}
		if (num2 > num)
		{
			throw new DetourException("Method {0} has only {1:D} parameters, but {2:D} were supplied".F(actual.ToString(), num, num2));
		}
		for (int j = 0; j < num2; j++)
		{
			Type type = array2[j];
			Type type2 = parameterTypes[j];
			if (!type.IsAssignableFrom(type2))
			{
				throw new DetourException("Argument {0:D} for method {3} cannot be converted from {1} to {2}".F(j, type.FullName, type2.FullName, text));
			}
		}
		int num3 = ((!flag) ? 1 : 0);
		for (int k = num2; k < num; k++)
		{
			ParameterInfo parameterInfo = parameters[k - num3];
			if (!parameterInfo.IsOptional)
			{
				throw new DetourException("New argument {0:D} for method {1} ({2}) is not optional".F(k, text, parameterInfo.ParameterType.FullName));
			}
		}
		return parameters;
	}
}
