using System;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    [Serializable]
    public sealed class UIWindowIndirectReference : CollectionItemIndirectReference<UIWindow>
    {
        public UIWindowIndirectReference(UIWindow item) : base(item)
        {
        }
    }
}
