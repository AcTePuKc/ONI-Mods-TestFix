using System;
using Klei.AI;
using UnityEngine;

namespace ContainerTooltips
{
    public sealed class StorageContentsBehaviour : KMonoBehaviour
    {
        private static readonly EventSystem.IntraObjectHandler<StorageContentsBehaviour> OnStorageChangedHandler =
            new((component, _) => component.OnStorageChanged());

        private Guid statusHandle;
        private Storage? storage;
        private KSelectable? selectable;

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            storage = GetComponent<Storage>();
            selectable = GetComponent<KSelectable>();
            Subscribe((int)GameHashes.OnStorageChange, OnStorageChangedHandler);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            RefreshStatus();
        }

        public override void OnCleanUp()
        {
            ClearStatus();
            Unsubscribe((int)GameHashes.OnStorageChange, OnStorageChangedHandler);
            base.OnCleanUp();
        }

        private void OnStorageChanged()
        {
            if (storage != null)
            {
                UserMod.InvalidateCache(storage);
            }

            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (storage == null || selectable == null)
            {
                Debug.LogWarning($"[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus missing dependencies on {name}");
                ClearStatus();
                return;
            }

            if (UserMod.ContentsStatusItem == null)
            {
                Debug.LogWarning("[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus found null ContentsStatusItem");
                ClearStatus();
                return;
            }

            if (statusHandle != Guid.Empty && !storage.showInUI)
            {
                ClearStatus();
                return;
            }

            statusHandle = selectable.ReplaceStatusItem(statusHandle, UserMod.ContentsStatusItem, storage);
        }

        private void ClearStatus()
        {
            if (statusHandle == Guid.Empty || selectable == null)
            {
                return;
            }

            selectable.RemoveStatusItem(statusHandle);
            statusHandle = Guid.Empty;
        }
    }
}
