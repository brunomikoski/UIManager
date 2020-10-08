using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class WindowID : CollectableScriptableObject
    {
        [SerializeField]
        private LayerID layerID;
        public LayerID LayerID => layerID;

        [SerializeField]
        private GroupID groupID;
        public GroupID GroupID => groupID;

        [Header("Transitions")] 
        [SerializeField]
        private TransitionBase inTransition;
        public TransitionBase InTransition => inTransition;

        [SerializeField] 
        private TransitionBase outTransition;
        public TransitionBase OutTransition => outTransition;


        private WindowsManager windowsManager;
        
        public void SetWindowsManager(WindowsManager windowsManager)
        {
            this.windowsManager = windowsManager;
        }

        public void Open()
        {
            this.windowsManager.Open(this);
        }
    }
}
