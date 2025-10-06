using System;

namespace PeterHan.PLib.Detours;

public interface IDetouredField<P, T>
{
	Func<P, T> Get { get; }

	string Name { get; }

	Action<P, T> Set { get; }
}
