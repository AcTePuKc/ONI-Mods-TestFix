using System;

namespace PeterHan.PLib.Actions;

public sealed class PAction
{
	private readonly int id;

	public static Action MaxAction { get; }

	internal PKeyBinding DefaultBinding { get; }

	public string Identifier { get; }

	public LocString Title { get; }

	static PAction()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!Enum.TryParse<Action>("NumActions", out Action result))
		{
			result = (Action)278;
		}
		MaxAction = result;
	}

	internal PAction(int id, string identifier, LocString title, PKeyBinding binding)
	{
		if (id <= 0)
		{
			throw new ArgumentOutOfRangeException("id");
		}
		DefaultBinding = binding;
		Identifier = identifier;
		this.id = id;
		Title = title;
	}

	public override bool Equals(object obj)
	{
		if (obj is PAction pAction)
		{
			return pAction.id == id;
		}
		return false;
	}

	public Action GetKAction()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return (Action)(MaxAction + id);
	}

	public override int GetHashCode()
	{
		return id;
	}

	public override string ToString()
	{
		return "PAction[" + Identifier + "]: " + LocString.op_Implicit(Title);
	}
}
