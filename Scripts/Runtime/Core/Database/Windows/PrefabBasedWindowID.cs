using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public sealed class PrefabBasedWindowID : WindowID
    {
        [Header("References")]
        [SerializeField]
        private Window windowPrefab;
       
        public override async UniTask InstantiateAsync()
        {
            if (windowPrefab == null)
            {
                Debug.LogError($"Window Prefab on {this} is null", this);
            }
            else
            {
                windowInstance = Instantiate(windowPrefab);
                windowInstance.name = $"{windowPrefab.name} [{this.name}]";
            }
        }

        public override async UniTask DestroyAsync()
        {
            if (windowInstance == null)
                return;

            Destroy(windowInstance.gameObject);
        }
   }
}
