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
        
         
        public event Action OnWillBeLoadedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.OnWillBeLoaded, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.OnWillBeLoaded, this, value);
        }
        
        
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

        void IAsyncPrefabLoader.LoadPrefab(Action callback)
        {
            if (cachedWindowPrefab != null)
                return;
            
            GetOrCreateLoadingOperation();

            void LoadingComplete(AsyncOperationHandle<GameObject> asyncOperationHandle)
            {
                loadingOperation.Value.Completed -= LoadingComplete;
                callback?.Invoke();
                CacheWindowPrefabFromLoadingOperation();
            }

            loadingOperation.Value.Completed += LoadingComplete;
        }

        private void CacheWindowPrefabFromLoadingOperation()
        {
            GameObject result = loadingOperation.Value.Result;
            cachedWindowPrefab = result.GetComponent<Window>();
        }

        void IAsyncPrefabLoader.UnloadPrefab()
        {
            ClearWindowInstance();
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
