using System;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    [Serializable]
    public sealed class WindowIDIndirectReference : CollectableIndirectReference<WindowID>
    {
#if UNITY_EDITOR
        [SerializeField]
        private WindowID editorAsset;
#endif
    }
}
