using System.Collections;
using UnityEngine;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static IEnumerator AsCoroutine(this Task task)
        {
            yield return new WaitUntil(() => task.IsCompleted);
            task.GetAwaiter().GetResult();
        }
    }
}
