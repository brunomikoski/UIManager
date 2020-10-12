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
            windowInstance = Instantiate(windowPrefab);
            yield break;
        }
    }
}
