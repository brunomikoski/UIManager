using System;
using System.Collections.Generic;

namespace BrunoMikoski.UIManager
{
    public partial class WindowsManager
    {
        private struct TransitionEventData
        {
            public WindowID FromWindowID;
            public WindowID ToWindowID;
            public Action Callback;

            public TransitionEventData(WindowID fromWindowID, WindowID toWindowID, Action callback)
            {
                FromWindowID = fromWindowID;
                ToWindowID = toWindowID;
                Callback = callback;
            }
        }


        private Dictionary<WindowEvent, List<Action<Window>>> windowEventToAnyWindowCallbackList = new Dictionary<WindowEvent,List<Action<Window>>>();
        private Dictionary<WindowID, Dictionary<WindowEvent, List<Action>>> windowToEventToCallbackList = new Dictionary<WindowID, Dictionary<WindowEvent, List<Action>>>();
        private List<TransitionEventData> transationEvents = new List<TransitionEventData>();
        
        
        public void SubscribeToTransitionEvent(WindowID fromWindowID, WindowID toWindowID, Action callback)
        {
            if (TryGetTransitionEventData(fromWindowID, toWindowID, callback, out _))
                return;
            
            transationEvents.Add(new TransitionEventData(fromWindowID, toWindowID, callback));
        }
        
        public void UnsubscribeToTransitionEvent(WindowID fromWindowID, WindowID toWindowID, Action callback)
        {
            if (!TryGetTransitionEventData(fromWindowID, toWindowID, callback, out TransitionEventData result))
                return;

            transationEvents.Remove(result);
        }
        
        public void SubscribeToAnyWindowEvent(WindowEvent targetEvent, Action<Window> callback)
        {
            if (!windowEventToAnyWindowCallbackList.ContainsKey(targetEvent))
                windowEventToAnyWindowCallbackList.Add(targetEvent, new List<Action<Window>>());

            if (windowEventToAnyWindowCallbackList[targetEvent].Contains(callback))
                return;

            windowEventToAnyWindowCallbackList[targetEvent].Add(callback);
        }
        
        public void UnsubscribeToAnyWindowEvent(WindowEvent targetEvent, Action<Window> callback)
        {
            if (!windowEventToAnyWindowCallbackList.ContainsKey(targetEvent))
                return;

            if (!windowEventToAnyWindowCallbackList[targetEvent].Contains(callback))
                return;

            windowEventToAnyWindowCallbackList[targetEvent].Remove(callback);
        }
        
        public void UnsubscribeToWindowEvent(WindowEvent targetEvent, WindowID windowID, Action callback)
        {
            if (!windowToEventToCallbackList.ContainsKey(windowID))
                return;

            if (!windowToEventToCallbackList[windowID].ContainsKey(targetEvent))
                return;

            windowToEventToCallbackList[windowID][targetEvent].Remove(callback);
        }

        
        public void SubscribeToWindowEvent(WindowEvent targetEvent, WindowID windowID, Action callback)
        {
            if (!windowToEventToCallbackList.ContainsKey(windowID))
                windowToEventToCallbackList.Add(windowID, new Dictionary<WindowEvent, List<Action>>());

            if (!windowToEventToCallbackList[windowID].ContainsKey(targetEvent))
                windowToEventToCallbackList[windowID].Add(targetEvent, new List<Action>());

            if (windowToEventToCallbackList[windowID][targetEvent].Contains(callback))
                return;

            windowToEventToCallbackList[windowID][targetEvent].Add(callback);
        }
        
         
        private void DispatchTransition(List<Window> fromWindows, Window toWindow)
        {
            for (int i = 0; i < fromWindows.Count; i++)
            {
                Window fromWindow = fromWindows[i];
                for (int j = 0; j < transationEvents.Count; j++)
                {
                    TransitionEventData transition = transationEvents[j];
                    if (transition.FromWindowID == fromWindow.WindowID
                        && transition.ToWindowID == toWindow.WindowID)
                    {
                        transition.Callback.Invoke();
                    }
                }
            }
        }

        private bool TryGetTransitionEventData(WindowID fromWindowID, WindowID toWindowID, Action callback, out TransitionEventData result)
        {
            for (int i = 0; i < transationEvents.Count; i++)
            {
                TransitionEventData transitionEventData = transationEvents[i];
                if (transitionEventData.FromWindowID == fromWindowID
                    && transitionEventData.ToWindowID == toWindowID
                    && transitionEventData.Callback == callback)
                {
                    result = transitionEventData;
                    return true;
                }
            }

            result = default;
            return false;
        }

        private void DispatchWindowEvent(WindowEvent targetEvent, Window window)
        {
            if (windowToEventToCallbackList.ContainsKey(window.WindowID))
            {
                if (windowToEventToCallbackList[window.WindowID].ContainsKey(targetEvent))
                {
                    for (int i = 0; i < windowToEventToCallbackList[window.WindowID][targetEvent].Count; i++)
                    {
                        windowToEventToCallbackList[window.WindowID][targetEvent][i].Invoke();
                    }
                }
            }

            if (windowEventToAnyWindowCallbackList.ContainsKey(targetEvent))
            {
                for (int i = 0; i < windowEventToAnyWindowCallbackList[targetEvent].Count; i++)
                {
                    windowEventToAnyWindowCallbackList[targetEvent][i].Invoke(window);
                }
            }
        }

    }

    public enum WindowEvent
    {
        OnOpen,
        OnClose,
        OnLostFocus,
        OnGainFocus
    }
}