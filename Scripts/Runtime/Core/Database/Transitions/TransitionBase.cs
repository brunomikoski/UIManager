using System.Collections;
using BrunoMikoski.ScriptableObjectCollections;

namespace BrunoMikoski.UIManager
{
    public abstract class TransitionBase : CollectableScriptableObject
    {
    }

    public abstract class AnimatedTransition : TransitionBase
    {
        public abstract IEnumerator ExecuteEnumerator(Window targetWindow, bool isBackwards);

        public virtual void BeforeTransition(Window targetWindow){}
    }
}
