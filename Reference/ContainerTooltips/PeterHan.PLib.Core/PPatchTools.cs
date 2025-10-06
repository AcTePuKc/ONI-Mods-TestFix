using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace PeterHan.PLib.Core;

public static class PPatchTools
{
	public const BindingFlags BASE_FLAGS = BindingFlags.Public | BindingFlags.NonPublic;

	public static readonly MethodInfo RemoveCall = typeof(PPatchTools).GetMethodSafe("RemoveMethodCallPrivate", true);

	public static Type[] AnyArguments => new Type[1];

	public static T CreateDelegate<T>(this Type type, string method, object caller, params Type[] argumentTypes) where T : Delegate
	{
		T result = null;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(method))
		{
			throw new ArgumentNullException("method");
		}
		MethodInfo methodSafe = type.GetMethodSafe(method, isStatic: false, argumentTypes);
		if (methodSafe != null)
		{
			return Delegate.CreateDelegate(typeof(T), caller, methodSafe, throwOnBindFailure: false) as T;
		}
		return result;
	}

	public static T CreateDelegate<T>(this MethodInfo method, object caller) where T : Delegate
	{
		T result = null;
		if (method != null)
		{
			return Delegate.CreateDelegate(typeof(T), caller, method, throwOnBindFailure: false) as T;
		}
		return result;
	}

	public static Func<T> CreateGetDelegate<T>(this Type type, string property, object caller)
	{
		Func<T> result = null;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(property))
		{
			throw new ArgumentNullException("property");
		}
		MethodInfo methodInfo = type.GetPropertySafe<T>(property, isStatic: false)?.GetGetMethod(nonPublic: true);
		if (methodInfo != null)
		{
			result = Delegate.CreateDelegate(typeof(Func<T>), caller, methodInfo, throwOnBindFailure: false) as Func<T>;
		}
		return result;
	}

	public static Func<T> CreateGetDelegate<T>(this PropertyInfo property, object caller)
	{
		Func<T> result = null;
		MethodInfo methodInfo = property?.GetGetMethod(nonPublic: true);
		if (methodInfo != null && typeof(T).IsAssignableFrom(property.PropertyType))
		{
			result = Delegate.CreateDelegate(typeof(Func<T>), caller, methodInfo, throwOnBindFailure: false) as Func<T>;
		}
		return result;
	}

	public static Action<T> CreateSetDelegate<T>(this Type type, string property, object caller)
	{
		Action<T> result = null;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(property))
		{
			throw new ArgumentNullException("property");
		}
		MethodInfo methodInfo = type.GetPropertySafe<T>(property, isStatic: false)?.GetSetMethod(nonPublic: true);
		if (methodInfo != null)
		{
			result = Delegate.CreateDelegate(typeof(Action<T>), caller, methodInfo, throwOnBindFailure: false) as Action<T>;
		}
		return result;
	}

	public static Action<T> CreateSetDelegate<T>(this PropertyInfo property, object caller)
	{
		Action<T> result = null;
		MethodInfo methodInfo = property?.GetSetMethod(nonPublic: true);
		if (methodInfo != null && property.PropertyType.IsAssignableFrom(typeof(T)))
		{
			result = Delegate.CreateDelegate(typeof(Action<T>), caller, methodInfo, throwOnBindFailure: false) as Action<T>;
		}
		return result;
	}

	public static T CreateStaticDelegate<T>(this Type type, string method, params Type[] argumentTypes) where T : Delegate
	{
		T result = null;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(method))
		{
			throw new ArgumentNullException("method");
		}
		MethodInfo methodSafe = type.GetMethodSafe(method, isStatic: true, argumentTypes);
		if (methodSafe != null)
		{
			return Delegate.CreateDelegate(typeof(T), methodSafe, throwOnBindFailure: false) as T;
		}
		return result;
	}

	private static IEnumerable<CodeInstruction> DoReplaceMethodCalls(IEnumerable<CodeInstruction> method, IDictionary<MethodInfo, MethodInfo> translation)
	{
		MethodInfo remove = RemoveCall;
		int replaced = 0;
		foreach (CodeInstruction item in method)
		{
			OpCode opcode = item.opcode;
			if ((opcode == OpCodes.Call || opcode == OpCodes.Calli || opcode == OpCodes.Callvirt) && item.operand is MethodInfo methodInfo && translation.TryGetValue(methodInfo, out var value))
			{
				if (value != null && value != remove)
				{
					item.opcode = (value.IsStatic ? OpCodes.Call : OpCodes.Callvirt);
					item.operand = value;
					yield return item;
				}
				else
				{
					int n = methodInfo.GetParameters().Length;
					if (!methodInfo.IsStatic)
					{
						n++;
					}
					item.opcode = ((n == 0) ? OpCodes.Nop : OpCodes.Pop);
					item.operand = null;
					yield return item;
					for (int i = 0; i < n - 1; i++)
					{
						yield return new CodeInstruction(OpCodes.Pop, (object)null);
					}
				}
				replaced++;
			}
			else
			{
				yield return item;
			}
		}
	}

	public static void DumpMethodBody(IEnumerable<CodeInstruction> opcodes)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder(1024);
		stringBuilder.AppendLine("METHOD BODY:");
		foreach (CodeInstruction opcode in opcodes)
		{
			foreach (ExceptionBlock block in opcode.blocks)
			{
				ExceptionBlockType blockType = block.blockType;
				if ((int)blockType == 5)
				{
					stringBuilder.AppendLine("}");
					continue;
				}
				if ((int)blockType != 0)
				{
					stringBuilder.Append("} ");
				}
				stringBuilder.Append(block.blockType);
				stringBuilder.AppendLine(" {");
			}
			foreach (Label label in opcode.labels)
			{
				stringBuilder.Append(label.GetHashCode());
				stringBuilder.Append(": ");
			}
			stringBuilder.Append('\t');
			stringBuilder.Append(opcode.opcode);
			object operand = opcode.operand;
			if (operand != null)
			{
				stringBuilder.Append('\t');
				if (operand is Label)
				{
					stringBuilder.Append(operand.GetHashCode());
				}
				else if (operand is MethodBase method)
				{
					FormatMethodCall(stringBuilder, method);
				}
				else
				{
					stringBuilder.Append(FormatArgument(operand));
				}
			}
			stringBuilder.AppendLine();
		}
		PUtil.LogDebug(stringBuilder.ToString());
	}

	private static string FormatArgument(object argument)
	{
		if (argument == null)
		{
			return "NULL";
		}
		if (argument is MethodBase methodBase)
		{
			return GeneralExtensions.FullDescription(methodBase);
		}
		if (argument is FieldInfo fieldInfo)
		{
			return GeneralExtensions.FullDescription(fieldInfo.FieldType) + " " + GeneralExtensions.FullDescription(fieldInfo.DeclaringType) + "::" + fieldInfo.Name;
		}
		if (argument is Label label)
		{
			return $"Label{label.GetHashCode()}";
		}
		if (argument is Label[] array)
		{
			int num = array.Length;
			string[] array2 = new string[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = array[i].GetHashCode().ToString();
			}
			return "Labels" + array2.Join();
		}
		if (argument is LocalBuilder localBuilder)
		{
			return $"{localBuilder.LocalIndex} ({localBuilder.LocalType})";
		}
		if (argument is string text)
		{
			return GeneralExtensions.ToLiteral(text, "\"");
		}
		return argument.ToString().Trim();
	}

	private static void FormatMethodCall(StringBuilder result, MethodBase method)
	{
		bool flag = true;
		Type declaringType = method.DeclaringType;
		if (method is MethodInfo methodInfo)
		{
			result.Append(methodInfo.ReturnType.Name);
			result.Append(' ');
		}
		if (declaringType != null)
		{
			result.Append(declaringType.Name);
			result.Append('.');
		}
		result.Append(method.Name);
		result.Append('(');
		ParameterInfo[] parameters = method.GetParameters();
		foreach (ParameterInfo parameterInfo in parameters)
		{
			string name = parameterInfo.Name;
			if (!flag)
			{
				result.Append(", ");
			}
			result.Append(parameterInfo.ParameterType.Name);
			if (!string.IsNullOrEmpty(name))
			{
				result.Append(' ');
				result.Append(name);
			}
			if (parameterInfo.IsOptional)
			{
				result.Append(" = ");
				result.Append(parameterInfo.DefaultValue);
			}
			flag = false;
		}
		result.Append(')');
	}

	public static FieldInfo GetFieldSafe(this Type type, string fieldName, bool isStatic)
	{
		FieldInfo result = null;
		if (type != null && !string.IsNullOrEmpty(fieldName))
		{
			try
			{
				BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
				result = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags);
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
		}
		return result;
	}

	public static CodeInstruction GetMatchingStoreInstruction(CodeInstruction load)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		OpCode opcode = load.opcode;
		if (opcode == OpCodes.Ldloc)
		{
			return new CodeInstruction(OpCodes.Stloc, load.operand);
		}
		if (opcode == OpCodes.Ldloc_S)
		{
			return new CodeInstruction(OpCodes.Stloc_S, load.operand);
		}
		if (opcode == OpCodes.Ldloc_0)
		{
			return new CodeInstruction(OpCodes.Stloc_0, (object)null);
		}
		if (opcode == OpCodes.Ldloc_1)
		{
			return new CodeInstruction(OpCodes.Stloc_1, (object)null);
		}
		if (opcode == OpCodes.Ldloc_2)
		{
			return new CodeInstruction(OpCodes.Stloc_2, (object)null);
		}
		if (opcode == OpCodes.Ldloc_3)
		{
			return new CodeInstruction(OpCodes.Stloc_3, (object)null);
		}
		return new CodeInstruction(OpCodes.Pop, (object)null);
	}

	public static MethodInfo GetMethodSafe(this Type type, string methodName, bool isStatic, params Type[] arguments)
	{
		MethodInfo result = null;
		if (type != null && arguments != null && !string.IsNullOrEmpty(methodName))
		{
			try
			{
				BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
				result = ((arguments.Length != 1 || !(arguments[0] == null)) ? type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags, null, arguments, new ParameterModifier[arguments.Length]) : type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags));
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
		}
		return result;
	}

	public static MethodInfo GetOverloadWithMostArguments(this Type type, string methodName, bool isStatic, params Type[] arguments)
	{
		MethodInfo result = null;
		if (type != null && arguments != null && !string.IsNullOrEmpty(methodName))
		{
			MethodInfo[] methods = type.GetMethods((BindingFlags)(0x30 | (isStatic ? 8 : 4)));
			int num = methods.Length;
			int num2 = -1;
			for (int i = 0; i < num; i++)
			{
				MethodInfo methodInfo = methods[i];
				int num3;
				if (methodInfo.Name == methodName && (num3 = ParametersMatch(methodInfo, arguments)) > num2)
				{
					num2 = num3;
					result = methodInfo;
				}
			}
		}
		return result;
	}

	public static PropertyInfo GetPropertySafe<T>(this Type type, string propName, bool isStatic)
	{
		PropertyInfo result = null;
		if (type != null && !string.IsNullOrEmpty(propName))
		{
			try
			{
				BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
				result = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags, null, typeof(T), Type.EmptyTypes, null);
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
		}
		return result;
	}

	public static PropertyInfo GetPropertyIndexedSafe<T>(this Type type, string propName, bool isStatic, params Type[] arguments)
	{
		PropertyInfo result = null;
		if (type != null && arguments != null && !string.IsNullOrEmpty(propName))
		{
			try
			{
				BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
				result = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags, null, typeof(T), arguments, new ParameterModifier[arguments.Length]);
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
		}
		return result;
	}

	public static Type GetTypeSafe(string name, string assemblyName = null)
	{
		Type type = null;
		if (string.IsNullOrEmpty(assemblyName))
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				try
				{
					type = assembly.GetType(name, throwOnError: false);
				}
				catch (IOException)
				{
				}
				catch (BadImageFormatException)
				{
				}
				if (type != null)
				{
					break;
				}
			}
		}
		else
		{
			try
			{
				type = Type.GetType(name + ", " + assemblyName, throwOnError: false);
			}
			catch (TargetInvocationException thrown)
			{
				PUtil.LogWarning("Unable to load type {0} from assembly {1}:".F(name, assemblyName));
				PUtil.LogExcWarn(thrown);
			}
			catch (ArgumentException thrown2)
			{
				PUtil.LogWarning("Unable to load type {0} from assembly {1}:".F(name, assemblyName));
				PUtil.LogExcWarn(thrown2);
			}
			catch (ReflectionTypeLoadException ex3)
			{
				PUtil.LogWarning("Unable to load type {0} from assembly {1}:".F(name, assemblyName));
				Exception[] loaderExceptions = ex3.LoaderExceptions;
				foreach (Exception ex4 in loaderExceptions)
				{
					if (ex4 != null)
					{
						PUtil.LogExcWarn(ex4);
					}
				}
			}
			catch (IOException)
			{
			}
			catch (BadImageFormatException)
			{
			}
		}
		return type;
	}

	public static bool HasPatchWithMethodName(Harmony instance, MethodBase target, HarmonyPatchType type, string name)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected I4, but got Unknown
		bool flag = false;
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		Patches patchInfo = Harmony.GetPatchInfo(target);
		if (patchInfo != null)
		{
			ICollection<Patch> collection;
			switch ((int)type)
			{
			case 1:
				collection = patchInfo.Prefixes;
				break;
			case 2:
				collection = patchInfo.Postfixes;
				break;
			case 3:
				collection = patchInfo.Transpilers;
				break;
			default:
				if (patchInfo.Transpilers != null)
				{
					flag = HasPatchWithMethodName(patchInfo.Transpilers, name);
				}
				if (patchInfo.Prefixes != null)
				{
					flag = flag || HasPatchWithMethodName(patchInfo.Prefixes, name);
				}
				collection = patchInfo.Postfixes;
				break;
			}
			if (collection != null)
			{
				flag = flag || HasPatchWithMethodName(collection, name);
			}
		}
		return flag;
	}

	private static bool HasPatchWithMethodName(IEnumerable<Patch> patchList, string name)
	{
		bool result = false;
		foreach (Patch patch in patchList)
		{
			if (patch.PatchMethod.Name == name)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static bool IsConditionalBranchInstruction(this OpCode opcode)
	{
		return PTranspilerTools.IsConditionalBranchInstruction(opcode);
	}

	[Obsolete("Do not use this method in production code. Make sure to remove it in release builds, or disable it with #if DEBUG.")]
	public static void LogAllExceptions()
	{
		PUtil.LogWarning("PLib in mod " + Assembly.GetCallingAssembly().GetName()?.Name + " is logging ALL unhandled exceptions!");
		PTranspilerTools.LogAllExceptions();
	}

	[Obsolete("Do not use this method in production code. Make sure to remove it in release builds, or disable it with #if DEBUG.")]
	public static void LogAllFailedAsserts()
	{
		PUtil.LogWarning("PLib in mod " + Assembly.GetCallingAssembly().GetName()?.Name + " is logging ALL failed assertions!");
		PTranspilerTools.LogAllFailedAsserts();
	}

	private static int ParametersMatch(MethodBase method, Type[] required)
	{
		int result = -1;
		ParameterInfo[] parameters = method.GetParameters();
		int num = parameters.Length;
		int num2 = required.Length;
		if (num2 == 1 && required[0] == null)
		{
			result = num;
		}
		else if (num >= num2)
		{
			bool flag = true;
			for (int i = 0; i < num2 && flag; i++)
			{
				flag = parameters[i].ParameterType == required[i];
			}
			if (flag)
			{
				result = num;
			}
		}
		return result;
	}

	public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, double oldValue, double newValue, bool all = false)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		int replaced = 0;
		foreach (CodeInstruction item in method)
		{
			if (item.opcode == OpCodes.Ldc_R8 && item.operand is double num && num == oldValue)
			{
				if (all || replaced == 0)
				{
					item.operand = newValue;
				}
				replaced++;
			}
			yield return item;
		}
	}

	public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, float oldValue, float newValue, bool all = false)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		int replaced = 0;
		foreach (CodeInstruction item in method)
		{
			if (item.opcode == OpCodes.Ldc_R4 && item.operand is float num && num == oldValue)
			{
				if (all || replaced == 0)
				{
					item.operand = newValue;
				}
				replaced++;
			}
			yield return item;
		}
	}

	public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, int oldValue, int newValue, bool all = false)
	{
		int replaced = 0;
		bool quickCode = oldValue >= -1 && oldValue <= 8;
		OpCode qc = OpCodes.Nop;
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (quickCode)
		{
			qc = PTranspilerTools.LOAD_INT[oldValue + 1];
		}
		foreach (CodeInstruction item in method)
		{
			OpCode opcode = item.opcode;
			object operand = item.operand;
			if ((opcode == OpCodes.Ldc_I4 && operand is int num && num == oldValue) || (opcode == OpCodes.Ldc_I4_S && operand is byte b && b == oldValue) || (quickCode && qc == opcode))
			{
				if (all || replaced == 0)
				{
					PTranspilerTools.ModifyLoadI4(item, newValue);
				}
				replaced++;
			}
			yield return item;
		}
	}

	public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, long oldValue, long newValue, bool all = false)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		int replaced = 0;
		foreach (CodeInstruction item in method)
		{
			if (item.opcode == OpCodes.Ldc_I8 && item.operand is long num && num == oldValue)
			{
				if (all || replaced == 0)
				{
					item.operand = newValue;
				}
				replaced++;
			}
			yield return item;
		}
	}

	public static IEnumerable<CodeInstruction> RemoveMethodCall(IEnumerable<CodeInstruction> method, MethodInfo victim)
	{
		return ReplaceMethodCallSafe(method, new Dictionary<MethodInfo, MethodInfo> { { victim, RemoveCall } });
	}

	private static void RemoveMethodCallPrivate()
	{
	}

	[Obsolete("This method is unsafe. Use the RemoveMethodCall or ReplaceMethodCallSafe versions instead.")]
	public static IEnumerable<CodeInstruction> ReplaceMethodCall(IEnumerable<CodeInstruction> method, MethodInfo victim, MethodInfo newMethod = null)
	{
		if (newMethod == null)
		{
			newMethod = RemoveCall;
		}
		return ReplaceMethodCallSafe(method, new Dictionary<MethodInfo, MethodInfo> { { victim, newMethod } });
	}

	public static IEnumerable<CodeInstruction> ReplaceMethodCallSafe(IEnumerable<CodeInstruction> method, MethodInfo victim, MethodInfo newMethod)
	{
		if (newMethod == null)
		{
			throw new ArgumentNullException("newMethod");
		}
		return ReplaceMethodCallSafe(method, new Dictionary<MethodInfo, MethodInfo> { { victim, newMethod } });
	}

	[Obsolete("This method is unsafe. Use ReplaceMethodCallSafe instead.")]
	public static IEnumerable<CodeInstruction> ReplaceMethodCall(IEnumerable<CodeInstruction> method, IDictionary<MethodInfo, MethodInfo> translation)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (translation == null)
		{
			throw new ArgumentNullException("translation");
		}
		foreach (KeyValuePair<MethodInfo, MethodInfo> item in translation)
		{
			MethodInfo key = item.Key;
			MethodInfo value = item.Value;
			if (key == null)
			{
				throw new ArgumentNullException("victim");
			}
			if (value != null)
			{
				PTranspilerTools.CompareMethodParams(key, key.GetParameterTypes(), value);
			}
			else if (key.ReturnType != typeof(void))
			{
				throw new ArgumentException("Cannot remove method {0} with a return value".F(key.Name));
			}
		}
		return DoReplaceMethodCalls(method, translation);
	}

	public static IEnumerable<CodeInstruction> ReplaceMethodCallSafe(IEnumerable<CodeInstruction> method, IDictionary<MethodInfo, MethodInfo> translation)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (translation == null)
		{
			throw new ArgumentNullException("translation");
		}
		MethodInfo removeCall = RemoveCall;
		foreach (KeyValuePair<MethodInfo, MethodInfo> item in translation)
		{
			MethodInfo key = item.Key;
			MethodInfo value = item.Value;
			if (key == null)
			{
				throw new ArgumentNullException("victim");
			}
			if (value == null)
			{
				throw new ArgumentNullException("newMethod");
			}
			if (value == removeCall)
			{
				if (key.ReturnType != typeof(void))
				{
					throw new ArgumentException("Cannot remove method {0} with a return value".F(key.Name));
				}
			}
			else
			{
				PTranspilerTools.CompareMethodParams(key, key.GetParameterTypes(), value);
			}
		}
		return DoReplaceMethodCalls(method, translation);
	}

	public static bool TryGetFieldValue<T>(Type type, string name, out T value)
	{
		bool result = false;
		if (type != null && !string.IsNullOrEmpty(name))
		{
			FieldInfo fieldSafe = type.GetFieldSafe(name, isStatic: true);
			if (fieldSafe != null && fieldSafe.GetValue(null) is T val)
			{
				result = true;
				value = val;
			}
			else
			{
				value = default(T);
			}
		}
		else
		{
			value = default(T);
		}
		return result;
	}

	public static bool TryGetFieldValue<T>(object source, string name, out T value)
	{
		if (source != null && !string.IsNullOrEmpty(name))
		{
			FieldInfo fieldSafe = source.GetType().GetFieldSafe(name, isStatic: false);
			if (fieldSafe != null && fieldSafe.GetValue(source) is T val)
			{
				value = val;
				return true;
			}
		}
		value = default(T);
		return false;
	}

	public static bool TryGetPropertyValue<T>(object source, string name, out T value)
	{
		if (source != null && !string.IsNullOrEmpty(name))
		{
			PropertyInfo propertySafe = source.GetType().GetPropertySafe<T>(name, isStatic: false);
			if (propertySafe != null && propertySafe.GetIndexParameters().Length < 1 && propertySafe.GetValue(source, null) is T val)
			{
				value = val;
				return true;
			}
		}
		value = default(T);
		return false;
	}

	public static IEnumerable<CodeInstruction> WrapWithErrorLogger(IEnumerable<CodeInstruction> method, ILGenerator generator)
	{
		MethodInfo logger = typeof(PUtil).GetMethodSafe("LogException", true, typeof(Exception));
		IEnumerator<CodeInstruction> ee = method.GetEnumerator();
		if (ee.MoveNext())
		{
			bool isFirst = true;
			Label endMethod = generator.DefineLabel();
			CodeInstruction last;
			bool hasNext;
			do
			{
				last = ee.Current;
				if (isFirst)
				{
					last.blocks.Add(new ExceptionBlock((ExceptionBlockType)0, (Type)null));
				}
				hasNext = ee.MoveNext();
				isFirst = false;
				if (hasNext)
				{
					yield return last;
				}
			}
			while (hasNext);
			last.opcode = OpCodes.Nop;
			last.operand = null;
			yield return last;
			yield return new CodeInstruction(OpCodes.Leave, (object)endMethod);
			CodeInstruction val = ((logger != null) ? new CodeInstruction(OpCodes.Call, (object)logger) : new CodeInstruction(OpCodes.Pop, (object)null));
			val.blocks.Add(new ExceptionBlock((ExceptionBlockType)1, typeof(Exception)));
			yield return val;
			yield return new CodeInstruction(OpCodes.Rethrow, (object)null);
			CodeInstruction val2 = new CodeInstruction(OpCodes.Leave, (object)endMethod);
			val2.blocks.Add(new ExceptionBlock((ExceptionBlockType)5, (Type)null));
			yield return val2;
			CodeInstruction val3 = new CodeInstruction(OpCodes.Ret, (object)null);
			val3.labels.Add(endMethod);
			yield return val3;
		}
		ee.Dispose();
	}
}
