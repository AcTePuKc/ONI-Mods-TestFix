using System;
using System.Reflection;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.PatchManager;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PLibMethodAttribute : Attribute, IPLibAnnotation
{
	public string RequireAssembly { get; set; }

	public string RequireType { get; set; }

	public uint Runtime { get; }

	public PLibMethodAttribute(uint runtime)
	{
		Runtime = runtime;
	}

	public IPatchMethodInstance CreateInstance(MethodInfo method)
	{
		return new PLibMethodInstance(this, method);
	}

	public override string ToString()
	{
		return "PLibMethod[RunAt={0}]".F(RunAt.ToString(Runtime));
	}
}
