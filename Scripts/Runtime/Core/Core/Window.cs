using System.Collections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
    public partial class Window : MonoBehaviour
    {
        [SerializeField]
        private bool cacheInterfacesInstance = true;
        
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
        public WindowsManager WindowsManager => windowsManager;

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
            {
                if (windowID.InTransition is AnimatedTransition animatedTransition)
                    animatedTransition.BeforeTransition(this);
            }
            
            gameObject.SetActive(true);

            if (windowID.InTransition != null)
            {
                if (windowID.InTransition is AnimatedTransition animatedTransition)
                {
                    yield return animatedTransition.ExecuteEnumerator(this, false);
                }
            }
            DispatchOnAfterWindowOpen();
        }

        private IEnumerator CloseEnumerator()
        {
            DispatchOnBeforeWindowClose();

            if (windowID.OutTransition != null)
            {
                if (windowID.OutTransition is AnimatedTransition animatedTransition)
                    animatedTransition.BeforeTransition(this);
                else if (windowID.OutTransition is ReverseInTransition)
                {
                    if (windowID.InTransition is AnimatedTransition InAnimatedTransition)
                    {
                        InAnimatedTransition.BeforeTransition(this);
                    }    
                }
            }

            if (windowID.OutTransition != null)
            {
                if (windowID.OutTransition is AnimatedTransition animatedTransition)
                {
                    yield return animatedTransition.ExecuteEnumerator(this, false);
                }
                else if (windowID.OutTransition is ReverseInTransition reverseInTransition)
                {
                    if (windowID.InTransition is AnimatedTransition InAnimatedTransition)
                    {
                        yield return InAnimatedTransition.ExecuteEnumerator(this, true);
                    }
                }
            }
            
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

        public void OnSentToBackground()
        {
            
        }

        public void OnWindowFocused()
        {
            
        }
    }
}
