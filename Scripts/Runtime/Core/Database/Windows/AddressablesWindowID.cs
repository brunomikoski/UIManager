#if USE_ADDRESSABLES
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BrunoMikoski.UIManager
{
    public sealed class AddressablesWindowID : WindowID, IAsyncPrefabLoader
    {
        [SerializeField] 
        private AssetReferenceGameObject windowPrefabAssetRef;
        public AssetReferenceGameObject WindowPrefabAssetRef => windowPrefabAssetRef;

        [NonSerialized]
        private Window cachedWindowPrefab;

        private AsyncOperationHandle<GameObject>? loadingOperation;
        
        
        public override Window GetWindowPrefab()
        {
            if (cachedWindowPrefab == null)
            {
                Debug.LogWarning(
                    $"{name} implements IAsyncPrefabLoader but it's not loaded yet, please make sure to load the group {Group} before trying to get the prefab");
                GetOrCreateLoadingOperation();
                loadingOperation.Value.WaitForCompletion();
                CacheWindowPrefabFromLoadingOperation();
            }
            return cachedWindowPrefab;
        }

        private void GetOrCreateLoadingOperation()
        {
            if (loadingOperation != null)
                return;

            if (!windowPrefabAssetRef.RuntimeKeyIsValid())
            {
                throw new InvalidKeyException(
                    $"The key {windowPrefabAssetRef.RuntimeKey} is not valid, please make sure to set the correct key on the AddressableWindowID {name}");
            }
            
            loadingOperation = windowPrefabAssetRef.LoadAssetAsync<GameObject>();
        }

        void IAsyncPrefabLoader.LoadPrefab()
        {
            if (cachedWindowPrefab != null)
                return;
            
            GetOrCreateLoadingOperation();

            void OnLoadingComplete(AsyncOperationHandle<GameObject> asyncOperationHandle)
            {
                loadingOperation.Value.Completed -= OnLoadingComplete;
                CacheWindowPrefabFromLoadingOperation();
            }

            loadingOperation.Value.Completed += OnLoadingComplete;
        }

        private void CacheWindowPrefabFromLoadingOperation()
        {
            GameObject result = loadingOperation.Value.Result;
            cachedWindowPrefab = result.GetComponent<Window>();
        }

        void IAsyncPrefabLoader.UnloadPrefab()
        {
            windowInstance = null;
            cachedWindowPrefab = null;
            if (loadingOperation.HasValue)
                Addressables.Release(loadingOperation.Value);
            loadingOperation = null;
        }

        public bool IsLoaded()
        {
            return cachedWindowPrefab != null;
        }
    }
}
#endif
