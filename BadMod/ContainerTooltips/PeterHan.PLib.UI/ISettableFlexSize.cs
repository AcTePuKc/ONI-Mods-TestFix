using UnityEngine.UI;

namespace PeterHan.PLib.UI;

internal interface ISettableFlexSize : ILayoutGroup, ILayoutController
{
	float flexibleWidth { get; set; }

	float flexibleHeight { get; set; }
}
