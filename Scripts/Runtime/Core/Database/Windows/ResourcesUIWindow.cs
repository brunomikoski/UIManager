using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public sealed class ResourcesUIWindow : UIWindow, IAsyncPrefabLoader
    {
        [SerializeField]
        private string resourcePath;
        public string ResourcePath => resourcePath;

        [NonSerialized]
        private WindowController cachedWindowControllerPrefab;

        public override WindowController GetWindowPrefab()
        {
            if (cachedWindowControllerPrefab == null)
                CachePrefab(Resources.Load<GameObject>(resourcePath));

            return cachedWindowControllerPrefab;
        }

        void IAsyncPrefabLoader.LoadPrefab(Action loadedCallback = null)
        {
            if (cachedWindowControllerPrefab == null)
                CachePrefab(Resources.Load<GameObject>(resourcePath));

            loadedCallback?.Invoke();
        }

        UniTask IAsyncPrefabLoader.LoadPrefabAsync(CancellationToken cancellationToken, Action loadedCallback)
        {
            if (cachedWindowControllerPrefab != null)
            {
                loadedCallback?.Invoke();
                return UniTask.CompletedTask;
            }

            return LoadPrefabAsyncInternal(cancellationToken, loadedCallback);
        }

        private async UniTask LoadPrefabAsyncInternal(CancellationToken cancellationToken, Action loadedCallback)
        {
            ResourceRequest request = Resources.LoadAsync<GameObject>(resourcePath);
            await request.ToUniTask(cancellationToken: cancellationToken);

            CachePrefab(request.asset as GameObject);
            loadedCallback?.Invoke();
        }

        private void CachePrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError($"{name} could not load a prefab from Resources/{resourcePath}");
                return;
            }

            cachedWindowControllerPrefab = prefab.GetComponent<WindowController>();
            if (cachedWindowControllerPrefab == null)
                Debug.LogError($"{name} loaded Resources/{resourcePath} but it has no {nameof(WindowController)} on it");
        }

        // Dropping the reference is all this can do - the asset itself is only freed by the next
        // Resources.UnloadUnusedAssets, which the application transitions already run
        void IAsyncPrefabLoader.UnloadPrefab()
        {
            ClearWindowInstance();
            cachedWindowControllerPrefab = null;
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
