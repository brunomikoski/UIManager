using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BrunoMikoski.UIManager
{
    public interface IOnWindowInitialized
    {
        void OnWindowInitialized();
    }

    public interface IOnBeforeOpenWindow
    {
        void OnBeforeOpenWindow();
    }

    public interface IOnAfterWindowOpen
    {
        void OnAfterWindowOpen();
    }
    
    public interface IOnBeforeWindowClose
    {
        void OnBeforeWindowClose();
    }
    
    public interface IOnAfterWindowClose
    {
        void OnAfterWindowClose();
    }
    
    public interface IOnGainFocus
    {
        void OnGainFocus();
    }
    
    public interface IOnLostFocus
    {
        void OnLostFocus();
    }

    public interface IAsyncPrefabLoader
    {
        IEnumerator LoadWindowPrefab();
        void UnloadWindowPrefab();
    }
}
