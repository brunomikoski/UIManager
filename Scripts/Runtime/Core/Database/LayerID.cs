using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class LayerID : CollectableScriptableObject
    {
        [SerializeField]
        private LayerBehaviour behaviour = LayerBehaviour.Exclusive;
        public LayerBehaviour Behaviour => behaviour;
    }
}
