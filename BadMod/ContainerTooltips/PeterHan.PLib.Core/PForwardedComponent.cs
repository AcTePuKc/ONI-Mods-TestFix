using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;

namespace PeterHan.PLib.Core;

public abstract class PForwardedComponent : IComparable<PForwardedComponent>
{
	public sealed class PComponentComparator : IComparer<PForwardedComponent>
	{
		public int Compare(PForwardedComponent a, PForwardedComponent b)
		{
			if (b != null)
			{
				if (a != null)
				{
					return b.Version.CompareTo(a.Version);
				}
				return 1;
			}
			if (a != null)
			{
				return -1;
			}
			return 0;
		}
	}

	public const int MAX_DEPTH = 8;

	private volatile bool registered;

	private readonly object candidateLock;

	protected virtual object InstanceData { get; set; }

	public string ID => GetType().FullName;

	protected JsonSerializer SerializationSettings { get; set; }

	public abstract Version Version { get; }

	protected PForwardedComponent()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		candidateLock = new object();
		InstanceData = null;
		registered = false;
		SerializationSettings = new JsonSerializer
		{
			DateTimeZoneHandling = (DateTimeZoneHandling)3,
			Culture = CultureInfo.InvariantCulture,
			MaxDepth = 8
		};
	}

	public virtual void Bootstrap(Harmony plibInstance)
	{
	}

	public int CompareTo(PForwardedComponent other)
	{
		return Version.CompareTo(other.Version);
	}

	internal virtual object DoInitialize(Harmony plibInstance)
	{
		Initialize(plibInstance);
		return this;
	}

	public T GetInstanceData<T>(T defValue = default(T))
	{
		if (!(InstanceData is T result))
		{
			return defValue;
		}
		return result;
	}

	public T GetInstanceDataSerialized<T>(T defValue = default(T))
	{
		//IL_0097: Expected O, but got Unknown
		object instanceData = InstanceData;
		T result = defValue;
		using (MemoryStream memoryStream = new MemoryStream(1024))
		{
			try
			{
				StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
				SerializationSettings.Serialize((TextWriter)streamWriter, instanceData);
				streamWriter.Flush();
				memoryStream.Position = 0L;
				StreamReader streamReader = new StreamReader(memoryStream, Encoding.UTF8);
				if (SerializationSettings.Deserialize((TextReader)streamReader, typeof(T)) is T val)
				{
					result = val;
				}
			}
			catch (JsonException ex)
			{
				PUtil.LogError("Unable to serialize instance data for component " + ID + ":");
				PUtil.LogException((Exception)ex);
				result = defValue;
			}
		}
		return result;
	}

	public T GetSharedData<T>(T defValue = default(T))
	{
		if (!(PRegistry.Instance.GetSharedData(ID) is T result))
		{
			return defValue;
		}
		return result;
	}

	public T GetSharedDataSerialized<T>(T defValue = default(T))
	{
		//IL_0093: Expected O, but got Unknown
		object sharedData = PRegistry.Instance.GetSharedData(ID);
		T result = defValue;
		using (MemoryStream memoryStream = new MemoryStream(1024))
		{
			try
			{
				SerializationSettings.Serialize((TextWriter)new StreamWriter(memoryStream, Encoding.UTF8), sharedData);
				memoryStream.Position = 0L;
				if (SerializationSettings.Deserialize((TextReader)new StreamReader(memoryStream, Encoding.UTF8), typeof(T)) is T val)
				{
					result = val;
				}
			}
			catch (JsonException ex)
			{
				PUtil.LogError("Unable to serialize shared data for component " + ID + ":");
				PUtil.LogException((Exception)ex);
				result = defValue;
			}
		}
		return result;
	}

	public virtual Assembly GetOwningAssembly()
	{
		return GetType().Assembly;
	}

	public abstract void Initialize(Harmony plibInstance);

	protected void InvokeAllProcess(uint operation, object args)
	{
		IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(ID);
		if (allComponents == null)
		{
			return;
		}
		foreach (PForwardedComponent item in allComponents)
		{
			item.Process(operation, args);
		}
	}

	public HarmonyMethod PatchMethod(string name)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		return new HarmonyMethod(GetType(), name, (Type[])null);
	}

	public virtual void PostInitialize(Harmony plibInstance)
	{
	}

	public virtual void Process(uint operation, object args)
	{
	}

	protected bool RegisterForForwarding()
	{
		bool result = false;
		lock (candidateLock)
		{
			if (!registered)
			{
				PUtil.InitLibrary(logVersion: false);
				PRegistry.Instance.AddCandidateVersion(this);
				result = (registered = true);
			}
		}
		return result;
	}

	public void SetSharedData(object value)
	{
		PRegistry.Instance.SetSharedData(ID, value);
	}
}
