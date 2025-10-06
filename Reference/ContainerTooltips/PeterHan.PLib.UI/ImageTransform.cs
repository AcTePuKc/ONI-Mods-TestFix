using System;

namespace PeterHan.PLib.UI;

[Flags]
public enum ImageTransform : uint
{
	None = 0u,
	FlipHorizontal = 1u,
	FlipVertical = 2u,
	Rotate90 = 4u,
	Rotate180 = 8u,
	Rotate270 = 0xCu
}
