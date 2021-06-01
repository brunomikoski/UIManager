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
            LayerID popup = layerIDs.GetOrAddNew("Popup");
            popup.SetIncludedInHistory(false);
            layerIDs.GetOrAddNew("Overlay");

            if (!CollectionsRegistry.Instance.TryGetCollectionOfType(out GroupIDs groupIDs))
            {
                groupIDs = ScriptableObjectCollectionUtils.CreateScriptableObjectOfType<GroupIDs>(scriptableObjectFolder, true,
                    "GroupIDs");
            }
            groupIDs.GetOrAddNew("Main");

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
           
            return false;
        }
    }
}
