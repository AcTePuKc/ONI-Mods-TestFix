using System;
using System.Collections.Generic;
using PeterHan.PLib.Core;
using UnityEngine;

namespace PeterHan.PLib.Buildings;

public abstract class ColoredRangeVisualizer : KMonoBehaviour
{
	protected sealed class VisCellData : IComparable<VisCellData>
	{
		public int Cell { get; }

		public KBatchedAnimController Controller { get; private set; }

		public Color Tint { get; }

		public VisCellData(int cell)
			: this(cell, Color.white)
		{
		}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


		public VisCellData(int cell, Color tint)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Cell = cell;
			Controller = null;
			Tint = tint;
		}

		public int CompareTo(VisCellData other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			return Cell.CompareTo(other.Cell);
		}

		public void CreateController(SceneLayer sceneLayer)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			Controller = FXHelpers.CreateEffect("transferarmgrid_kanim", Grid.CellToPosCCC(Cell, sceneLayer), (Transform)null, false, sceneLayer, true);
			((KAnimControllerBase)Controller).destroyOnAnimComplete = false;
			((KAnimControllerBase)Controller).visibilityType = (VisibilityType)2;
			((Component)Controller).gameObject.SetActive(true);
			((KAnimControllerBase)Controller).Play(PRE_ANIMS, (PlayMode)0);
			((KAnimControllerBase)Controller).TintColour = Color32.op_Implicit(Tint);
		}

		public void Destroy()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Controller != (Object)null)
			{
				((KAnimControllerBase)Controller).destroyOnAnimComplete = true;
				((KAnimControllerBase)Controller).Play(POST_ANIM, (PlayMode)1, 1f, 0f);
				Controller = null;
			}
		}

		public override bool Equals(object obj)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (obj is VisCellData visCellData && visCellData.Cell == Cell)
			{
				Color tint = Tint;
				return ((Color)(ref tint)).Equals(visCellData.Tint);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Cell;
		}

		public override string ToString()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			return "CellData[cell={0:D},color={1}]".F(Cell, Tint);
		}
	}

	private const string ANIM_NAME = "transferarmgrid_kanim";

	private static readonly HashedString[] PRE_ANIMS = (HashedString[])(object)new HashedString[2]
	{
		HashedString.op_Implicit("grid_pre"),
		HashedString.op_Implicit("grid_loop")
	};

	private static readonly HashedString POST_ANIM = HashedString.op_Implicit("grid_pst");

	[MyCmpGet]
	protected BuildingPreview preview;

	[MyCmpGet]
	protected Rotatable rotatable;

	private readonly HashSet<VisCellData> cells;

	public SceneLayer Layer { get; set; }

	protected ColoredRangeVisualizer()
	{
		cells = new HashSet<VisCellData>();
		Layer = (SceneLayer)32;
	}

	private void CreateVisualizers()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		PooledHashSet<VisCellData, ColoredRangeVisualizer> val = HashSetPool<VisCellData, ColoredRangeVisualizer>.Allocate();
		PooledList<VisCellData, ColoredRangeVisualizer> val2 = ListPool<VisCellData, ColoredRangeVisualizer>.Allocate();
		try
		{
			if ((Object)(object)((Component)this).gameObject != (Object)null)
			{
				VisualizeCells((ICollection<VisCellData>)val);
			}
			foreach (VisCellData cell in cells)
			{
				if (((HashSet<VisCellData>)(object)val).Remove(cell))
				{
					((List<VisCellData>)(object)val2).Add(cell);
				}
				else
				{
					cell.Destroy();
				}
			}
			foreach (VisCellData item in (HashSet<VisCellData>)(object)val)
			{
				item.CreateController(Layer);
				((List<VisCellData>)(object)val2).Add(item);
			}
			cells.Clear();
			foreach (VisCellData item2 in (List<VisCellData>)(object)val2)
			{
				cells.Add(item2);
			}
		}
		finally
		{
			val.Recycle();
			val2.Recycle();
		}
	}

	private void OnCellChange()
	{
		CreateVisualizers();
	}

	protected override void OnCleanUp()
	{
		((KMonoBehaviour)this).Unsubscribe(-1503271301);
		if ((Object)(object)preview != (Object)null)
		{
			Singleton<CellChangeMonitor>.Instance.UnregisterCellChangedHandler(((KMonoBehaviour)this).transform, (Action)OnCellChange);
			if ((Object)(object)rotatable != (Object)null)
			{
				((KMonoBehaviour)this).Unsubscribe(-1643076535);
			}
		}
		RemoveVisualizers();
		((KMonoBehaviour)this).OnCleanUp();
	}

	private void OnRotated(object _)
	{
		CreateVisualizers();
	}

	private void OnSelect(object data)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (data is bool flag)
		{
			Vector3 position = ((KMonoBehaviour)this).transform.position;
			if (flag)
			{
				PGameUtils.PlaySound("RadialGrid_form", position);
				CreateVisualizers();
			}
			else
			{
				PGameUtils.PlaySound("RadialGrid_disappear", position);
				RemoveVisualizers();
			}
		}
	}

	protected override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		((KMonoBehaviour)this).Subscribe(-1503271301, (Action<object>)OnSelect);
		if ((Object)(object)preview != (Object)null)
		{
			Singleton<CellChangeMonitor>.Instance.RegisterCellChangedHandler(((KMonoBehaviour)this).transform, (Action)OnCellChange, "ColoredRangeVisualizer.OnSpawn");
			if ((Object)(object)rotatable != (Object)null)
			{
				((KMonoBehaviour)this).Subscribe(-1643076535, (Action<object>)OnRotated);
			}
		}
	}

	private void RemoveVisualizers()
	{
		foreach (VisCellData cell in cells)
		{
			cell.Destroy();
		}
		cells.Clear();
	}

	protected int RotateOffsetCell(int baseCell, CellOffset offset)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rotatable != (Object)null)
		{
			offset = rotatable.GetRotatedCellOffset(offset);
		}
		return Grid.OffsetCell(baseCell, offset);
	}

	protected abstract void VisualizeCells(ICollection<VisCellData> newCells);
}
