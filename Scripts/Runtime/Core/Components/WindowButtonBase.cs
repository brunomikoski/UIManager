using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    public abstract class WindowButtonBase : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        
        private Window cachedParentWindow;

        protected Window ParentWindow
        {
            get
            {
                if (cachedParentWindow == null)
                    cachedParentWindow = GetComponentInParent<Window>();
                return cachedParentWindow;
            }
        }

        protected WindowsManager WindowsManager => ParentWindow.WindowsManager;

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }

        protected abstract void OnClick();
    }
}
