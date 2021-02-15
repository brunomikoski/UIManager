using System;
using System.Collections.Generic;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager.Animation
{
    [Serializable]
    public class DOTweenBaseAnimationStep : BaseAnimationStep
    {
        [SerializeField]
        protected float delay;
        public float Delay => delay;

        [SerializeField]
        protected float duration = 1;
        public float Duration => duration;
        
        [SerializeReference, HideInInspector]
        private DOTweenSettingsBase[] instructionBase;
        public DOTweenSettingsBase[] InstructionBase => instructionBase;

        public DOTweenBaseAnimationStep()
        {
            List<Type> tweenSettings = TypeUtility.GetAllSubclasses(typeof(DOTweenSettingsBase));
            List<DOTweenSettingsBase> availableAnimations = new List<DOTweenSettingsBase>();
            for (int i = 0; i < tweenSettings.Count; i++)
            {
                Type type = tweenSettings[i];
                if (type.IsAbstract)
                    continue;
                
                availableAnimations.Add(Activator.CreateInstance(type) as DOTweenSettingsBase);
            }

            instructionBase = availableAnimations.ToArray();
        }

        
        public override void Play(ref float time)
        {
            
        }

        public override string ToString()
        {
            return "DOTween";
        }
    }
}
