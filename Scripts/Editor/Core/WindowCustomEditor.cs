using System.Collections.Generic;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    [CustomEditor(typeof(Window))]
    public class WindowCustomEditor : BaseEditor<Window>
    {
        private SerializedProperty windowIDProperty;
        private bool lazyWindowIDReference;

        protected void OnEnable()
        {
            windowIDProperty = serializedObject.FindProperty("windowID");
            if (windowIDProperty.objectReferenceValue != null)
            {
                WindowID windowID = windowIDProperty.objectReferenceValue as WindowID;
                if (windowID is PrefabWindowID)
                {
                    lazyWindowIDReference = true;
                }
#if USE_ADDRESSABLES
                else if (windowID is AddressablesWindowID)
                {
                    lazyWindowIDReference = true;
                }
#endif
            }
            ValidateWindowIDReference();
        }

        private void ValidateWindowIDReference()
        {
            List<ScriptableObjectCollection> windowsCollections =
                CollectionsRegistry.Instance.GetCollectionsByItemType<WindowID>();
            WindowID currentWindowID = null;
            if (windowIDProperty.objectReferenceValue != null)
                currentWindowID = windowIDProperty.objectReferenceValue as WindowID;
                
            for (int i = 0; i < windowsCollections.Count; i++)
            {
                ScriptableObjectCollection windowsCollection = windowsCollections[i];
                for (int j = 0; j < windowsCollection.Count; j++)
                {
                    WindowID windowID = (WindowID) windowsCollection[j];
                    if (windowID == null)
                        continue;

                    GameObject targetObject = PrefabUtility.GetCorrespondingObjectFromOriginalSource(Target.gameObject);
                    if (targetObject == null)
                        targetObject = Target.gameObject;
                    
                    bool isPartOfPrefab = PrefabUtility.IsPartOfRegularPrefab(Target.gameObject);
                    bool isAtScene = !string.IsNullOrEmpty(Target.gameObject.scene.path);
                    
                    if (windowID is PrefabWindowID prefabWindowID)
                    {
                        if (prefabWindowID.WindowPrefab == null)
                            continue;
                        
                        if (prefabWindowID.WindowPrefab.gameObject == targetObject)
                        {
                            if (prefabWindowID != currentWindowID)
                            {
                                windowIDProperty.objectReferenceValue = prefabWindowID;
                                windowIDProperty.serializedObject.ApplyModifiedProperties();
                                lazyWindowIDReference = true;
                            }
                        }
                    }
#if USE_ADDRESSABLES
                    else if (windowID is AddressablesWindowID addressablesWindowID)
                    {
                        if (!addressablesWindowID.WindowPrefabAssetRef.RuntimeKeyIsValid())
                            continue;

                        if (addressablesWindowID.WindowPrefabAssetRef.editorAsset == targetObject)
                        {
                            if (windowIDProperty.objectReferenceValue != windowID)
                            {
                                if (isPartOfPrefab && isAtScene)
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(Target.gameObject);

                                windowIDProperty.objectReferenceValue = windowID;
                                windowIDProperty.serializedObject.ApplyModifiedProperties();
                                lazyWindowIDReference = true;

                                if (isPartOfPrefab && isAtScene)
                                    PrefabUtility.ApplyPrefabInstance(Target.gameObject, InteractionMode.AutomatedAction);
                            }
                        }
                    }
#endif
                }
            }
        }

        public override void OnInspectorGUI()
        {
            BeginInspector();
            EditorGUI.BeginChangeCheck();
            ExcludeProperty("windowID");

            EditorGUI.BeginDisabledGroup(lazyWindowIDReference);
            EditorGUILayout.PropertyField(windowIDProperty);
            EditorGUI.EndDisabledGroup();
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            DrawRemainingPropertiesInInspector();
        }
    }
}