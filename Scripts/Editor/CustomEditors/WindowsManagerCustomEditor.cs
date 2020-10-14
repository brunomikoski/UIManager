using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.UIManager.CustomEditors
{
    [CustomEditor(typeof(WindowsManager))]
    public class WindowsManagerCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawEditorSpawning();
        }

        private void DrawEditorSpawning()
        {
            using (new EditorGUI.DisabledGroupScope(DisableEditorSpawningOptions()))
            {
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUILayout.LabelField("Windows", EditorStyles.foldoutHeader);
                    EditorGUILayout.Space();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Instantiate All Windows", EditorStyles.miniButton))
                            InstantiateAllWindows();
                        if (GUILayout.Button("Remove All Windows", EditorStyles.miniButton))
                            InstantiateAllWindows();
                    }
                }
            }
        }

        private void InstantiateAllWindows()
        {
            WindowsManager windowsManager = target as WindowsManager;
            windowsManager.InitializeLayers();
        }

        private bool DisableEditorSpawningOptions()
        {
            return Application.isPlaying;
        }
    }
}