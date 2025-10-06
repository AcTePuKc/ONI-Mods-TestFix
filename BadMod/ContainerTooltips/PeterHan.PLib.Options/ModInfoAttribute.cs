using System;

namespace PeterHan.PLib.Options;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ModInfoAttribute : Attribute
{
	public bool ForceCollapseCategories { get; }

	public string Image { get; }

	public string URL { get; }

	public ModInfoAttribute(string url, string image = null, bool collapse = false)
	{
		ForceCollapseCategories = collapse;
		Image = image;
		URL = url;
	}

	public override string ToString()
	{
		return string.Format("ModInfoAttribute[URL={1},Image={2}]", URL, Image);
	}
}
