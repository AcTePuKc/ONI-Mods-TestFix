using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.PLib.Lighting;

internal sealed class PLightShape : ILightShape
{
	private readonly PLightManager.CastLightDelegate handler;

	public string Identifier { get; }

	public LightShape KleiLightShape => (LightShape)(ShapeID + 1);

	public LightShape RayMode { get; }

	internal int ShapeID { get; }

	internal PLightShape(int id, string identifier, PLightManager.CastLightDelegate handler, LightShape rayMode)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		this.handler = handler ?? throw new ArgumentNullException("handler");
		Identifier = identifier ?? throw new ArgumentNullException("identifier");
		RayMode = rayMode;
		ShapeID = id;
	}

	public override bool Equals(object obj)
	{
		if (obj is PLightShape pLightShape)
		{
			return pLightShape.Identifier == Identifier;
		}
		return false;
	}

	internal void DoFillLight(GameObject source, int cell, int range, IDictionary<int, float> brightness)
	{
		handler(new LightingArgs(source, cell, range, brightness));
	}

	public void FillLight(LightingArgs args)
	{
		handler(args);
	}

	public override int GetHashCode()
	{
		return Identifier.GetHashCode();
	}

	public override string ToString()
	{
		return "PLightShape[ID=" + Identifier + "]";
	}
}
