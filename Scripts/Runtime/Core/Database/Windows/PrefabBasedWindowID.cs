using System;
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
            if (windowPrefab == null)
            {
                Debug.LogError($"Window Prefab on {this} is null", this);
                yield break;
            }
            windowInstance = Instantiate(windowPrefab);
            windowInstance.name = $"{windowPrefab.name} [{this.name}]";
        }

        public override IEnumerator DestroyEnumerator()
        {
            Destroy(windowInstance.gameObject);
            windowInstance = null;
            yield break;
        }
    }
}
