using System;
using UnityEngine;

namespace ContainerTooltips;

public sealed class StorageContentsBehaviour : KMonoBehaviour
{
	private Guid statusHandle;

	private Storage? storage;

	private KSelectable? selectable;

	protected override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		storage = ((Component)this).GetComponent<Storage>();
		selectable = ((Component)this).GetComponent<KSelectable>();
	}

	protected override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		RefreshStatus();
	}

	protected override void OnCleanUp()
	{
		((KMonoBehaviour)this).OnCleanUp();
		Debug.Log((object)("[ContainerTooltips]: StorageContentsBehaviour.OnCleanUp on " + ((Object)((Component)this).gameObject).name + " storage=" + (((Object)(object)storage != (Object)null) ? ((Object)storage).name : "<null>") + " selectable=" + (((Object)(object)selectable != (Object)null) ? ((Object)selectable).name : "<null>")));
		ClearStatus();
	}

	private void OnStorageChange(object _)
	{
		Debug.Log((object)("[ContainerTooltips]: StorageContentsBehaviour.OnStorageChange event received for " + ((Object)((Component)this).gameObject).name + " storage=" + (((Object)(object)storage != (Object)null) ? ((Object)storage).name : "<null>") + " selectable=" + (((Object)(object)selectable != (Object)null) ? ((Object)selectable).name : "<null>")));
		RefreshStatus();
	}

	private void RefreshStatus()
	{
		if (statusHandle != Guid.Empty)
		{
			Debug.Log((object)string.Format("[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus called on {0} storage={1} selectable={2} handle={3}", ((Object)((Component)this).gameObject).name, ((Object)(object)storage != (Object)null) ? ((Object)storage).name : "<null>", ((Object)(object)selectable != (Object)null) ? ((Object)selectable).name : "<null>", statusHandle));
		}
		if ((Object)(object)storage == (Object)null || (Object)(object)selectable == (Object)null)
		{
			Debug.LogWarning((object)string.Format("[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus missing storage or selectable on {0} storage={1} selectable={2} handle={3}", ((Object)((Component)this).gameObject).name, ((Object)(object)storage != (Object)null) ? ((Object)storage).name : "<null>", ((Object)(object)selectable != (Object)null) ? ((Object)selectable).name : "<null>", statusHandle));
		}
		else if (UserMod.ContentsStatusItem == null)
		{
			Debug.LogError((object)"[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus found null contentsStatusItem");
			ClearStatus();
		}
		else if (statusHandle != Guid.Empty && !storage.showInUI)
		{
			Debug.Log((object)"[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus cleaning our status item since storage is now set to not show in UI");
			ClearStatus();
		}
		else
		{
			Guid guid = selectable.ReplaceStatusItem(statusHandle, UserMod.ContentsStatusItem, (object)storage);
			if (statusHandle != Guid.Empty)
			{
				Debug.Log((object)$"[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus applied status item on {((Object)((Component)this).gameObject).name}, new handle={guid}");
			}
			statusHandle = guid;
		}
	}

	private void ClearStatus()
	{
		if (statusHandle != Guid.Empty && (Object)(object)selectable != (Object)null)
		{
			selectable.RemoveStatusItem(statusHandle, false);
			Debug.Log((object)$"[ContainerTooltips]: StorageContentsBehaviour.ClearStatus removed status item on {((Object)((Component)this).gameObject).name}, handle={statusHandle}");
			statusHandle = Guid.Empty;
		}
	}
}
