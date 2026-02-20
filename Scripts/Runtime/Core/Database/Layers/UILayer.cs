using System;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class UILayer : ScriptableObjectCollectionItem
    {
        [SerializeField]
        private UILayerBehaviour behaviour = UILayerBehaviour.Exclusive;
        public UILayerBehaviour Behaviour => behaviour;

        [SerializeField]
        private bool includedOnHistory = true;
        public bool IncludedOnHistory => includedOnHistory;

        protected WindowsManager WindowsManager;

        public event Action OnLostFocusEvent
        {
            add => WindowsManager.SubscribeToLayerEvent(LayerEvent.LayerLostFocus, this, value);
            remove => WindowsManager.UnsubscribeToLayerEvent(LayerEvent.LayerLostFocus, this, value);
        }

        public event Action OnGainFocusEvent
        {
            add => WindowsManager.SubscribeToLayerEvent(LayerEvent.LayerGainedFocus, this, value);
            remove => WindowsManager.UnsubscribeToLayerEvent(LayerEvent.LayerGainedFocus, this, value);
        }

        public void Initialize(WindowsManager windowsManager)
        {
            WindowsManager = windowsManager;
        }

        public void SetIncludedInHistory(bool shouldIncludeInHistory)
        {
            includedOnHistory = shouldIncludeInHistory;
        }

        public bool HasAnyWindowOpen()
        {
            return WindowsManager.TryGetOpenWindowsOfLayer(this, out _);
        }
    }
}
