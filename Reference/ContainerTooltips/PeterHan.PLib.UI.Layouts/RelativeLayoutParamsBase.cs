using System;
using UnityEngine;

namespace PeterHan.PLib.UI.Layouts;

internal class RelativeLayoutParamsBase<T>
{
	[Serializable]
	internal sealed class EdgeStatus
	{
		internal RelativeConstraintType Constraint;

		internal float FromAnchor;

		internal T FromComponent;

		internal float Offset;

		public bool Locked => Constraint == RelativeConstraintType.Locked;

		public bool Unconstrained => Constraint == RelativeConstraintType.Unconstrained;

		internal EdgeStatus()
		{
			FromAnchor = 0f;
			FromComponent = default(T);
			Offset = 0f;
			Constraint = RelativeConstraintType.Unconstrained;
		}

		internal void CopyFrom(EdgeStatus other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			switch (Constraint = other.Constraint)
			{
			case RelativeConstraintType.ToComponent:
				FromComponent = other.FromComponent;
				break;
			case RelativeConstraintType.ToAnchor:
				FromAnchor = other.FromAnchor;
				break;
			case RelativeConstraintType.Locked:
				FromAnchor = other.FromAnchor;
				Offset = other.Offset;
				break;
			case RelativeConstraintType.Unconstrained:
				break;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is EdgeStatus edgeStatus && edgeStatus.FromAnchor == FromAnchor && edgeStatus.Offset == Offset)
			{
				return Constraint == edgeStatus.Constraint;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 37 * (37 * FromAnchor.GetHashCode() + Offset.GetHashCode()) + Constraint.GetHashCode();
		}

		public void Reset()
		{
			Constraint = RelativeConstraintType.Unconstrained;
			Offset = 0f;
			FromAnchor = 0f;
			FromComponent = default(T);
		}

		public override string ToString()
		{
			return string.Format("EdgeStatus[Constraints={2},Anchor={0:F2},Offset={1:F2}]", FromAnchor, Offset, Constraint);
		}
	}

	internal EdgeStatus BottomEdge { get; }

	internal RectOffset Insets { get; set; }

	internal EdgeStatus LeftEdge { get; }

	internal Vector2 OverrideSize { get; set; }

	internal EdgeStatus RightEdge { get; }

	internal EdgeStatus TopEdge { get; }

	internal RelativeLayoutParamsBase()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Insets = null;
		BottomEdge = new EdgeStatus();
		LeftEdge = new EdgeStatus();
		RightEdge = new EdgeStatus();
		TopEdge = new EdgeStatus();
		OverrideSize = Vector2.zero;
	}
}
