using System;
using System.Collections.Generic;

namespace BrunoMikoski.UIManager
{
    public partial class WindowsManager
    {
        public enum WindowEvent
        {
            OnOpen,
            OnClose,
            OnEnterBackground,
            OnBecomeFocused
        }
        
        
        private Dictionary<WindowID, Dictionary<WindowEvent, List<Action>>> windowToEventToCallbackList = new Dictionary<WindowID, Dictionary<WindowEvent, List<Action>>>();

        private void UnsubscribeToWindowEvent(WindowEvent targetEvent, WindowID windowID, Action callback)
        {
            if (!windowToEventToCallbackList.ContainsKey(windowID))
                return;

            if (!windowToEventToCallbackList[windowID].ContainsKey(targetEvent))
                return;

            windowToEventToCallbackList[windowID][targetEvent].Remove(callback);
        }

        private void SubscribeToWindowEvent(WindowEvent targetEvent, WindowID windowID, Action callback)
        {
            if (!windowToEventToCallbackList.ContainsKey(windowID))
                windowToEventToCallbackList.Add(windowID, new Dictionary<WindowEvent, List<Action>>());

            if (!windowToEventToCallbackList[windowID].ContainsKey(targetEvent))
                windowToEventToCallbackList[windowID].Add(targetEvent, new List<Action>());

            if (windowToEventToCallbackList[windowID][targetEvent].Contains(callback))
                return;

            windowToEventToCallbackList[windowID][targetEvent].Add(callback);
        }

        private void DispatchWindowEvent(WindowEvent targetEvent, WindowID windowID)
        {
            if (!windowToEventToCallbackList.ContainsKey(windowID))
                return;

            if (!windowToEventToCallbackList[windowID].ContainsKey(targetEvent))
                return;

            for (int i = 0; i < windowToEventToCallbackList[windowID][targetEvent].Count; i++)
            {
                windowToEventToCallbackList[windowID][targetEvent][i].Invoke();
            }
        }
    }
}