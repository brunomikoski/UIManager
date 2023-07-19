using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class PrefabUIWindow : UIWindow
    {
        [SerializeField]
        private WindowController windowControllerPrefab;

        public override WindowController GetWindowPrefab()
        {
            return windowControllerPrefab;
        }
    }
}
