using System.IO;
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
            if (!CollectionsRegistry.Instance.TryGetCollectionOfType(out UIWindowCollection _))
                ScriptableObjectCollectionUtility.CreateScriptableObjectOfType<UIWindowCollection>(Path.Combine(AssetDatabase.GetAssetPath(scriptableObjectFolder), "Windows"), "UIWindowCollection");
            
            if (!CollectionsRegistry.Instance.TryGetCollectionOfType(out UILayerCollection layerIDs))
                layerIDs = ScriptableObjectCollectionUtility.CreateScriptableObjectOfType<UILayerCollection>(Path.Combine(AssetDatabase.GetAssetPath(scriptableObjectFolder), "Layers"), "UILayerCollection");
            

            layerIDs.GetOrAddNew("Main");
            layerIDs.GetOrAddNew("Popup").SetIncludedInHistory(false);
            layerIDs.GetOrAddNew("Overlay").SetIncludedInHistory(false);

            if (!CollectionsRegistry.Instance.TryGetCollectionOfType(out UIGroupCollection groupIDs))
                groupIDs = ScriptableObjectCollectionUtility.CreateScriptableObjectOfType<UIGroupCollection>(Path.Combine(AssetDatabase.GetAssetPath(scriptableObjectFolder), "Groups"), "UIGroupCollection");
            
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
            if (!CollectionsRegistry.Instance.TryGetCollectionOfType<UIWindowCollection>(out _))
                return true;

            if (!CollectionsRegistry.Instance.TryGetCollectionOfType<UILayerCollection>(out _))
                return true;

            if (!CollectionsRegistry.Instance.TryGetCollectionOfType<UIGroupCollection>(out _))
                return true;
           
            return false;
        }
    }
}
