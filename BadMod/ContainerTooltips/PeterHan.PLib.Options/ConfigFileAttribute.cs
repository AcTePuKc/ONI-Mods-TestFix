using System;

namespace PeterHan.PLib.Options;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ConfigFileAttribute : Attribute
{
	public string ConfigFileName { get; }

	public bool IndentOutput { get; }

	public bool UseSharedConfigLocation { get; }

	public ConfigFileAttribute(string FileName = "config.json", bool IndentOutput = false, bool SharedConfigLocation = false)
	{
		ConfigFileName = FileName;
		this.IndentOutput = IndentOutput;
		UseSharedConfigLocation = SharedConfigLocation;
	}

	public override string ToString()
	{
		return ConfigFileName;
	}
}
