using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class LayerID : CollectableScriptableObject
    {
        [SerializeField]
        private LayerBehaviour behaviour = LayerBehaviour.Exclusive;
        public LayerBehaviour Behaviour => behaviour;

        [SerializeField]
        private bool includedOnHistory = true;
        public bool IncludedOnHistory => includedOnHistory;

        public void SetIncludedInHistory(bool shouldIncludeInHistory)
        {
            includedOnHistory = shouldIncludeInHistory;
        }
    }
}
