using System;
using UnityEngine;

namespace BrunoMikoski.UIManager.Animation
{
    [Serializable]
    public class AnimationStepBase
    {
        [SerializeField]
        protected GameObject target;
        public GameObject Target => target;
        [SerializeField]
        protected FlowType flow;

        public virtual void Play(ref float time)
        {
            
        }

        public void SetTarget(GameObject target)
        {
            this.target = target;
        }
    }
}
