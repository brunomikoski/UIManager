using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
    public partial class Window : MonoBehaviour
    {
        [SerializeField]
        private bool cacheInterfacesInstance = true;

        [SerializeField] 
        private bool disableInteractionWhileTransitioning = true;

        [SerializeField]
        private WindowID windowID;
        public WindowID WindowID => windowID;

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

        
        private GraphicRaycaster cachedGraphicRaycaster;
        public GraphicRaycaster GraphicRaycaster
        {
            get
            {
                if (cachedGraphicRaycaster == null)
                    cachedGraphicRaycaster = this.GetOrAddComponent<GraphicRaycaster>();
                return cachedGraphicRaycaster;
            }
        }

        protected WindowsManager windowsManager;
        public WindowsManager WindowsManager => windowsManager;

        private bool isOpen;
        public bool IsOpen => isOpen;
        
        private bool initialized;
        public bool Initialized => initialized;

        private Coroutine closeRoutine;
        private Coroutine openRoutine;
        

        internal void Initialize(WindowsManager targetWindowsManager, WindowID targetWindowID)
        {
            windowsManager = targetWindowsManager;
            windowID = targetWindowID;
            initialized = true;
            DispatchWindowInitialized();
        }

        internal void Open(Action<Window> callback = null)
        {
            if (isOpen)
                return;

            StopTransitionsCoroutines();
            
            isOpen = true;
            openRoutine = windowsManager.StartCoroutine(OpenEnumerator(callback));
        }
        
        private IEnumerator OpenEnumerator(Action<Window> callback)
        {
            if (disableInteractionWhileTransitioning)
                GraphicRaycaster.enabled = false;

            OnBeforeOpen();

            yield return TransiteInEnumerator();
            
            GraphicRaycaster.enabled = true;
            callback?.Invoke(this);
            OnAfterOpen();
        }

        protected virtual void OnBeforeOpen()
        {
            DispatchOnBeforeWindowOpen();
        }
        
        private void OnAfterOpen()
        {
            DispatchOnAfterWindowOpen();
        }
        
        internal void Close(Action<Window> callback = null)
        {
            if (!isOpen)
                return;
            
            StopTransitionsCoroutines();
            
            isOpen = false;
            closeRoutine = windowsManager.StartCoroutine(CloseEnumerator(callback));
        }

        private IEnumerator CloseEnumerator(Action<Window> callback)
        {
            if (disableInteractionWhileTransitioning)
                GraphicRaycaster.enabled = false;
            
            OnBeforeClose();

            yield return TransiteOutEnumerator();

            GraphicRaycaster.enabled = true;
            callback?.Invoke(this);
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

        public virtual void OnGainFocus()
        {
            DispatchOnGainFocus();
        }

        public void OnLostFocus()
        {
            DispatchOnLostFocus();
        }

        private void StopTransitionsCoroutines()
        {
            if (closeRoutine != null)
                StopCoroutine(closeRoutine);

            if (openRoutine != null)
                StopCoroutine(openRoutine);
            
            closeRoutine = null;
            openRoutine = null;
        }
        
        protected virtual void OnDestroy()
        {
            StopTransitionsCoroutines();
        }
    }
}
