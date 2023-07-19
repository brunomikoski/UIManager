using System;

namespace BrunoMikoski.UIManager
{
    public interface IAsyncPrefabLoader
    {
        void LoadPrefab(Action loadedCallback = null);
        void UnloadPrefab();
        bool IsLoaded();
    }
}