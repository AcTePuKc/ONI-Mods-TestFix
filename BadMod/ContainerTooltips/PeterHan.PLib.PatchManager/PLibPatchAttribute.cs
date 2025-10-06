using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.PatchManager;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PLibPatchAttribute : Attribute, IPLibAnnotation
{
	public Type[] ArgumentTypes { get; set; }

	public bool IgnoreOnFail { get; set; }

	public string MethodName { get; }

	public HarmonyPatchType PatchType { get; set; }

	public string RequireAssembly { get; set; }

	public string RequireType { get; set; }

	public uint Runtime { get; }

	public Type TargetType { get; }

	public PLibPatchAttribute(uint runtime, Type target, string method)
	{
		ArgumentTypes = null;
		IgnoreOnFail = false;
		MethodName = method;
		PatchType = (HarmonyPatchType)0;
		Runtime = runtime;
		TargetType = target ?? throw new ArgumentNullException("target");
	}

	public PLibPatchAttribute(uint runtime, Type target, string method, params Type[] argTypes)
	{
		ArgumentTypes = argTypes;
		IgnoreOnFail = false;
		MethodName = method;
		PatchType = (HarmonyPatchType)0;
		Runtime = runtime;
		TargetType = target ?? throw new ArgumentNullException("target");
	}

	public PLibPatchAttribute(uint runtime, string method)
	{
		ArgumentTypes = null;
		IgnoreOnFail = false;
		MethodName = method;
		PatchType = (HarmonyPatchType)0;
		Runtime = runtime;
		TargetType = null;
	}

	public PLibPatchAttribute(uint runtime, string method, params Type[] argTypes)
	{
		ArgumentTypes = argTypes;
		IgnoreOnFail = false;
		MethodName = method;
		PatchType = (HarmonyPatchType)0;
		Runtime = runtime;
		TargetType = null;
	}

	public IPatchMethodInstance CreateInstance(MethodInfo method)
	{
		return new PLibPatchInstance(this, method);
	}

	public override string ToString()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return "PLibPatch[RunAt={0},PatchType={1},MethodName={2}]".F(RunAt.ToString(Runtime), PatchType, MethodName);
	}
}
