using System;
using System.Collections.Generic;

namespace BrunoMikoski.UIManager
{
    public partial class WindowsManager
    {
        private struct TransitionEventData
        {
            public UIWindow FromUIWindow;
            public UIWindow ToUIWindow;
            public Action Callback;

            public TransitionEventData(UIWindow fromUIWindow, UIWindow toUIWindow, Action callback)
            {
                FromUIWindow = fromUIWindow;
                ToUIWindow = toUIWindow;
                Callback = callback;
            }
        }


        private Dictionary<WindowEvent, List<Action<UIWindow>>> windowEventToAnyWindowCallbackList = new();
        private Dictionary<UIWindow, Dictionary<WindowEvent, List<Action>>> windowToEventToCallbackList = new();
        private List<TransitionEventData> transationEvents = new();
        
        
        public void SubscribeToTransitionEvent(UIWindow fromUIWindow, UIWindow toUIWindow, Action callback)
        {
            if (TryGetTransitionEventData(fromUIWindow, toUIWindow, callback, out _))
                return;
            
            transationEvents.Add(new TransitionEventData(fromUIWindow, toUIWindow, callback));
        }
        
        public void UnsubscribeToTransitionEvent(UIWindow fromUIWindow, UIWindow toUIWindow, Action callback)
        {
            if (!TryGetTransitionEventData(fromUIWindow, toUIWindow, callback, out TransitionEventData result))
                return;

            transationEvents.Remove(result);
        }
        
        public void SubscribeToAnyWindowEvent(WindowEvent targetEvent, Action<UIWindow> callback)
        {
            if (!windowEventToAnyWindowCallbackList.ContainsKey(targetEvent))
                windowEventToAnyWindowCallbackList.Add(targetEvent, new List<Action<UIWindow>>());

            if (windowEventToAnyWindowCallbackList[targetEvent].Contains(callback))
                return;

            windowEventToAnyWindowCallbackList[targetEvent].Add(callback);
        }
        
        public void UnsubscribeToAnyWindowEvent(WindowEvent targetEvent, Action<UIWindow> callback)
        {
            if (!windowEventToAnyWindowCallbackList.ContainsKey(targetEvent))
                return;

            if (!windowEventToAnyWindowCallbackList[targetEvent].Contains(callback))
                return;

            windowEventToAnyWindowCallbackList[targetEvent].Remove(callback);
        }
        
        public void UnsubscribeToWindowEvent(WindowEvent targetEvent, UIWindow uiWindow, Action callback)
        {
            if (!windowToEventToCallbackList.ContainsKey(uiWindow))
                return;

            if (!windowToEventToCallbackList[uiWindow].ContainsKey(targetEvent))
                return;

            windowToEventToCallbackList[uiWindow][targetEvent].Remove(callback);
        }
        
        public void SubscribeToWindowEvent(WindowEvent targetEvent, UIWindow uiWindow, Action callback)
        {
            if (!windowToEventToCallbackList.ContainsKey(uiWindow))
                windowToEventToCallbackList.Add(uiWindow, new Dictionary<WindowEvent, List<Action>>());

            if (!windowToEventToCallbackList[uiWindow].ContainsKey(targetEvent))
                windowToEventToCallbackList[uiWindow].Add(targetEvent, new List<Action>());

            if (windowToEventToCallbackList[uiWindow][targetEvent].Contains(callback))
                return;

            windowToEventToCallbackList[uiWindow][targetEvent].Add(callback);
        }
        
         
        private void DispatchTransition(List<WindowController> fromWindows, WindowController toWindowController)
        {
            for (int i = 0; i < fromWindows.Count; i++)
            {
                WindowController fromWindowController = fromWindows[i];
                for (int j = 0; j < transationEvents.Count; j++)
                {
                    TransitionEventData transition = transationEvents[j];
                    if (transition.FromUIWindow == fromWindowController.UIWindow
                        && transition.ToUIWindow == toWindowController.UIWindow)
                    {
                        transition.Callback.Invoke();
                    }
                }
            }
        }

        private bool TryGetTransitionEventData(UIWindow fromUIWindow, UIWindow toUIWindow, Action callback, out TransitionEventData result)
        {
            for (int i = 0; i < transationEvents.Count; i++)
            {
                TransitionEventData transitionEventData = transationEvents[i];
                if (transitionEventData.FromUIWindow == fromUIWindow
                    && transitionEventData.ToUIWindow == toUIWindow
                    && transitionEventData.Callback == callback)
                {
                    result = transitionEventData;
                    return true;
                }
            }

            result = default;
            return false;
        }

        private void DispatchWindowEvent(WindowEvent targetEvent, WindowController windowController)
        {
            DispatchWindowEvent(targetEvent, windowController.UIWindow);
        }
        
        private void DispatchWindowEvent(WindowEvent targetEvent, UIWindow uiWindow)
        {
            if (windowToEventToCallbackList.ContainsKey(uiWindow))
            {
                if (windowToEventToCallbackList[uiWindow].ContainsKey(targetEvent))
                {
                    for (int i = 0; i < windowToEventToCallbackList[uiWindow][targetEvent].Count; i++)
                    {
                        windowToEventToCallbackList[uiWindow][targetEvent][i].Invoke();
                    }
                }
            }

            if (uiWindow.HasWindowInstance)
            {
                if (windowEventToAnyWindowCallbackList.ContainsKey(targetEvent))
                {
                    for (int i = 0; i < windowEventToAnyWindowCallbackList[targetEvent].Count; i++)
                    {
                        windowEventToAnyWindowCallbackList[targetEvent][i].Invoke(uiWindow);
                    }
                }
            }
        }

    }

    public enum WindowEvent
    {
        WindowInitialized,
        BeforeWindowOpen,
        WindowOpened,
        BeforeWindowClose,
        WindowClosed,
        WindowLostFocus,
        WindowGainedFocus,
        BeforeWindowLoad,
        WindowLoaded,
        BeforeWindowDestroy,
        WindowDestroyed
    }
}
