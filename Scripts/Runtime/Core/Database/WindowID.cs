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
    }
}
