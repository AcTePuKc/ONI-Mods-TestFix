using UnityEngine;

namespace PeterHan.PLib.UI;

public static class TextAnchorUtils
{
	public static bool IsLeft(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		if ((int)anchor != 0 && (int)anchor != 3)
		{
			return (int)anchor == 6;
		}
		return true;
	}

	public static bool IsLower(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		if ((int)anchor != 7 && (int)anchor != 6)
		{
			return (int)anchor == 8;
		}
		return true;
	}

	public static bool IsRight(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		if ((int)anchor != 2 && (int)anchor != 5)
		{
			return (int)anchor == 8;
		}
		return true;
	}

	public static bool IsUpper(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		if ((int)anchor != 1 && (int)anchor != 0)
		{
			return (int)anchor == 2;
		}
		return true;
	}

	public static TextAnchor MirrorHorizontal(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		int num = (int)anchor;
		num = 3 * (num / 3) + 2 - num % 3;
		return (TextAnchor)num;
	}

	public static TextAnchor MirrorVertical(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		int num = (int)anchor;
		num = 6 - 3 * (num / 3) + num % 3;
		return (TextAnchor)num;
	}
}
