using UnityEngine;

namespace PeterHan.PLib.UI;

public sealed class PUIDelegates
{
	public delegate void OnDialogClosed(string option);

	public delegate void OnButtonPressed(GameObject source);

	public delegate void OnChecked(GameObject source, int state);

	public delegate void OnDropdownChanged<T>(GameObject source, T choice) where T : class, IListableOption;

	public delegate void OnRealize(GameObject realized);

	public delegate void OnSliderChanged(GameObject source, float newValue);

	public delegate void OnSliderDrag(GameObject source, float newValue);

	public delegate void OnTextChanged(GameObject source, string text);

	public delegate void OnToggleButton(GameObject source, bool on);
}
