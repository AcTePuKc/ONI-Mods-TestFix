using System;
using UnityEngine;

namespace PeterHan.PLib.UI.Layouts;

internal sealed class RelativeLayoutResults : RelativeLayoutParamsBase<GameObject>
{
	private static readonly RectOffset ZERO = new RectOffset();

	private Vector2 prefSize;

	internal RelativeLayoutResults BottomParams { get; set; }

	internal float EffectiveHeight { get; private set; }

	internal float EffectiveWidth { get; private set; }

	internal RelativeLayoutResults LeftParams { get; set; }

	internal float PreferredHeight
	{
		get
		{
			return prefSize.y;
		}
		set
		{
			prefSize.y = value;
			EffectiveHeight = value + (float)base.Insets.top + (float)base.Insets.bottom;
		}
	}

	internal float PreferredWidth
	{
		get
		{
			return prefSize.x;
		}
		set
		{
			prefSize.x = value;
			EffectiveWidth = value + (float)base.Insets.left + (float)base.Insets.right;
		}
	}

	internal RelativeLayoutResults RightParams { get; set; }

	internal RelativeLayoutResults TopParams { get; set; }

	internal RectTransform Transform { get; set; }

	internal bool UseSizeDeltaX { get; set; }

	internal bool UseSizeDeltaY { get; set; }

	internal RelativeLayoutResults(RectTransform transform, RelativeLayoutParams other)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Transform = transform ?? throw new ArgumentNullException("transform");
		if (other != null)
		{
			base.BottomEdge.CopyFrom(other.BottomEdge);
			base.TopEdge.CopyFrom(other.TopEdge);
			base.RightEdge.CopyFrom(other.RightEdge);
			base.LeftEdge.CopyFrom(other.LeftEdge);
			base.Insets = other.Insets ?? ZERO;
			base.OverrideSize = other.OverrideSize;
		}
		else
		{
			base.Insets = ZERO;
		}
		BottomParams = (LeftParams = (TopParams = (RightParams = null)));
		PreferredWidth = (PreferredHeight = 0f);
		UseSizeDeltaX = (UseSizeDeltaY = false);
	}

	public override string ToString()
	{
		GameObject gameObject = ((Component)Transform).gameObject;
		return string.Format("component={0} {1:F2}x{2:F2}", ((Object)(object)gameObject == (Object)null) ? "null" : ((Object)gameObject).name, prefSize.x, prefSize.y);
	}
}
