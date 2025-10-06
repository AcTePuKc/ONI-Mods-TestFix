using System;

namespace PeterHan.PLib.Options;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RestartRequiredAttribute : Attribute
{
}
