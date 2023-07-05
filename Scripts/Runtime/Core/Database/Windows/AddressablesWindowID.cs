#if USE_ADDRESSABLES
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BrunoMikoski.UIManager
{
    public sealed class AddressablesWindowID : WindowID, IAsyncPrefabLoader
    {
        [SerializeField] 
        private AssetReferenceGameObject windowPrefabAssetRef;
        
        [NonSerialized]
        private Window cachedWindowPrefab;

        private AsyncOperationHandle<GameObject>? loadingOperation;
        
        
        public override Window GetWindowPrefab()
        {
            if (cachedWindowPrefab == null)
            {
                Debug.LogWarning(
                    $"{this.name} implements IAsyncPrefabLoader but it's not loaded yet, please make sure to load the group {GroupID} before trying to get the prefab");
                GetOrCreateLoadingOperation();
                CacheWindowPrefabFromLoadingOperation();
            }
            return cachedWindowPrefab;
        }

        private void GetOrCreateLoadingOperation()
        {
            if (loadingOperation != null)
                return;

            if (!windowPrefabAssetRef.RuntimeKeyIsValid())
                return;
            
            loadingOperation = windowPrefabAssetRef.InstantiateAsync();
        }

        IEnumerator IAsyncPrefabLoader.LoadWindowPrefab()
        {
            GetOrCreateLoadingOperation();
            while (!loadingOperation.Value.IsDone)
                yield return null;

            CacheWindowPrefabFromLoadingOperation();
        }

        private void CacheWindowPrefabFromLoadingOperation()
        {
            GameObject result = loadingOperation.Value.Result;
            cachedWindowPrefab = result.GetComponent<Window>();
        }

        void IAsyncPrefabLoader.UnloadWindowPrefab()
        {
            windowInstance = null;
            cachedWindowPrefab = null;
            loadingOperation = null;
        }


        // public IEnumerator LoadWindowPrefab()
        // {
        //     AsyncOperationHandle<GameObject> task = windowPrefabAssetRef.InstantiateAsync();
        //     while (!task.IsDone)
        //         yield return null;
        //
        //     cachedWindowPrefab = task.Result.GetComponent<Window>();
        // }
    }
}
#endif
