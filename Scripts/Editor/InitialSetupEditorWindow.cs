using BrunoMikoski.ScriptableObjectCollections;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class InitialSetupEditorWindow : EditorWindow
    {
        private DefaultAsset scriptableObjectFolder;
        private DefaultAsset generatedCodeFolder;

        private static InitialSetupEditorWindow GetWindowInstance()
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
                scriptableObjectFolder = (DefaultAsset) EditorGUILayout.ObjectField("Scriptable Objects Folder",
                    scriptableObjectFolder, typeof(DefaultAsset), false);
                
                generatedCodeFolder = (DefaultAsset) EditorGUILayout.ObjectField("Generated Code Folder",
                    generatedCodeFolder, typeof(DefaultAsset), false);

                
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
            if (!CollectionsRegistry.Instance.TryGetCollectionOfType(out WindowIDs windowIDs))
            {
                windowIDs = ScriptableObjectCollectionUtils.CreateScriptableObjectOfType<WindowIDs>(scriptableObjectFolder, true,
                    "WindowIDs");
            }
            
            if (!CollectionsRegistry.Instance.TryGetCollectionOfType(out LayerIDs layerIDs))
            {
                layerIDs = ScriptableObjectCollectionUtils.CreateScriptableObjectOfType<LayerIDs>(scriptableObjectFolder, true,
                    "LayerIDs");
            }
            

            layerIDs.GetOrAddNew("Main");
            layerIDs.GetOrAddNew("Popup");
            layerIDs.GetOrAddNew("Overlay");

            if (!CollectionsRegistry.Instance.TryGetCollectionOfType(out GroupIDs groupIDs))
            {
                groupIDs = ScriptableObjectCollectionUtils.CreateScriptableObjectOfType<GroupIDs>(scriptableObjectFolder, true,
                    "GroupIDs");
            }
            groupIDs.GetOrAddNew("Main");


            if (!CollectionsRegistry.Instance.TryGetCollectionOfType(out Transitions transitions))
            {
                transitions = ScriptableObjectCollectionUtils.CreateScriptableObjectOfType<Transitions>(scriptableObjectFolder, true,
                    "Transitions");
            }
            transitions.GetOrAddNew(typeof(ReverseTransition), "ReverseInTransition");
            transitions.GetOrAddNew<FadeTransition>("FadeInTransition")
                .SetAnimationValues(0, 1, 0.3f, Ease.Linear);
            
            transitions.GetOrAddNew<ScaleTransition>("ScaleInTransition")
                .SetAnimationValues(Vector3.zero, Vector3.one, 0.6f, Ease.OutBack);
            
            
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFile(windowIDs, true);
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFile(layerIDs, true);
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFile(groupIDs, true);
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFile(transitions, true);

            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFileName(windowIDs, "WindowIDsStatic");
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFileName(layerIDs, "LayerIDsStatic");
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFileName(groupIDs, "GroupIDsStatic");
            ScriptableObjectCollectionSettings.Instance.SetGenerateCustomStaticFileName(transitions, "TransitionsStatic");
            
            string generatedCodeFolderPath = AssetDatabase.GetAssetPath(generatedCodeFolder);
            
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
            Close();
        }

        private bool AreSettingsValid()
        {
            return scriptableObjectFolder != null;
        }

        public static bool NeedSetup()
        {
            if (!CollectionsRegistry.Instance.TryGetCollectionOfType<WindowIDs>(out _))
                return true;

            if (!CollectionsRegistry.Instance.TryGetCollectionOfType<LayerIDs>(out _))
                return true;

            if (!CollectionsRegistry.Instance.TryGetCollectionOfType<GroupIDs>(out _))
                return true;

            if (!CollectionsRegistry.Instance.TryGetCollectionOfType<Transitions>(out _))
                return true;
            
            return false;
        }
    }
}
