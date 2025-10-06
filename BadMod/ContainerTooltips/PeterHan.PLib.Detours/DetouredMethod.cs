using System;

namespace PeterHan.PLib.Detours;

public sealed class DetouredMethod<D> where D : Delegate
{
	private D delg;

	private readonly Type type;

	public D Invoke
	{
		get
		{
			Initialize();
			return delg;
		}
	}

	public string Name { get; }

	internal DetouredMethod(Type type, string name)
	{
		this.type = type ?? throw new ArgumentNullException("type");
		Name = name ?? throw new ArgumentNullException("name");
		delg = null;
	}

	public void Initialize()
	{
		if (delg == null)
		{
			delg = type.Detour<D>(Name);
		}
	}

	public override string ToString()
	{
		return string.Format("LazyDetouredMethod[type={1},name={0}]", Name, type.FullName);
	}
}
