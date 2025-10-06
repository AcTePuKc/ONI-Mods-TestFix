using System;
using System.Reflection;
using HarmonyLib;

namespace PeterHan.PLib.Core;

internal sealed class PRemoteComponent : PForwardedComponent
{
	private delegate void InitializeDelegate(Harmony instance);

	private delegate void ProcessDelegate(uint operation, object args);

	private readonly InitializeDelegate doBootstrap;

	private readonly InitializeDelegate doInitialize;

	private readonly InitializeDelegate doPostInitialize;

	private readonly Func<object> getData;

	private readonly ProcessDelegate process;

	private readonly Action<object> setData;

	private readonly Version version;

	private readonly object wrapped;

	protected override object InstanceData
	{
		get
		{
			return getData?.Invoke();
		}
		set
		{
			setData?.Invoke(value);
		}
	}

	public override Version Version => version;

	internal PRemoteComponent(object wrapped)
	{
		this.wrapped = wrapped ?? throw new ArgumentNullException("wrapped");
		if (!PPatchTools.TryGetPropertyValue<Version>(wrapped, "Version", out var value))
		{
			throw new ArgumentException("Remote component missing Version property");
		}
		version = value;
		Type type = wrapped.GetType();
		doInitialize = type.CreateDelegate<InitializeDelegate>("Initialize", wrapped, new Type[1] { typeof(Harmony) });
		if (doInitialize == null)
		{
			throw new ArgumentException("Remote component missing Initialize");
		}
		doBootstrap = type.CreateDelegate<InitializeDelegate>("Bootstrap", wrapped, new Type[1] { typeof(Harmony) });
		doPostInitialize = type.CreateDelegate<InitializeDelegate>("PostInitialize", wrapped, new Type[1] { typeof(Harmony) });
		getData = type.CreateGetDelegate<object>("InstanceData", wrapped);
		setData = type.CreateSetDelegate<object>("InstanceData", wrapped);
		process = type.CreateDelegate<ProcessDelegate>("Process", wrapped, new Type[2]
		{
			typeof(uint),
			typeof(object)
		});
	}

	public override void Bootstrap(Harmony plibInstance)
	{
		doBootstrap?.Invoke(plibInstance);
	}

	internal override object DoInitialize(Harmony plibInstance)
	{
		doInitialize(plibInstance);
		return wrapped;
	}

	public override Assembly GetOwningAssembly()
	{
		return wrapped.GetType().Assembly;
	}

	public override void Initialize(Harmony plibInstance)
	{
		DoInitialize(plibInstance);
	}

	public override void PostInitialize(Harmony plibInstance)
	{
		doPostInitialize?.Invoke(plibInstance);
	}

	public override void Process(uint operation, object args)
	{
		process?.Invoke(operation, args);
	}

	public override string ToString()
	{
		return "PRemoteComponent[ID={0},TargetType={1}]".F(base.ID, wrapped.GetType().AssemblyQualifiedName);
	}
}
