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
            gameObject.SetActive(true);

            yield return ExecuteTransitionEnumerator(TransitionType.TransitionIn);
            DispatchOnAfterWindowOpen();
        }
       
        private IEnumerator CloseEnumerator()
        {
            DispatchOnBeforeWindowClose();

            yield return ExecuteTransitionEnumerator(TransitionType.TransitionOut);
            gameObject.SetActive(false);
            DispatchOnAfterWindowClose();
        }
        
        private IEnumerator ExecuteTransitionEnumerator(TransitionType transitionType)
        {
            if(!windowID.TryGetTransition(transitionType, out AnimatedTransition animatedTransition, out bool playBackwards))
                yield break;

            animatedTransition.BeforeTransition(this);
            yield return animatedTransition.ExecuteEnumerator(this, transitionType, playBackwards);
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

        public virtual void OnGainFocus()
        {
            DispatchOnGainFocus();
        }

        public void OnLostFocus()
        {
            DispatchOnLostFocus();
        }
    }
}
