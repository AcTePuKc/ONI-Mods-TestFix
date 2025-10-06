using System;

namespace PeterHan.PLib.Detours;

public class DetourException : ArgumentException
{
	public DetourException(string message)
		: base(message)
	{
	}
}
