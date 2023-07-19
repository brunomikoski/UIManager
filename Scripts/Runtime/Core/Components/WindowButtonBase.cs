using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    public abstract class WindowButtonBase : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        
        private WindowController cachedParentWindowController;

        protected WindowController ParentWindowController
        {
            get
            {
                if (cachedParentWindowController == null)
                    cachedParentWindowController = GetComponentInParent<WindowController>();
                return cachedParentWindowController;
            }
        }

        protected WindowsManager WindowsManager => ParentWindowController.WindowsManager;

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
