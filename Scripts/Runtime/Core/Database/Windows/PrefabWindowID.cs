using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class PrefabWindowID : WindowID
    {
        [SerializeField]
        private Window windowPrefab;
        public Window WindowPrefab => windowPrefab;

        public override Window GetWindowPrefab()
        {
            return windowPrefab;
        }
    }
}
