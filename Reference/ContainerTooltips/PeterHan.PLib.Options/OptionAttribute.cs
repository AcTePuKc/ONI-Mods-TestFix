using System;

namespace PeterHan.PLib.Options;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class OptionAttribute : Attribute, IOptionSpec
{
	public string Category { get; }

	public string Format { get; set; }

	public string Title { get; }

	public string Tooltip { get; }

	public OptionAttribute()
	{
		Format = null;
		Title = null;
		Tooltip = null;
		Category = null;
	}

	public OptionAttribute(string title, string tooltip = null, string category = null)
	{
		if (string.IsNullOrEmpty(title))
		{
			throw new ArgumentNullException("title");
		}
		Category = category;
		Format = null;
		Title = title;
		Tooltip = tooltip;
	}

	public override string ToString()
	{
		return Title;
	}
}
