#if USE_ADDRESSABLES
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BrunoMikoski.UIManager
{
    public sealed class AddressablesUIWindow : UIWindow, IAsyncPrefabLoader
    {
        [SerializeField] 
        private AssetReferenceGameObject windowPrefabAssetRef;
        public AssetReferenceGameObject WindowPrefabAssetRef => windowPrefabAssetRef;

        [NonSerialized]
        private WindowController cachedWindowControllerPrefab;

        private AsyncOperationHandle<GameObject>? loadingOperation;
        
         
        public event Action OnWillBeLoadedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.BeforeWindowLoad, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.BeforeWindowLoad, this, value);
        }
        
        
        public override WindowController GetWindowPrefab()
        {
            if (cachedWindowControllerPrefab == null)
            {
                Debug.LogWarning(
                    $"{name} implements IAsyncPrefabLoader but it's not loaded yet, please make sure to load the group {Group} before trying to get the prefab");
                GetOrCreateLoadingOperation();
                loadingOperation.Value.WaitForCompletion();
                CacheWindowPrefabFromLoadingOperation();
            }
            return cachedWindowControllerPrefab;
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
            if (cachedWindowControllerPrefab != null)
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

        UniTask IAsyncPrefabLoader.LoadPrefabAsync(CancellationToken cancellationToken, Action callback)
        {
            if (cachedWindowControllerPrefab != null)
            {
                callback?.Invoke();
                return UniTask.CompletedTask;
            }

            GetOrCreateLoadingOperation();
            // Wait until Addressables operation completes; cannot cancel underlying load but we can stop awaiting.
            return AwaitHandle(cancellationToken, callback);
        }

        private async UniTask AwaitHandle(CancellationToken cancellationToken, Action callback)
        {
            AsyncOperationHandle<GameObject> handle = loadingOperation.Value;
            while (!handle.IsDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Yield(cancellationToken);
            }
            CacheWindowPrefabFromLoadingOperation();
            callback?.Invoke();
        }

        private void CacheWindowPrefabFromLoadingOperation()
        {
            GameObject result = loadingOperation.Value.Result;
            cachedWindowControllerPrefab = result.GetComponent<WindowController>();
        }

        void IAsyncPrefabLoader.UnloadPrefab()
        {
            ClearWindowInstance();
            cachedWindowControllerPrefab = null;
            if (loadingOperation.HasValue)
                Addressables.Release(loadingOperation.Value);
            loadingOperation = null;
        }

        UniTask IAsyncPrefabLoader.UnloadPrefabAsync(CancellationToken cancellationToken)
        {
            ((IAsyncPrefabLoader)this).UnloadPrefab();
            return UniTask.CompletedTask;
        }

        public bool IsLoaded()
        {
            return cachedWindowControllerPrefab != null;
        }
    }
}
#endif
