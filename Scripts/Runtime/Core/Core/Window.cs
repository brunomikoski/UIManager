using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public partial class Window : MonoBehaviour
    {
        private RectTransform cachedRectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (cachedRectTransform == null)
                    cachedRectTransform = GetComponent<RectTransform>();
                return cachedRectTransform;
            }
        }
        
        private WindowManager windowManager;
        private WindowID windowID;


        public void Initialize(WindowManager windowManager, WindowID windowID)
        {
            this.windowManager = windowManager;
            this.windowID = windowID;
            DispatchWindowInitialized();
        }
        
        private void DispatchWindowInitialized()
        {
            IWindowInitialized[] listeners = gameObject.GetComponentsInChildren<IWindowInitialized>(true);
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].OnWindowInitialized();
            }
        }
    }
}
