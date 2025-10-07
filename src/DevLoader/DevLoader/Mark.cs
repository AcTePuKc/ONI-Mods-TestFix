using UnityEngine;

namespace DevLoader;

public static class Mark
{
	public static void Add(GameObject go)
	{
		if (!((Object)(object)go == (Object)null) && (Object)(object)go.GetComponent<Marker>() == (Object)null)
		{
			go.AddComponent<Marker>();
		}
	}
}
