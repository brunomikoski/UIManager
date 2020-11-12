using System;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    [Serializable]
    public sealed class LayerIDIndirectReference : CollectableIndirectReference<LayerID>
    {
#if UNITY_EDITOR
        [SerializeField]
        private LayerID editorAsset;
#endif
    }
}
