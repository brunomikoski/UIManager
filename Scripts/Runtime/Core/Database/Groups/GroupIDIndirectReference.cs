using System;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    [Serializable]
    public sealed class GroupIDIndirectReference : CollectableIndirectReference<GroupID>
    {
#if UNITY_EDITOR
        [SerializeField]
        private GroupID editorAsset;
#endif
    }
}
