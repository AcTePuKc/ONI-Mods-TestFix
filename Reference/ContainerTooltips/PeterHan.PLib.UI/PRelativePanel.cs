using System;
using System.Collections.Generic;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI.Layouts;
using UnityEngine;

namespace PeterHan.PLib.UI;

public class PRelativePanel : PContainer, IDynamicSizable, IUIComponent
{
	private readonly IDictionary<IUIComponent, RelativeLayoutParamsBase<IUIComponent>> constraints;

	public bool DynamicSize { get; set; }

	public PRelativePanel()
		: this(null)
	{
		DynamicSize = true;
	}

	public PRelativePanel(string name)
		: base(name ?? "RelativePanel")
	{
		constraints = new Dictionary<IUIComponent, RelativeLayoutParamsBase<IUIComponent>>(16);
		base.Margin = null;
	}

	public PRelativePanel AddChild(IUIComponent child)
	{
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		if (!constraints.ContainsKey(child))
		{
			constraints.Add(child, new RelativeLayoutParamsBase<IUIComponent>());
		}
		return this;
	}

	public PRelativePanel AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		base.OnRealize += onRealize;
		return this;
	}

	public PRelativePanel AnchorXAxis(IUIComponent item, float anchor = 0.5f)
	{
		SetLeftEdge(item, anchor);
		return SetRightEdge(item, anchor);
	}

	public PRelativePanel AnchorYAxis(IUIComponent item, float anchor = 0.5f)
	{
		SetTopEdge(item, anchor);
		return SetBottomEdge(item, anchor);
	}

	public override GameObject Build()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(null, base.Name);
		PooledDictionary<IUIComponent, GameObject, PRelativePanel> val2 = DictionaryPool<IUIComponent, GameObject, PRelativePanel>.Allocate();
		SetImage(val);
		foreach (KeyValuePair<IUIComponent, RelativeLayoutParamsBase<IUIComponent>> constraint in constraints)
		{
			IUIComponent key = constraint.Key;
			GameObject val3 = key.Build();
			val3.SetParent(val);
			((Dictionary<IUIComponent, GameObject>)(object)val2)[key] = val3;
		}
		RelativeLayoutGroup relativeLayoutGroup = val.AddComponent<RelativeLayoutGroup>();
		relativeLayoutGroup.Margin = base.Margin;
		foreach (KeyValuePair<IUIComponent, RelativeLayoutParamsBase<IUIComponent>> constraint2 in constraints)
		{
			GameObject item = ((Dictionary<IUIComponent, GameObject>)(object)val2)[constraint2.Key];
			RelativeLayoutParamsBase<IUIComponent> value = constraint2.Value;
			RelativeLayoutParams relativeLayoutParams = new RelativeLayoutParams();
			Resolve(relativeLayoutParams.TopEdge, value.TopEdge, (IDictionary<IUIComponent, GameObject>)val2);
			Resolve(relativeLayoutParams.BottomEdge, value.BottomEdge, (IDictionary<IUIComponent, GameObject>)val2);
			Resolve(relativeLayoutParams.LeftEdge, value.LeftEdge, (IDictionary<IUIComponent, GameObject>)val2);
			Resolve(relativeLayoutParams.RightEdge, value.RightEdge, (IDictionary<IUIComponent, GameObject>)val2);
			relativeLayoutParams.OverrideSize = value.OverrideSize;
			relativeLayoutParams.Insets = value.Insets;
			relativeLayoutGroup.SetRaw(item, relativeLayoutParams);
		}
		if (!DynamicSize)
		{
			relativeLayoutGroup.LockLayout();
		}
		val2.Recycle();
		relativeLayoutGroup.flexibleWidth = base.FlexSize.x;
		relativeLayoutGroup.flexibleHeight = base.FlexSize.y;
		InvokeRealize(val);
		return val;
	}

	private RelativeLayoutParamsBase<IUIComponent> GetOrThrow(IUIComponent item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (!constraints.TryGetValue(item, out var value))
		{
			throw new ArgumentException("Components must be added to the panel before using them in a constraint");
		}
		return value;
	}

	public PRelativePanel OverrideSize(IUIComponent item, Vector2 size)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (item != null)
		{
			GetOrThrow(item).OverrideSize = size;
		}
		return this;
	}

	private void Resolve(RelativeLayoutParamsBase<GameObject>.EdgeStatus dest, RelativeLayoutParamsBase<IUIComponent>.EdgeStatus status, IDictionary<IUIComponent, GameObject> mapping)
	{
		IUIComponent fromComponent = status.FromComponent;
		dest.FromAnchor = status.FromAnchor;
		if (fromComponent != null)
		{
			dest.FromComponent = mapping[fromComponent];
		}
		dest.Constraint = status.Constraint;
		dest.Offset = status.Offset;
	}

	public PRelativePanel SetBottomEdge(IUIComponent item, float fraction = -1f, IUIComponent above = null)
	{
		if (item != null)
		{
			if (above == item)
			{
				throw new ArgumentException("Component cannot refer directly to itself");
			}
			SetEdge(GetOrThrow(item).BottomEdge, fraction, above);
		}
		return this;
	}

	private void SetEdge(RelativeLayoutParamsBase<IUIComponent>.EdgeStatus edge, float fraction, IUIComponent child)
	{
		if (fraction >= 0f && fraction <= 1f)
		{
			edge.Constraint = RelativeConstraintType.ToAnchor;
			edge.FromAnchor = fraction;
			edge.FromComponent = null;
		}
		else if (child != null)
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

	public PRelativePanel SetLeftEdge(IUIComponent item, float fraction = -1f, IUIComponent toRight = null)
	{
		if (item != null)
		{
			if (toRight == item)
			{
				throw new ArgumentException("Component cannot refer directly to itself");
			}
			SetEdge(GetOrThrow(item).LeftEdge, fraction, toRight);
		}
		return this;
	}

	public PRelativePanel SetMargin(IUIComponent item, RectOffset insets)
	{
		if (item != null)
		{
			GetOrThrow(item).Insets = insets;
		}
		return this;
	}

	public PRelativePanel SetRightEdge(IUIComponent item, float fraction = -1f, IUIComponent toLeft = null)
	{
		if (item != null)
		{
			if (toLeft == item)
			{
				throw new ArgumentException("Component cannot refer directly to itself");
			}
			SetEdge(GetOrThrow(item).RightEdge, fraction, toLeft);
		}
		return this;
	}

	public PRelativePanel SetTopEdge(IUIComponent item, float fraction = -1f, IUIComponent below = null)
	{
		if (item != null)
		{
			if (below == item)
			{
				throw new ArgumentException("Component cannot refer directly to itself");
			}
			SetEdge(GetOrThrow(item).TopEdge, fraction, below);
		}
		return this;
	}

	public override string ToString()
	{
		return $"PRelativePanel[Name={base.Name}]";
	}
}
