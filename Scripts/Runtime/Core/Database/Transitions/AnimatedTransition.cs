using System.Collections;

namespace BrunoMikoski.UIManager
{
    public abstract class AnimatedTransition : TransitionBase
    {
        public abstract IEnumerator ExecuteEnumerator(Window targetWindow, bool isBackwards);

        public virtual void BeforeTransition(Window targetWindow){}
    }
}
