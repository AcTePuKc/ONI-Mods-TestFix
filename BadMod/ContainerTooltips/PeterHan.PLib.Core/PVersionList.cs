using System.Collections.Generic;

namespace PeterHan.PLib.Core;

internal sealed class PVersionList
{
	public List<PForwardedComponent> Components { get; }

	public object SharedData { get; set; }

	public PVersionList()
	{
		Components = new List<PForwardedComponent>(32);
		SharedData = null;
	}

	public override string ToString()
	{
		return Components.ToString();
	}
}
