#if USE_ADDRESSABLES
using System;
using UnityEngine.AddressableAssets;

namespace BrunoMikoski.UIManager
{
    [Serializable]
    public class AssetReferenceWindow : AssetReferenceT<Window>
    {
        public AssetReferenceWindow(string guid) : base(guid) { }
    }
}
#endif
