using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
    public partial class WindowController : MonoBehaviour
    {
        [SerializeField]
        private bool cacheInterfacesInstance = true;

        [SerializeField] 
        private bool disableInteractionWhileTransitioning = true;

        [FormerlySerializedAs("windowIndirectRef")]
        [SerializeField]
        private UIWindowIndirectReference window;
        public UIWindow UIWindow => window.Ref;

        private bool hasCachedRectTransform;
        private RectTransform cachedRectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (!hasCachedRectTransform)
                {
                    cachedRectTransform = transform as RectTransform;
                    hasCachedRectTransform = true;
                }
                return cachedRectTransform;
            }
        }

        private bool hasCachedCanvasGroup;
        private CanvasGroup cachedCanvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!hasCachedCanvasGroup)
                {
                    cachedCanvasGroup = GetComponent<CanvasGroup>();
                    hasCachedCanvasGroup = cachedCanvasGroup != null;
                }
                return cachedCanvasGroup;
            }
        }

        
        private bool hasCachedGraphicRaycaster;
        private GraphicRaycaster cachedGraphicRaycaster;
        public GraphicRaycaster GraphicRaycaster
        {
            get
            {
                if (!hasCachedGraphicRaycaster)
                {
                    cachedGraphicRaycaster = GetComponent<GraphicRaycaster>();
                    hasCachedGraphicRaycaster = cachedGraphicRaycaster != null;
                }
                return cachedGraphicRaycaster;
            }
        }

        protected WindowsManager windowsManager;
        public WindowsManager WindowsManager => windowsManager;

        private bool isOpen;
        public bool IsOpen => isOpen;


        private bool initialized;
        internal bool Initialized => initialized;

        private Coroutine activeCoroutine;


        internal void Initialize(WindowsManager targetWindowsManager, UIWindow targetUIWindow)
        {
            windowsManager = targetWindowsManager;
            window = new UIWindowIndirectReference(targetUIWindow);
            initialized = true;
            DispatchWindowInitialized();
        }

        internal IEnumerator OpenEnumerator(Action<WindowController> callback = null)
        {
            if (isOpen)
                yield break;
            
            isOpen = true;

            OnBeforeOpen();
            
            bool wasGraphicRaycasterEnabled = false;
            if (hasCachedGraphicRaycaster)
            {
                wasGraphicRaycasterEnabled = GraphicRaycaster.enabled;
            }
            
            if (disableInteractionWhileTransitioning)
            {
                if (hasCachedGraphicRaycaster)
                    GraphicRaycaster.enabled = false;
            }

            yield return TransiteInEnumerator();
            
            if (hasCachedGraphicRaycaster)
                GraphicRaycaster.enabled = wasGraphicRaycasterEnabled;

            
            callback?.Invoke(this);
            OnAfterOpen();
        }

        protected virtual void OnBeforeOpen()
        {
            DispatchOnBeforeWindowOpen();
        }
        
        protected virtual void OnAfterOpen()
        {
            DispatchOnAfterWindowOpen();
        }

        public void Back()
        {
            windowsManager.Back();
        }
        
        public void Close()
        {
            windowsManager.Close(UIWindow);
        }

        public void CloseLast()
        {
            windowsManager.CloseLastOpenWindow();
        }

        public void Open()
        {
            windowsManager.Open(UIWindow);
        }

        internal IEnumerator CloseEnumerator()
        {
            if (!isOpen)
                yield break;
            
            isOpen = false;

            if (disableInteractionWhileTransitioning)
            {
                if (hasCachedGraphicRaycaster)
                    GraphicRaycaster.enabled = false;
            }
            
            OnBeforeClose();

            yield return TransiteOutEnumerator();


            OnAfterClose();
        }

        protected virtual void OnBeforeClose()
        {
            DispatchOnBeforeWindowClose();
        }
        
        protected virtual void OnAfterClose()
        {
            DispatchOnAfterWindowClose();
        }
        
        protected virtual IEnumerator TransiteInEnumerator()
        {
            gameObject.SetActive(true);
            yield return null;
        }
        
        protected virtual IEnumerator TransiteOutEnumerator()
        {
            gameObject.SetActive(false);
            yield return null;
        }

        internal virtual void OnGainFocus()
        {
            DispatchOnGainFocus();
        }

        internal virtual void OnLostFocus()
        {
            DispatchOnLostFocus();
        }
       
        protected virtual void OnDestroy()
        {
            StopActiveTransitionCoroutine();
        }

        private void StopActiveTransitionCoroutine()
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }
        }

        internal void SetCurrentActiveTransitionCoroutine(Coroutine coroutine)
        {
            StopActiveTransitionCoroutine();
            activeCoroutine = coroutine;
        }

        internal void ClearCurrentActiveTransitionCoroutine()
        {
            StopActiveTransitionCoroutine();
            activeCoroutine = null;
        }

        public bool HasActiveTransitionCoroutine()
        {
            return activeCoroutine != null;
        }
    }
}
