using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class UILayer : ScriptableObjectCollectionItem
    {
        [SerializeField]
        private UILayerBehaviour behaviour = UILayerBehaviour.Exclusive;
        public UILayerBehaviour Behaviour => behaviour;

        [SerializeField]
        private bool includedOnHistory = true;
        public bool IncludedOnHistory => includedOnHistory;

        public void SetIncludedInHistory(bool shouldIncludeInHistory)
        {
            includedOnHistory = shouldIncludeInHistory;
        }
    }
}
