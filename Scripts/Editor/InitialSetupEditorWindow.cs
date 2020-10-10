using BrunoMikoski.ScriptableObjectCollections;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class InitialSetupEditorWindow : EditorWindow
    {
        private DefaultAsset ScriptableObjectFolder;
        private DefaultAsset GeneratedCodeFolder;

        public static InitialSetupEditorWindow GetWindowInstance()
        {
            return GetWindow<InitialSetupEditorWindow>("Setup UI Manager");
        }
        
        public static void Open()
        {
            InitialSetupEditorWindow window = GetWindowInstance();
            window.ShowPopup();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                EditorGUILayout.LabelField("Settings", EditorStyles.foldoutHeader);
                EditorGUILayout.Space();
                ScriptableObjectFolder = (DefaultAsset) EditorGUILayout.ObjectField("Scriptable Objects Folder",
                    ScriptableObjectFolder, typeof(DefaultAsset), false);
                
                GeneratedCodeFolder = (DefaultAsset) EditorGUILayout.ObjectField("Generated Code Folder",
                    GeneratedCodeFolder, typeof(DefaultAsset), false);

                
                using (new EditorGUI.DisabledScope(!AreSettingsValid()))
                {
                    Color color = GUI.color;
                    GUI.color = Color.green;
                    if (GUILayout.Button("Create"))
                        PerformInitialSetup();

                    GUI.color = color;
                }
            }
        }

        private void PerformInitialSetup()
        {
            WindowIDs windowIDs = ScriptableObjectCollectionUtils.CreateScriptableObjectOfType<WindowIDs>(ScriptableObjectFolder, true,
                "WindowIDs");
            
            LayerIDs layerIDs = ScriptableObjectCollectionUtils.CreateScriptableObjectOfType<LayerIDs>(ScriptableObjectFolder, true,
                "LayerIDs");
            layerIDs.AddNew("Main");
            layerIDs.AddNew("Popup");
            layerIDs.AddNew("Overlay");
            
            GroupIDs groupIDs = ScriptableObjectCollectionUtils.CreateScriptableObjectOfType<GroupIDs>(ScriptableObjectFolder, true,
                "GroupIDs");
            groupIDs.AddNew("Main");
            
            Transitions transitions = ScriptableObjectCollectionUtils.CreateScriptableObjectOfType<Transitions>(ScriptableObjectFolder, true,
                "Transitions");
            transitions.AddNew(typeof(ReverseTransition), "ReverseTransition");
            
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFile(windowIDs, true);
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFile(layerIDs, true);
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFile(groupIDs, true);
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFile(transitions, true);

            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFileName(windowIDs, "WindowIDStatic");
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFileName(layerIDs, "LayerIDStatic");
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFileName(groupIDs, "GroupIDStatic");
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFileName(transitions, "TransitionStatic");
            
            string generatedCodeFolderPath = AssetDatabase.GetAssetPath(GeneratedCodeFolder);
            
            ScriptableObjectCollectionSettings.Instance.SetOverridingStaticFileLocation(windowIDs, true);
            ScriptableObjectCollectionSettings.Instance.SetOverridingStaticFileLocation(layerIDs, true);
            ScriptableObjectCollectionSettings.Instance.SetOverridingStaticFileLocation(groupIDs, true);
            ScriptableObjectCollectionSettings.Instance.SetOverridingStaticFileLocation(transitions, true);
            
            ScriptableObjectCollectionSettings.Instance.SetStaticFileFolderForCollection(windowIDs, generatedCodeFolderPath);
            ScriptableObjectCollectionSettings.Instance.SetStaticFileFolderForCollection(layerIDs, generatedCodeFolderPath);
            ScriptableObjectCollectionSettings.Instance.SetStaticFileFolderForCollection(groupIDs, generatedCodeFolderPath);
            ScriptableObjectCollectionSettings.Instance.SetStaticFileFolderForCollection(transitions, generatedCodeFolderPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private bool AreSettingsValid()
        {
            return ScriptableObjectFolder != null;
        }
    }
}
