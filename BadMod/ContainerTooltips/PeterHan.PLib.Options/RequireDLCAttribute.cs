using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Options;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class RequireDLCAttribute : Attribute
{
	public string DlcID { get; }

	public bool Required { get; }

	public RequireDLCAttribute(string dlcID)
	{
		DlcID = dlcID;
		Required = true;
	}

	public RequireDLCAttribute(string dlcID, bool required)
	{
		DlcID = dlcID ?? "";
		Required = required;
	}

	public override string ToString()
	{
		return "RequireDLC[DLC={0},require={1}]".F(DlcID, Required);
	}
}
