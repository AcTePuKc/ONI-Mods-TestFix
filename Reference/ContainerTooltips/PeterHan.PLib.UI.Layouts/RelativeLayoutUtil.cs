using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI.Layouts;

internal static class RelativeLayoutUtil
{
	internal static void CalcX(this ICollection<RelativeLayoutResults> children, RectTransform all, IDictionary<GameObject, RelativeLayoutParams> constraints)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Component, RelativeLayoutGroup> val = ListPool<Component, RelativeLayoutGroup>.Allocate();
		PooledDictionary<GameObject, RelativeLayoutResults, RelativeLayoutGroup> val2 = DictionaryPool<GameObject, RelativeLayoutResults, RelativeLayoutGroup>.Allocate();
		int childCount = ((Transform)all).childCount;
		children.Clear();
		for (int i = 0; i < childCount; i++)
		{
			Transform child = ((Transform)all).GetChild(i);
			GameObject val3 = ((child != null) ? ((Component)child).gameObject : null);
			if (!((Object)(object)val3 != (Object)null))
			{
				continue;
			}
			((List<Component>)(object)val).Clear();
			val3.GetComponents<Component>((List<Component>)(object)val);
			LayoutSizes layoutSizes = PUIUtils.CalcSizes(val3, PanelDirection.Horizontal, (IEnumerable<Component>)val);
			if (layoutSizes.ignore)
			{
				continue;
			}
			float preferredWidth = layoutSizes.preferred;
			RelativeLayoutResults relativeLayoutResults;
			if (constraints.TryGetValue(val3, out var value))
			{
				relativeLayoutResults = new RelativeLayoutResults(Util.rectTransform(val3), value);
				Vector2 overrideSize = relativeLayoutResults.OverrideSize;
				if (overrideSize.x > 0f)
				{
					preferredWidth = overrideSize.x;
				}
				((Dictionary<GameObject, RelativeLayoutResults>)(object)val2)[val3] = relativeLayoutResults;
			}
			else
			{
				relativeLayoutResults = new RelativeLayoutResults(Util.rectTransform(val3), null);
			}
			relativeLayoutResults.PreferredWidth = preferredWidth;
			children.Add(relativeLayoutResults);
		}
		foreach (RelativeLayoutResults child2 in children)
		{
			child2.TopParams = InitResolve(child2.TopEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
			child2.BottomParams = InitResolve(child2.BottomEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
			child2.LeftParams = InitResolve(child2.LeftEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
			child2.RightParams = InitResolve(child2.RightEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
		}
		val2.Recycle();
		val.Recycle();
	}

	internal static void CalcY(this ICollection<RelativeLayoutResults> children)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Component, RelativeLayoutGroup> val = ListPool<Component, RelativeLayoutGroup>.Allocate();
		foreach (RelativeLayoutResults child in children)
		{
			GameObject gameObject = ((Component)child.Transform).gameObject;
			Vector2 overrideSize = child.OverrideSize;
			((List<Component>)(object)val).Clear();
			gameObject.gameObject.GetComponents<Component>((List<Component>)(object)val);
			float preferredHeight = PUIUtils.CalcSizes(gameObject, PanelDirection.Vertical, (IEnumerable<Component>)val).preferred;
			if (overrideSize.y > 0f)
			{
				preferredHeight = overrideSize.y;
			}
			child.PreferredHeight = preferredHeight;
		}
		val.Recycle();
	}

	internal static float ElbowRoom(RelativeLayoutParamsBase<GameObject>.EdgeStatus min, RelativeLayoutParamsBase<GameObject>.EdgeStatus max, float effective)
	{
		float fromAnchor = min.FromAnchor;
		float fromAnchor2 = max.FromAnchor;
		float offset = min.Offset;
		float offset2 = max.Offset;
		if (fromAnchor2 > fromAnchor)
		{
			return (effective + offset - offset2) / (fromAnchor2 - fromAnchor);
		}
		return Math.Max(effective, Math.Max(Math.Abs(offset), Math.Abs(offset2)));
	}

	internal static void ExecuteX(this IEnumerable<RelativeLayoutResults> children, List<ILayoutController> scratch, float mLeft = 0f, float mRight = 0f)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		foreach (RelativeLayoutResults child in children)
		{
			RectTransform transform = child.Transform;
			RectOffset insets = child.Insets;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus leftEdge = child.LeftEdge;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus rightEdge = child.RightEdge;
			transform.anchorMin = new Vector2(leftEdge.FromAnchor, 0f);
			transform.anchorMax = new Vector2(rightEdge.FromAnchor, 1f);
			if (child.UseSizeDeltaX)
			{
				transform.SetSizeWithCurrentAnchors((Axis)0, child.PreferredWidth);
			}
			else
			{
				transform.offsetMin = new Vector2(leftEdge.Offset + (float)insets.left + ((leftEdge.FromAnchor <= 0f) ? mLeft : 0f), transform.offsetMin.y);
				transform.offsetMax = new Vector2(rightEdge.Offset - (float)insets.right - ((rightEdge.FromAnchor >= 1f) ? mRight : 0f), transform.offsetMax.y);
			}
			scratch.Clear();
			((Component)transform).gameObject.GetComponents<ILayoutController>(scratch);
			foreach (ILayoutController item in scratch)
			{
				item.SetLayoutHorizontal();
			}
		}
	}

	internal static void ExecuteY(this IEnumerable<RelativeLayoutResults> children, List<ILayoutController> scratch, float mBottom = 0f, float mTop = 0f)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		foreach (RelativeLayoutResults child in children)
		{
			RectTransform transform = child.Transform;
			RectOffset insets = child.Insets;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus topEdge = child.TopEdge;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus bottomEdge = child.BottomEdge;
			transform.anchorMin = new Vector2(transform.anchorMin.x, bottomEdge.FromAnchor);
			transform.anchorMax = new Vector2(transform.anchorMax.x, topEdge.FromAnchor);
			if (child.UseSizeDeltaY)
			{
				transform.SetSizeWithCurrentAnchors((Axis)1, child.PreferredHeight);
			}
			else
			{
				transform.offsetMin = new Vector2(transform.offsetMin.x, bottomEdge.Offset + (float)insets.bottom + ((bottomEdge.FromAnchor <= 0f) ? mBottom : 0f));
				transform.offsetMax = new Vector2(transform.offsetMax.x, topEdge.Offset - (float)insets.top - ((topEdge.FromAnchor >= 1f) ? mTop : 0f));
			}
			scratch.Clear();
			((Component)transform).gameObject.GetComponents<ILayoutController>(scratch);
			foreach (ILayoutController item in scratch)
			{
				item.SetLayoutVertical();
			}
		}
	}

	internal static float GetMinSizeX(this IEnumerable<RelativeLayoutResults> children)
	{
		float num = 0f;
		foreach (RelativeLayoutResults child in children)
		{
			float num2 = ElbowRoom(child.LeftEdge, child.RightEdge, child.EffectiveWidth);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	internal static float GetMinSizeY(this IEnumerable<RelativeLayoutResults> children)
	{
		float num = 0f;
		foreach (RelativeLayoutResults child in children)
		{
			float num2 = ElbowRoom(child.BottomEdge, child.TopEdge, child.EffectiveHeight);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	private static RelativeLayoutResults InitResolve(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, IDictionary<GameObject, RelativeLayoutResults> lookup)
	{
		RelativeLayoutResults value = null;
		if (edge.Constraint == RelativeConstraintType.ToComponent && !lookup.TryGetValue(edge.FromComponent, out value))
		{
			edge.Constraint = RelativeConstraintType.Unconstrained;
		}
		return value;
	}

	private static bool LockEdgeAnchor(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, RelativeLayoutParamsBase<GameObject>.EdgeStatus otherEdge)
	{
		int num;
		if (edge.Constraint == RelativeConstraintType.ToAnchor && otherEdge.Constraint == RelativeConstraintType.ToAnchor)
		{
			num = ((edge.FromAnchor == otherEdge.FromAnchor) ? 1 : 0);
			if (num != 0)
			{
				edge.Constraint = RelativeConstraintType.Locked;
				otherEdge.Constraint = RelativeConstraintType.Locked;
				edge.Offset = 0f;
				otherEdge.Offset = 0f;
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private static void LockEdgeAnchor(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge)
	{
		if (edge.Constraint == RelativeConstraintType.ToAnchor)
		{
			edge.Constraint = RelativeConstraintType.Locked;
			edge.Offset = 0f;
		}
	}

	private static void LockEdgeComponent(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, RelativeLayoutParamsBase<GameObject>.EdgeStatus otherEdge)
	{
		if (edge.Constraint == RelativeConstraintType.ToComponent && otherEdge.Locked)
		{
			edge.Constraint = RelativeConstraintType.Locked;
			edge.FromAnchor = otherEdge.FromAnchor;
			edge.Offset = otherEdge.Offset;
		}
	}

	private static void LockEdgeRelative(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, float size, RelativeLayoutParamsBase<GameObject>.EdgeStatus opposing)
	{
		if (edge.Constraint == RelativeConstraintType.Unconstrained)
		{
			if (opposing.Locked)
			{
				edge.Constraint = RelativeConstraintType.Locked;
				edge.FromAnchor = opposing.FromAnchor;
				edge.Offset = opposing.Offset + size;
			}
			else if (opposing.Constraint == RelativeConstraintType.Unconstrained)
			{
				edge.Constraint = RelativeConstraintType.Locked;
				edge.FromAnchor = 0f;
				edge.Offset = 0f;
				opposing.Constraint = RelativeConstraintType.Locked;
				opposing.FromAnchor = 1f;
				opposing.Offset = 0f;
			}
		}
	}

	internal static bool RunPassX(this IEnumerable<RelativeLayoutResults> children)
	{
		bool result = true;
		foreach (RelativeLayoutResults child in children)
		{
			float effectiveWidth = child.EffectiveWidth;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus leftEdge = child.LeftEdge;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus rightEdge = child.RightEdge;
			if (LockEdgeAnchor(leftEdge, rightEdge))
			{
				child.UseSizeDeltaX = true;
			}
			LockEdgeAnchor(leftEdge);
			LockEdgeAnchor(rightEdge);
			LockEdgeRelative(leftEdge, 0f - effectiveWidth, rightEdge);
			LockEdgeRelative(rightEdge, effectiveWidth, leftEdge);
			if (child.LeftParams != null)
			{
				LockEdgeComponent(leftEdge, child.LeftParams.RightEdge);
			}
			if (child.RightParams != null)
			{
				LockEdgeComponent(rightEdge, child.RightParams.LeftEdge);
			}
			if (!leftEdge.Locked || !rightEdge.Locked)
			{
				result = false;
			}
		}
		return result;
	}

	internal static bool RunPassY(this IEnumerable<RelativeLayoutResults> children)
	{
		bool result = true;
		foreach (RelativeLayoutResults child in children)
		{
			float effectiveHeight = child.EffectiveHeight;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus topEdge = child.TopEdge;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus bottomEdge = child.BottomEdge;
			if (LockEdgeAnchor(topEdge, bottomEdge))
			{
				child.UseSizeDeltaY = true;
			}
			LockEdgeAnchor(bottomEdge);
			LockEdgeAnchor(topEdge);
			LockEdgeRelative(bottomEdge, 0f - effectiveHeight, topEdge);
			LockEdgeRelative(topEdge, effectiveHeight, bottomEdge);
			if (child.BottomParams != null)
			{
				LockEdgeComponent(bottomEdge, child.BottomParams.TopEdge);
			}
			if (child.TopParams != null)
			{
				LockEdgeComponent(topEdge, child.TopParams.BottomEdge);
			}
			if (!topEdge.Locked || !bottomEdge.Locked)
			{
				result = false;
			}
		}
		return result;
	}

	internal static void ThrowUnresolvable(this IEnumerable<RelativeLayoutResults> children, int limit, PanelDirection direction)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.Append("After ").Append(limit);
		stringBuilder.Append(" passes, unable to complete resolution of RelativeLayout:");
		foreach (RelativeLayoutResults child in children)
		{
			string name = ((Object)((Component)child.Transform).gameObject).name;
			if (direction == PanelDirection.Horizontal)
			{
				if (!child.LeftEdge.Locked)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(name);
					stringBuilder.Append(".Left");
				}
				if (!child.RightEdge.Locked)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(name);
					stringBuilder.Append(".Right");
				}
			}
			else
			{
				if (!child.BottomEdge.Locked)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(name);
					stringBuilder.Append(".Bottom");
				}
				if (!child.TopEdge.Locked)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(name);
					stringBuilder.Append(".Top");
				}
			}
		}
		throw new InvalidOperationException(stringBuilder.ToString());
	}
}
