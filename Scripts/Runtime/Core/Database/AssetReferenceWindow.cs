#if USE_ADDRESSABLES

using UnityEngine.AddressableAssets;

namespace BrunoMikoski.UIManager
{
    public sealed class AssetReferenceWindow : AssetReferenceT<Window>
    {
        public AssetReferenceWindow(string guid) : base(guid) { }
    }
}
#endif
