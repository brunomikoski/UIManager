using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public sealed class PrefabBasedWindowID : WindowID
    {
        [SerializeField]
        private Window windowPrefab;
        public Window WindowPrefab => windowPrefab;

        [SerializeField]
        private bool instantiateOnInitialization = true;
        public bool InstantiateOnInitialization => instantiateOnInitialization;

    }
}
