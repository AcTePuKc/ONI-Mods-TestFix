using System;

namespace PeterHan.PLib.Detours;

internal sealed class LazyDetouredField<P, T> : IDetouredField<P, T>
{
	private Func<P, T> getter;

	private Action<P, T> setter;

	private readonly Type type;

	public Func<P, T> Get
	{
		get
		{
			Initialize();
			return getter;
		}
	}

	public string Name { get; }

	public Action<P, T> Set
	{
		get
		{
			Initialize();
			return setter;
		}
	}

	internal LazyDetouredField(Type type, string name)
	{
		this.type = type ?? throw new ArgumentNullException("type");
		Name = name ?? throw new ArgumentNullException("name");
		getter = null;
		setter = null;
	}

	public void Initialize()
	{
		if (getter == null && setter == null)
		{
			IDetouredField<P, T> detouredField = PDetours.DetourField<P, T>(Name);
			getter = detouredField.Get;
			setter = detouredField.Set;
		}
	}

	public override string ToString()
	{
		return string.Format("LazyDetouredField[type={1},name={0}]", Name, type.FullName);
	}
}
