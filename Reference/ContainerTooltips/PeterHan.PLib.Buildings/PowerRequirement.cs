using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Buildings;

public class PowerRequirement
{
	public float MaxWattage { get; }

	public CellOffset PlugLocation { get; }

	public PowerRequirement(float wattage, CellOffset plugLocation)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (wattage.IsNaNOrInfinity() || wattage < 0f)
		{
			throw new ArgumentException("wattage");
		}
		MaxWattage = wattage;
		PlugLocation = plugLocation;
	}

	public override string ToString()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return "Power[Watts={0:F0},Location={1}]".F(MaxWattage, PlugLocation);
	}
}
