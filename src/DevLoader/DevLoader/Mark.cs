using UnityEngine;
using Object = UnityEngine.Object;

namespace DevLoader;

public static class Mark
{
        public static void Add(GameObject go)
        {
                if ((Object)go != null && (Object)go.GetComponent<Marker>() == null)
                {
                        go.AddComponent<Marker>();
                }
        }
}
