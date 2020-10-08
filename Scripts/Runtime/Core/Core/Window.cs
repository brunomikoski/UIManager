using System.Collections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
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

        private CanvasGroup cachedCanvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (cachedCanvasGroup == null)
                    cachedCanvasGroup = GetComponent<CanvasGroup>();
                return cachedCanvasGroup;
            }
        }

        private WindowsManager windowsManager;
        private WindowID windowID;
        public WindowID WindowID => windowID;

        private bool isOpen;
        public bool IsOpen => isOpen;


        public void Initialize(WindowsManager windowsManager, WindowID windowID)
        {
            this.windowsManager = windowsManager;
            this.windowID = windowID;
            DispatchWindowInitialized();
        }

        private IEnumerator OpenEnumerator()
        {
            DispatchOnBeforeWindowOpen();
            if (windowID.InTransition != null)
                windowID.InTransition.BeforeTransition(this);
            
            gameObject.SetActive(true);

            if (windowID.InTransition != null)
                yield return windowID.InTransition.ExecuteEnumerator(this);
            DispatchOnAfterWindowOpen();
        }

        private IEnumerator CloseEnumerator()
        {
            DispatchOnBeforeWindowClose();

            if (windowID.OutTransition != null)
                windowID.OutTransition.BeforeTransition(this);
            
            if (windowID.OutTransition != null)
                yield return windowID.OutTransition.ExecuteEnumerator(this);
            
            gameObject.SetActive(false);
            DispatchOnAfterWindowClose();

        }

        public void Close()
        {
            if (!isOpen)
                return;
            
            isOpen = false;
            windowsManager.StartCoroutine(CloseEnumerator());
        }

        public void Open()
        {
            if (isOpen)
                return;
            
            isOpen = true;
            windowsManager.StartCoroutine(OpenEnumerator());
        }
    }
}
