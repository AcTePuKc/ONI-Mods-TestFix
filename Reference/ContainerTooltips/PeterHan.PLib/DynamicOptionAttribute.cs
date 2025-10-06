using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DynamicOptionAttribute : Attribute
{
	public string Category { get; }

	public Type Handler { get; }

	public DynamicOptionAttribute(Type type, string category = null)
	{
		Category = category;
		Handler = type ?? throw new ArgumentNullException("type");
	}

	public override string ToString()
	{
		return "DynamicOption[handler={0},category={1}]".F(Handler.FullName, Category);
	}
}
