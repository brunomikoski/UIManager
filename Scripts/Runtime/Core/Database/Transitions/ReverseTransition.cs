using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class ReverseTransition : InstructionTransition
    {
        [SerializeField]
        private TransitionType transitionToPlayBackwards;
        public TransitionType TransitionToPlayBackwards => transitionToPlayBackwards;
    }
}
