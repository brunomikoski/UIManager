using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public abstract class PrefabUIWindow<T> : UIWindow where T : WindowController
    {
        [SerializeField]
        private T windowControllerPrefab;

        public new T WindowInstance => (T)base.WindowInstance;

        public override WindowController GetWindowPrefab()
        {
            return windowControllerPrefab;
        }
    }

    public class PrefabUIWindow : PrefabUIWindow<WindowController>
    {
    }
}
