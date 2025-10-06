using System;
using System.Collections.Generic;
using PeterHan.PLib.UI.Layouts;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public sealed class BoxLayoutGroup : AbstractLayoutGroup
{
	private BoxLayoutResults horizontal;

	[SerializeField]
	private BoxLayoutParams parameters;

	private BoxLayoutResults vertical;

	public BoxLayoutParams Params
	{
		get
		{
			return parameters;
		}
		set
		{
			parameters = value ?? throw new ArgumentNullException("Params");
		}
	}

	private static BoxLayoutResults Calc(GameObject obj, BoxLayoutParams args, PanelDirection direction)
	{
		RectTransform val = EntityTemplateExtensions.AddOrGet<RectTransform>(obj);
		int childCount = ((Transform)val).childCount;
		BoxLayoutResults boxLayoutResults = new BoxLayoutResults(direction, childCount);
		PooledList<Component, BoxLayoutGroup> val2 = ListPool<Component, BoxLayoutGroup>.Allocate();
		for (int i = 0; i < childCount; i++)
		{
			Transform child = ((Transform)val).GetChild(i);
			GameObject val3 = ((child != null) ? ((Component)child).gameObject : null);
			if (!((Object)(object)val3 != (Object)null) || !val3.activeInHierarchy)
			{
				continue;
			}
			((List<Component>)(object)val2).Clear();
			val3.GetComponents<Component>((List<Component>)(object)val2);
			LayoutSizes layoutSizes = PUIUtils.CalcSizes(val3, direction, (IEnumerable<Component>)val2);
			if (!layoutSizes.ignore)
			{
				if (args.Direction == direction)
				{
					boxLayoutResults.Accum(layoutSizes, args.Spacing);
				}
				else
				{
					boxLayoutResults.Expand(layoutSizes);
				}
				boxLayoutResults.children.Add(layoutSizes);
			}
		}
		val2.Recycle();
		return boxLayoutResults;
	}

	private static void DoLayout(BoxLayoutParams args, BoxLayoutResults required, float size)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (required == null)
		{
			throw new ArgumentNullException("required");
		}
		PanelDirection direction = required.direction;
		BoxLayoutStatus status = new BoxLayoutStatus(direction, (RectOffset)(((object)args.Margin) ?? ((object)new RectOffset())), size);
		if (args.Direction == direction)
		{
			DoLayoutLinear(required, args, status);
		}
		else
		{
			DoLayoutPerp(required, args, status);
		}
	}

	private static void DoLayoutLinear(BoxLayoutResults required, BoxLayoutParams args, BoxLayoutStatus status)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		LayoutSizes total = required.total;
		PooledList<ILayoutController, BoxLayoutGroup> val = ListPool<ILayoutController, BoxLayoutGroup>.Allocate();
		PanelDirection direction = args.Direction;
		float size = status.size;
		float num = 0f;
		float min = total.min;
		float preferred = total.preferred;
		float num2 = Math.Max(0f, size - preferred);
		float flexible = total.flexible;
		float num3 = status.offset;
		float spacing = args.Spacing;
		if (size > min && preferred > min)
		{
			num = Math.Min(1f, (size - min) / (preferred - min));
		}
		if (num2 > 0f && flexible == 0f)
		{
			num3 += PUIUtils.GetOffset(args.Alignment, status.direction, num2);
		}
		foreach (LayoutSizes child in required.children)
		{
			GameObject source = child.source;
			if (!((Object)(object)source != (Object)null) || !source.activeInHierarchy)
			{
				continue;
			}
			float num4 = child.min;
			if (num > 0f)
			{
				num4 += (child.preferred - child.min) * num;
			}
			if (num2 > 0f && flexible > 0f)
			{
				num4 += num2 * child.flexible / flexible;
			}
			EntityTemplateExtensions.AddOrGet<RectTransform>(source).SetInsetAndSizeFromParentEdge(status.edge, num3, num4);
			num3 += num4 + ((num4 > 0f) ? spacing : 0f);
			((List<ILayoutController>)(object)val).Clear();
			source.GetComponents<ILayoutController>((List<ILayoutController>)(object)val);
			foreach (ILayoutController item in (List<ILayoutController>)(object)val)
			{
				if (direction == PanelDirection.Horizontal)
				{
					item.SetLayoutHorizontal();
				}
				else
				{
					item.SetLayoutVertical();
				}
			}
		}
		val.Recycle();
	}

	private static void DoLayoutPerp(BoxLayoutResults required, BoxLayoutParams args, BoxLayoutStatus status)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		PooledList<ILayoutController, BoxLayoutGroup> val = ListPool<ILayoutController, BoxLayoutGroup>.Allocate();
		PanelDirection direction = args.Direction;
		float size = status.size;
		foreach (LayoutSizes child in required.children)
		{
			GameObject source = child.source;
			if (!((Object)(object)source != (Object)null) || !source.activeInHierarchy)
			{
				continue;
			}
			float num = size;
			if (child.flexible <= 0f)
			{
				num = Math.Min(num, child.preferred);
			}
			float num2 = ((size > num) ? PUIUtils.GetOffset(args.Alignment, status.direction, size - num) : 0f);
			EntityTemplateExtensions.AddOrGet<RectTransform>(source).SetInsetAndSizeFromParentEdge(status.edge, num2 + status.offset, num);
			((List<ILayoutController>)(object)val).Clear();
			source.GetComponents<ILayoutController>((List<ILayoutController>)(object)val);
			foreach (ILayoutController item in (List<ILayoutController>)(object)val)
			{
				if (direction == PanelDirection.Horizontal)
				{
					item.SetLayoutVertical();
				}
				else
				{
					item.SetLayoutHorizontal();
				}
			}
		}
		val.Recycle();
	}

	internal BoxLayoutGroup()
	{
		horizontal = null;
		base.layoutPriority = 1;
		parameters = new BoxLayoutParams();
		vertical = null;
	}

	public override void CalculateLayoutInputHorizontal()
	{
		if (!locked)
		{
			RectOffset margin = parameters.Margin;
			float num = ((margin == null) ? 0f : ((float)(margin.left + margin.right)));
			horizontal = Calc(((Component)this).gameObject, parameters, PanelDirection.Horizontal);
			LayoutSizes total = horizontal.total;
			base.minWidth = total.min + num;
			base.preferredWidth = total.preferred + num;
		}
	}

	public override void CalculateLayoutInputVertical()
	{
		if (!locked)
		{
			RectOffset margin = parameters.Margin;
			float num = ((margin == null) ? 0f : ((float)(margin.top + margin.bottom)));
			vertical = Calc(((Component)this).gameObject, parameters, PanelDirection.Vertical);
			LayoutSizes total = vertical.total;
			base.minHeight = total.min + num;
			base.preferredHeight = total.preferred + num;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		horizontal = null;
		vertical = null;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		horizontal = null;
		vertical = null;
	}

	public override void SetLayoutHorizontal()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (horizontal != null && !locked)
		{
			BoxLayoutParams args = parameters;
			BoxLayoutResults required = horizontal;
			Rect rect = base.rectTransform.rect;
			DoLayout(args, required, ((Rect)(ref rect)).width);
		}
	}

	public override void SetLayoutVertical()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (vertical != null && !locked)
		{
			BoxLayoutParams args = parameters;
			BoxLayoutResults required = vertical;
			Rect rect = base.rectTransform.rect;
			DoLayout(args, required, ((Rect)(ref rect)).height);
		}
	}
}
