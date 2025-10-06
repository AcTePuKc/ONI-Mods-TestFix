using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace PeterHan.PLib.Core;

internal static class PTranspilerTools
{
	private static readonly ISet<OpCode> BRANCH_CODES;

	internal static readonly OpCode[] LOAD_INT;

	static PTranspilerTools()
	{
		LOAD_INT = new OpCode[10]
		{
			OpCodes.Ldc_I4_M1,
			OpCodes.Ldc_I4_0,
			OpCodes.Ldc_I4_1,
			OpCodes.Ldc_I4_2,
			OpCodes.Ldc_I4_3,
			OpCodes.Ldc_I4_4,
			OpCodes.Ldc_I4_5,
			OpCodes.Ldc_I4_6,
			OpCodes.Ldc_I4_7,
			OpCodes.Ldc_I4_8
		};
		BRANCH_CODES = new HashSet<OpCode>
		{
			OpCodes.Beq,
			OpCodes.Beq_S,
			OpCodes.Bge,
			OpCodes.Bge_S,
			OpCodes.Bge_Un,
			OpCodes.Bge_Un_S,
			OpCodes.Bgt,
			OpCodes.Bgt_S,
			OpCodes.Bgt_Un,
			OpCodes.Bgt_Un_S,
			OpCodes.Ble,
			OpCodes.Ble_S,
			OpCodes.Ble_Un,
			OpCodes.Ble_Un_S,
			OpCodes.Blt,
			OpCodes.Blt_S,
			OpCodes.Blt_Un,
			OpCodes.Blt_Un_S,
			OpCodes.Bne_Un,
			OpCodes.Bne_Un_S,
			OpCodes.Brfalse,
			OpCodes.Brfalse_S,
			OpCodes.Brtrue,
			OpCodes.Brtrue_S
		};
	}

	internal static void CompareMethodParams(MethodInfo victim, Type[] paramTypes, MethodInfo newMethod)
	{
		Type[] array = newMethod.GetParameterTypes();
		if (!newMethod.IsStatic)
		{
			array = PushDeclaringType(array, newMethod.DeclaringType);
		}
		if (!victim.IsStatic)
		{
			paramTypes = PushDeclaringType(paramTypes, victim.DeclaringType);
		}
		int num = paramTypes.Length;
		if (array.Length != num)
		{
			throw new ArgumentException("New method {0} ({1:D} arguments) does not match method {2} ({3:D} arguments)".F(newMethod.Name, array.Length, victim.Name, num));
		}
		for (int i = 0; i < num; i++)
		{
			if (!array[i].IsAssignableFrom(paramTypes[i]))
			{
				throw new ArgumentException("Argument {0:D}: New method type {1} does not match old method type {2}".F(i, paramTypes[i].FullName, array[i].FullName));
			}
		}
		if (!victim.ReturnType.IsAssignableFrom(newMethod.ReturnType))
		{
			throw new ArgumentException("New method {0} (returns {1}) does not match method {2} (returns {3})".F(newMethod.Name, newMethod.ReturnType, victim.Name, victim.ReturnType));
		}
	}

	private static bool GenerateBasicLoad(ILGenerator generator, Type type, object value)
	{
		bool flag = !type.IsByRef;
		if (flag)
		{
			if (type == typeof(int))
			{
				if (value is int arg)
				{
					generator.Emit(OpCodes.Ldc_I4, arg);
				}
				else
				{
					generator.Emit(OpCodes.Ldc_I4_0);
				}
			}
			else if (type == typeof(char))
			{
				if (value is char arg2)
				{
					generator.Emit(OpCodes.Ldc_I4, arg2);
				}
				else
				{
					generator.Emit(OpCodes.Ldc_I4_0);
				}
			}
			else if (type == typeof(short))
			{
				if (value is short arg3)
				{
					generator.Emit(OpCodes.Ldc_I4, arg3);
				}
				else
				{
					generator.Emit(OpCodes.Ldc_I4_0);
				}
			}
			else if (type == typeof(uint))
			{
				if (value is uint arg4)
				{
					generator.Emit(OpCodes.Ldc_I4, (int)arg4);
				}
				else
				{
					generator.Emit(OpCodes.Ldc_I4_0);
				}
			}
			else if (type == typeof(ushort))
			{
				if (value is ushort arg5)
				{
					generator.Emit(OpCodes.Ldc_I4, arg5);
				}
				else
				{
					generator.Emit(OpCodes.Ldc_I4_0);
				}
			}
			else if (type == typeof(byte))
			{
				if (value is byte arg6)
				{
					generator.Emit(OpCodes.Ldc_I4_S, arg6);
				}
				else
				{
					generator.Emit(OpCodes.Ldc_I4_0);
				}
			}
			else if (type == typeof(sbyte))
			{
				if (value is sbyte arg7)
				{
					generator.Emit(OpCodes.Ldc_I4, arg7);
				}
				else
				{
					generator.Emit(OpCodes.Ldc_I4_0);
				}
			}
			else if (type == typeof(bool))
			{
				bool flag2 = default(bool);
				int num;
				if (value is bool)
				{
					flag2 = (bool)value;
					num = 1;
				}
				else
				{
					num = 0;
				}
				generator.Emit((((uint)num & (flag2 ? 1u : 0u)) != 0) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
			}
			else if (type == typeof(long))
			{
				generator.Emit(OpCodes.Ldc_I8, (value is long num2) ? num2 : 0);
			}
			else if (type == typeof(ulong))
			{
				generator.Emit(OpCodes.Ldc_I8, (long)((value is ulong num3) ? num3 : 0));
			}
			else if (type == typeof(float))
			{
				generator.Emit(OpCodes.Ldc_R4, (value is float num4) ? num4 : 0f);
			}
			else if (type == typeof(double))
			{
				generator.Emit(OpCodes.Ldc_R8, (value is double num5) ? num5 : 0.0);
			}
			else if (type == typeof(string))
			{
				if (value == null)
				{
					generator.Emit(OpCodes.Ldnull);
				}
				else
				{
					generator.Emit(OpCodes.Ldstr, (value is string text) ? text : "");
				}
			}
			else if (type.IsPointer)
			{
				generator.Emit(OpCodes.Ldc_I4_0);
			}
			else if (!type.IsValueType)
			{
				generator.Emit(OpCodes.Ldnull);
			}
			else
			{
				flag = false;
			}
		}
		return flag;
	}

	internal static void GenerateDefaultLoad(ILGenerator generator, Type type, object defaultValue)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (GenerateBasicLoad(generator, type, defaultValue))
		{
			return;
		}
		if (type.IsByRef)
		{
			Type elementType = type.GetElementType();
			int localIndex = generator.DeclareLocal(elementType).LocalIndex;
			if (GenerateBasicLoad(generator, elementType, defaultValue))
			{
				generator.Emit(OpCodes.Stloc_S, localIndex);
			}
			else
			{
				generator.Emit(OpCodes.Ldloca_S, localIndex);
				generator.Emit(OpCodes.Initobj, type);
			}
			generator.Emit(OpCodes.Ldloca_S, localIndex);
		}
		else
		{
			int localIndex2 = generator.DeclareLocal(type).LocalIndex;
			generator.Emit(OpCodes.Ldloca_S, localIndex2);
			generator.Emit(OpCodes.Initobj, type);
			generator.Emit(OpCodes.Ldloc_S, localIndex2);
		}
	}

	internal static Type[] GetParameterTypes(this MethodInfo method)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		ParameterInfo[] parameters = method.GetParameters();
		int num = parameters.Length;
		Type[] array = new Type[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = parameters[i].ParameterType;
		}
		return array;
	}

	internal static bool IsConditionalBranchInstruction(OpCode opcode)
	{
		return BRANCH_CODES.Contains(opcode);
	}

	internal static void LogAllExceptions()
	{
		AppDomain.CurrentDomain.UnhandledException += OnThrown;
	}

	internal static void LogAllFailedAsserts()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Harmony val = new Harmony("PeterHan.PLib.LogFailedAsserts");
		HarmonyMethod val2 = new HarmonyMethod(typeof(PTranspilerTools), "OnAssertFailed", (Type[])null);
		try
		{
			MethodBase methodSafe = typeof(Debug).GetMethodSafe("Assert", true, typeof(bool));
			if (methodSafe != null)
			{
				val.Patch(methodSafe, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
			methodSafe = typeof(Debug).GetMethodSafe("Assert", true, typeof(bool), typeof(object));
			if (methodSafe != null)
			{
				val.Patch(methodSafe, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
			methodSafe = typeof(Debug).GetMethodSafe("Assert", true, typeof(bool), typeof(object), typeof(Object));
			if (methodSafe != null)
			{
				val.Patch(methodSafe, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
		catch (Exception thrown)
		{
			PUtil.LogException(thrown);
		}
	}

	internal static void ModifyLoadI4(CodeInstruction instruction, int newValue)
	{
		if (newValue >= -1 && newValue <= 8)
		{
			instruction.opcode = LOAD_INT[newValue + 1];
			instruction.operand = null;
		}
		else if (newValue >= -128 && newValue <= 127)
		{
			instruction.opcode = OpCodes.Ldc_I4_S;
			instruction.operand = newValue;
		}
		else
		{
			instruction.opcode = OpCodes.Ldc_I4;
			instruction.operand = newValue;
		}
	}

	internal static void OnAssertFailed(bool condition)
	{
		if (!condition)
		{
			Debug.LogError((object)"Assert is about to fail:");
			Debug.LogError((object)new StackTrace().ToString());
		}
	}

	internal static void OnThrown(object sender, UnhandledExceptionEventArgs e)
	{
		if (!e.IsTerminating)
		{
			Debug.LogError((object)("Unhandled exception on Thread " + Thread.CurrentThread.Name));
			if (e.ExceptionObject is Exception ex)
			{
				Debug.LogException(ex);
			}
			else
			{
				Debug.LogError(e.ExceptionObject);
			}
		}
	}

	internal static Type[] PushDeclaringType(Type[] types, Type declaringType)
	{
		int num = types.Length;
		Type[] array = new Type[num + 1];
		if (declaringType.IsValueType)
		{
			declaringType = declaringType.MakeByRefType();
		}
		array[0] = declaringType;
		for (int i = 0; i < num; i++)
		{
			array[i + 1] = types[i];
		}
		return array;
	}
}
