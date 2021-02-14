using System.Collections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public sealed class PrefabBasedWindowID : WindowID
    {
        [Header("References")]
        [SerializeField]
        private Window windowPrefab;

        public override IEnumerator InstantiateEnumerator(WindowsManager windowsManager)
        {
            windowPrefab.gameObject.SetActive(false);
            windowInstance = Instantiate(windowPrefab);
            windowInstance.name = $"{windowPrefab.name} [{this.name}]";
            yield break;
        }

        public override IEnumerator DestroyEnumerator()
        {
            Destroy(windowInstance.gameObject);
            windowInstance = null;
            yield break;
        }
    }
}
