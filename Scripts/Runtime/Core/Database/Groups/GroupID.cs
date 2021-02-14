using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class GroupID : CollectableScriptableObject
    {
        [SerializeField]
        private bool autoLoaded = true;
        public bool AutoLoaded => autoLoaded;

    }
}
