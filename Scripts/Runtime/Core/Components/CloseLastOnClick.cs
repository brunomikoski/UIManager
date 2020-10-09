using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    public class CloseLastOnClick : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        
        private Window cachedParentWindow;
        private Window ParentWindow
        {
            get
            {
                if (cachedParentWindow == null)
                    cachedParentWindow = GetComponentInParent<Window>();
                return cachedParentWindow;
            }
        }
        
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

        private void OnClick()
        {
            ParentWindow.WindowsManager.CloseLast();
        }

    }
}
