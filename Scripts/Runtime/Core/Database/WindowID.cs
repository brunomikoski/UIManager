using System;
using System.Collections;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public abstract class WindowID : CollectableScriptableObject
    {
        [Serializable]
        private struct TransitionData
        {
            public TransitionType TransitionType;
            public TransitionBase Transition;
        }
        
        [SerializeField]
        private LayerID layerID;
        public LayerID LayerID => layerID;

        [SerializeField]
        private GroupID groupID;
        public GroupID GroupID => groupID;

        [Header("Transitions")] 
        [SerializeField]
        private TransitionData[] transitions;

        [NonSerialized]
        protected Window windowInstance;
        public Window WindowInstance => windowInstance;

        public bool HasWindowInstance
        {
            get
            {
                if (windowInstance == null)
                    return false;
                return windowInstance.Initialized;
            }
        }

        public void Open()
        {
            this.windowInstance.WindowsManager.Open(this);
        }

        public bool TryGetTransition(TransitionType transitionType, out AnimatedTransition resultAnimatedTransition, out bool playBackwards)
        {
            if (TryGetTransitionOfType(transitionType, out TransitionBase resultTransition))
            {
                if (resultTransition is AnimatedTransition animatedTransition)
                {
                    resultAnimatedTransition = animatedTransition;
                    playBackwards = false;
                    return true;
                }

                if (resultTransition is ReverseTransition reverseTransition)
                {
                    if (TryGetTransitionOfType(reverseTransition.TransitionToPlayBackwards, out TransitionBase backwardsTransition))
                    {
                        if (backwardsTransition is AnimatedTransition inTransitionAnimated)
                        {
                            resultAnimatedTransition = inTransitionAnimated;
                            playBackwards = true;
                            return true;
                        }
                    }
                }
            }
            resultAnimatedTransition = null;
            playBackwards = false;
            return false;
        }

        public bool TryGetTransitionOfType(TransitionType transitionType, out TransitionBase resultTransition)
        {
            for (int i = 0; i < transitions.Length; i++)
            {
                TransitionData transitionData = transitions[i];
                if (transitionData.TransitionType != transitionType)
                    continue;

                resultTransition = transitionData.Transition;
                return true;
            }

            resultTransition = null;
            return false;
        }

        public abstract IEnumerator InstantiateEnumerator(WindowsManager windowsManager);
    }
}
