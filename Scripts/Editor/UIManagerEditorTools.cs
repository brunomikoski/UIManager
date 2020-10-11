using BrunoMikoski.ScriptableObjectCollections;
using UnityEditor;

namespace BrunoMikoski.UIManager
{
    public static class UIManagerEditorTools 
    {

        [MenuItem("Tools/UI Manager/Initial Setup")]
        public static void InitialSetup()
        {
            InitialSetupEditorWindow.Open();
        }
        
        [MenuItem("Tools/UI Manager/Initial Setup", true)]
        public static bool InitialSetup_Validator()
        {
            return !InitialSetupEditorWindow.NeedSetup();
        }
    }
 
}
