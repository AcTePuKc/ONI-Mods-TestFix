namespace PeterHan.PLib.Actions;

public sealed class PKeyBinding
{
	public GamepadButton GamePadButton { get; set; }

	public KKeyCode Key { get; set; }

	public Modifier Modifiers { get; set; }

	public PKeyBinding(KKeyCode keyCode = (KKeyCode)0, Modifier modifiers = (Modifier)0, GamepadButton gamePadButton = (GamepadButton)16)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		GamePadButton = gamePadButton;
		Key = keyCode;
		Modifiers = modifiers;
	}

	public PKeyBinding(PKeyBinding other)
		: this((KKeyCode)0, (Modifier)0, (GamepadButton)16)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (other != null)
		{
			GamePadButton = other.GamePadButton;
			Key = other.Key;
			Modifiers = other.Modifiers;
		}
	}

	public override bool Equals(object obj)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (obj is PKeyBinding pKeyBinding && pKeyBinding.Key == Key && pKeyBinding.Modifiers == Modifiers)
		{
			return pKeyBinding.GamePadButton == GamePadButton;
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		return ((1488379021 + ((object)GamePadButton/*cast due to .constrained prefix*/).GetHashCode()) * -1521134295 + ((object)Key/*cast due to .constrained prefix*/).GetHashCode()) * -1521134295 + ((object)Modifiers/*cast due to .constrained prefix*/).GetHashCode();
	}

	public override string ToString()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return ((object)Modifiers/*cast due to .constrained prefix*/).ToString() + " " + ((object)Key/*cast due to .constrained prefix*/).ToString();
	}
}
