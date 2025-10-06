using System;
using System.Collections.Generic;
using PeterHan.PLib.Detours;

namespace PeterHan.PLib.Lighting;

public sealed class OctantBuilder
{
	private delegate void ScanOctantFunc(Vector2I cellPos, int range, int depth, Octant octant, double startSlope, double endSlope, List<int> visiblePoints);

	private static readonly ScanOctantFunc OCTANT_SCAN;

	private readonly IDictionary<int, float> destination;

	public float Falloff { get; set; }

	public bool SmoothLight { get; set; }

	public int SourceCell { get; }

	static OctantBuilder()
	{
		OCTANT_SCAN = typeof(DiscreteShadowCaster).Detour<ScanOctantFunc>("ScanOctant");
		if (OCTANT_SCAN == null)
		{
			PLightManager.LogLightingWarning("OctantBuilder cannot find default octant scanner!");
		}
	}

	public OctantBuilder(IDictionary<int, float> destination, int sourceCell)
	{
		if (!Grid.IsValidCell(sourceCell))
		{
			throw new ArgumentOutOfRangeException("sourceCell");
		}
		this.destination = destination ?? throw new ArgumentNullException("destination");
		destination[sourceCell] = 1f;
		Falloff = 0.5f;
		SmoothLight = false;
		SourceCell = sourceCell;
	}

	public OctantBuilder AddOctant(int range, Octant octant)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		PooledList<int, OctantBuilder> val = ListPool<int, OctantBuilder>.Allocate();
		OCTANT_SCAN?.Invoke(Grid.CellToXY(SourceCell), range, 1, octant, 1.0, 0.0, (List<int>)(object)val);
		foreach (int item in (List<int>)(object)val)
		{
			float value = ((!SmoothLight) ? PLightManager.GetDefaultFalloff(Falloff, item, SourceCell) : PLightManager.GetSmoothFalloff(Falloff, item, SourceCell));
			destination[item] = value;
		}
		val.Recycle();
		return this;
	}

	public override string ToString()
	{
		return $"OctantBuilder[Cell {SourceCell:D}, {destination.Count:D} lit]";
	}
}
