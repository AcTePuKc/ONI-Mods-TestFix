using System;
using System.Collections.Generic;
using PeterHan.PLib.UI.Layouts;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public sealed class RelativeLayoutGroup : AbstractLayoutGroup, ISerializationCallbackReceiver
{
	private readonly IDictionary<GameObject, RelativeLayoutParams> locConstraints;

	[SerializeField]
	private IList<KeyValuePair<GameObject, RelativeLayoutParams>> serialConstraints;

	[SerializeField]
	private RectOffset margin;

	private readonly IList<RelativeLayoutResults> results;

	public RectOffset Margin
	{
		get
		{
			return margin;
		}
		set
		{
			margin = value;
		}
	}

	internal RelativeLayoutGroup()
	{
		base.layoutPriority = 1;
		locConstraints = new Dictionary<GameObject, RelativeLayoutParams>(32);
		results = new List<RelativeLayoutResults>(32);
		serialConstraints = null;
	}

	private RelativeLayoutParams AddOrGet(GameObject item)
	{
		if (!locConstraints.TryGetValue(item, out var value))
		{
			value = (locConstraints[item] = new RelativeLayoutParams());
		}
		return value;
	}

	public RelativeLayoutGroup AnchorXAxis(GameObject item, float anchor = 0.5f)
	{
		SetLeftEdge(item, anchor);
		return SetRightEdge(item, anchor);
	}

	public RelativeLayoutGroup AnchorYAxis(GameObject item, float anchor = 0.5f)
	{
		SetTopEdge(item, anchor);
		return SetBottomEdge(item, anchor);
	}

	public override void CalculateLayoutInputHorizontal()
	{
		GameObject gameObject = ((Component)this).gameObject;
		RectTransform val = ((gameObject != null) ? Util.rectTransform(gameObject) : null);
		if (!((Object)(object)val != (Object)null) || locked)
		{
			return;
		}
		int num;
		int num2;
		if (Margin == null)
		{
			num = (num2 = 0);
		}
		else
		{
			num = Margin.left;
			num2 = Margin.right;
		}
		results.CalcX(val, locConstraints);
		if (results.Count > 0)
		{
			int num3 = 2 * results.Count;
			int i;
			for (i = 0; i < num3; i++)
			{
				if (results.RunPassX())
				{
					break;
				}
			}
			if (i >= num3)
			{
				results.ThrowUnresolvable(i, PanelDirection.Horizontal);
			}
		}
		float num4 = (base.preferredWidth = results.GetMinSizeX() + (float)num + (float)num2);
		base.minWidth = num4;
	}

	public override void CalculateLayoutInputVertical()
	{
		GameObject gameObject = ((Component)this).gameObject;
		if (!((Object)(object)((gameObject != null) ? Util.rectTransform(gameObject) : null) != (Object)null) || locked)
		{
			return;
		}
		int num = 2 * results.Count;
		int num2;
		int num3;
		if (Margin == null)
		{
			num2 = (num3 = 0);
		}
		else
		{
			num2 = Margin.top;
			num3 = Margin.bottom;
		}
		if (results.Count > 0)
		{
			results.CalcY();
			int i;
			for (i = 0; i < num; i++)
			{
				if (results.RunPassY())
				{
					break;
				}
			}
			if (i >= num)
			{
				results.ThrowUnresolvable(i, PanelDirection.Vertical);
			}
		}
		float num4 = (base.preferredHeight = results.GetMinSizeY() + (float)num2 + (float)num3);
		base.minHeight = num4;
	}

	internal void Import(IDictionary<GameObject, RelativeLayoutParams> values)
	{
		locConstraints.Clear();
		foreach (KeyValuePair<GameObject, RelativeLayoutParams> value in values)
		{
			locConstraints[value.Key] = value.Value;
		}
	}

	public void OnBeforeSerialize()
	{
		int count = locConstraints.Count;
		if (count <= 0)
		{
			return;
		}
		serialConstraints = new List<KeyValuePair<GameObject, RelativeLayoutParams>>(count);
		foreach (KeyValuePair<GameObject, RelativeLayoutParams> locConstraint in locConstraints)
		{
			serialConstraints.Add(locConstraint);
		}
	}

	public void OnAfterDeserialize()
	{
		if (serialConstraints == null)
		{
			return;
		}
		locConstraints.Clear();
		foreach (KeyValuePair<GameObject, RelativeLayoutParams> serialConstraint in serialConstraints)
		{
			locConstraints[serialConstraint.Key] = serialConstraint.Value;
		}
		serialConstraints = null;
	}

	public RelativeLayoutGroup OverrideSize(GameObject item, Vector2 size)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)item != (Object)null)
		{
			AddOrGet(item).OverrideSize = size;
		}
		return this;
	}

	public RelativeLayoutGroup SetBottomEdge(GameObject item, float fraction = -1f, GameObject above = null)
	{
		if ((Object)(object)item != (Object)null)
		{
			if ((Object)(object)above == (Object)(object)item)
			{
				throw new ArgumentException("Component cannot refer directly to itself");
			}
			SetEdge(AddOrGet(item).BottomEdge, fraction, above);
		}
		return this;
	}

	protected override void SetDirty()
	{
		if (!locked)
		{
			base.SetDirty();
		}
	}

	private void SetEdge(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, float fraction, GameObject child)
	{
		if (fraction >= 0f && fraction <= 1f)
		{
			edge.Constraint = RelativeConstraintType.ToAnchor;
			edge.FromAnchor = fraction;
			edge.FromComponent = null;
		}
		else if ((Object)(object)child != (Object)null)
		{
			edge.Constraint = RelativeConstraintType.ToComponent;
			edge.FromComponent = child;
		}
		else
		{
			edge.Constraint = RelativeConstraintType.Unconstrained;
			edge.FromComponent = null;
		}
	}

	public override void SetLayoutHorizontal()
	{
		if (!locked && results.Count > 0)
		{
			PooledList<ILayoutController, RelativeLayoutGroup> val = ListPool<ILayoutController, RelativeLayoutGroup>.Allocate();
			int num;
			int num2;
			if (Margin == null)
			{
				num = (num2 = 0);
			}
			else
			{
				num = Margin.left;
				num2 = Margin.right;
			}
			results.ExecuteX((List<ILayoutController>)(object)val, num, num2);
			val.Recycle();
		}
	}

	public override void SetLayoutVertical()
	{
		if (!locked && results.Count > 0)
		{
			PooledList<ILayoutController, RelativeLayoutGroup> val = ListPool<ILayoutController, RelativeLayoutGroup>.Allocate();
			int num;
			int num2;
			if (Margin == null)
			{
				num = (num2 = 0);
			}
			else
			{
				num = Margin.top;
				num2 = Margin.bottom;
			}
			results.ExecuteY((List<ILayoutController>)(object)val, num, num2);
			val.Recycle();
		}
	}

	public RelativeLayoutGroup SetLeftEdge(GameObject item, float fraction = -1f, GameObject toRight = null)
	{
		if ((Object)(object)item != (Object)null)
		{
			if ((Object)(object)toRight == (Object)(object)item)
			{
				throw new ArgumentException("Component cannot refer directly to itself");
			}
			SetEdge(AddOrGet(item).LeftEdge, fraction, toRight);
		}
		return this;
	}

	public RelativeLayoutGroup SetMargin(GameObject item, RectOffset insets)
	{
		if ((Object)(object)item != (Object)null)
		{
			AddOrGet(item).Insets = insets;
		}
		return this;
	}

	internal void SetRaw(GameObject item, RelativeLayoutParams rawParams)
	{
		if ((Object)(object)item != (Object)null && rawParams != null)
		{
			locConstraints[item] = rawParams;
		}
	}

	public RelativeLayoutGroup SetRightEdge(GameObject item, float fraction = -1f, GameObject toLeft = null)
	{
		if ((Object)(object)item != (Object)null)
		{
			if ((Object)(object)toLeft == (Object)(object)item)
			{
				throw new ArgumentException("Component cannot refer directly to itself");
			}
			SetEdge(AddOrGet(item).RightEdge, fraction, toLeft);
		}
		return this;
	}

	public RelativeLayoutGroup SetTopEdge(GameObject item, float fraction = -1f, GameObject below = null)
	{
		if ((Object)(object)item != (Object)null)
		{
			if ((Object)(object)below == (Object)(object)item)
			{
				throw new ArgumentException("Component cannot refer directly to itself");
			}
			SetEdge(AddOrGet(item).TopEdge, fraction, below);
		}
		return this;
	}
}
