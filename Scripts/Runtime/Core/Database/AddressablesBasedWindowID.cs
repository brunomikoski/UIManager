#if USE_ADDRESSABLES
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BrunoMikoski.UIManager
{
    public sealed class AddressablesBasedWindowID : WindowID
    {
        [SerializeField] 
        private AssetReferenceGameObject windowPrefab;
        
        public override IEnumerator InstantiateEnumerator(WindowsManager windowsManager)
        {
            AsyncOperationHandle<GameObject> task = windowPrefab.InstantiateAsync();
            while (!task.IsDone)
                yield return null;
            
            windowInstance = task.Result.GetComponent<Window>();
        }

        public override IEnumerator DestroyEnumerator()
        {
            windowPrefab.ReleaseInstance(windowInstance.gameObject);
            windowPrefab.ReleaseAsset();
            windowInstance = null;
            yield break;
        }
    }
}
#endif
