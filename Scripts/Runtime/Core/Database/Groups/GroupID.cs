using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class GroupID : ScriptableObjectCollectionItem, ISOCColorizedItem
    {
        [SerializeField]
        private bool autoLoaded = true;
        public bool AutoLoaded => autoLoaded;

        [SerializeField]
        private Color color = Color.black;
        public Color LabelColor => color;
    }
}
