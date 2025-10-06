using System;

namespace PeterHan.PLib.Detours;

internal sealed class DetouredField<P, T> : IDetouredField<P, T>
{
	public Func<P, T> Get { get; }

	public string Name { get; }

	public Action<P, T> Set { get; }

	internal DetouredField(string name, Func<P, T> get, Action<P, T> set)
	{
		Name = name ?? throw new ArgumentNullException("name");
		Get = get;
		Set = set;
	}

	public override string ToString()
	{
		return $"DetouredField[name={Name}]";
	}
}
