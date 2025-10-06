using UnityEngine;

namespace PeterHan.PLib.UI;

public interface IUIComponent
{
	string Name { get; }

	event PUIDelegates.OnRealize OnRealize;

	GameObject Build();
}
