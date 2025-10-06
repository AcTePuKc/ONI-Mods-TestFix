using System;
using System.Collections.Generic;
using PeterHan.PLib.Core;
using UnityEngine;

namespace PeterHan.PLib.Lighting;

internal sealed class PRemoteLightWrapper : ILightShape
{
	private delegate void FillLightDelegate(GameObject source, int cell, int range, IDictionary<int, float> brightness);

	private readonly FillLightDelegate fillLight;

	public string Identifier { get; }

	public LightShape KleiLightShape { get; }

	public LightShape RayMode { get; }

	internal static ILightShape LightToInstance(object other)
	{
		if (other != null && !(other.GetType().Name != "PLightShape"))
		{
			if (!(other is ILightShape result))
			{
				return new PRemoteLightWrapper(other);
			}
			return result;
		}
		return null;
	}

	internal PRemoteLightWrapper(object other)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (!PPatchTools.TryGetPropertyValue<LightShape>(other, "KleiLightShape", out var value))
		{
			throw new ArgumentException("Light shape is missing KleiLightShape");
		}
		KleiLightShape = value;
		if (!PPatchTools.TryGetPropertyValue<string>(other, "Identifier", out var value2) || value2 == null)
		{
			throw new ArgumentException("Light shape is missing Identifier");
		}
		Identifier = value2;
		if (!PPatchTools.TryGetPropertyValue<LightShape>(other, "RayMode", out var value3))
		{
			value3 = (LightShape)(-1);
		}
		RayMode = value3;
		Type type = other.GetType();
		fillLight = type.CreateDelegate<FillLightDelegate>("DoFillLight", other, new Type[4]
		{
			typeof(GameObject),
			typeof(int),
			typeof(int),
			typeof(IDictionary<int, float>)
		});
		if (fillLight == null)
		{
			throw new ArgumentException("Light shape is missing FillLight");
		}
	}

	public void FillLight(LightingArgs args)
	{
		fillLight(args.Source, args.SourceCell, args.Range, args.Brightness);
	}
}
