using System;
using System.Collections.Generic;
using BrunoMikoski.ScriptableObjectCollections;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BrunoMikoski.UIManager.Animation
{
    //https://github.com/Link-SD/Unity3D-Extending-The-Editor/blob/master/Assets/Editor/Scripts/TutorialShortcutCustomEditor.cs
    [CustomEditor(typeof(AnimationController))]
    public class AnimationControllerCustomEditor : Editor
    {
        private ReorderableList reordList;

        private GenericMenu availableAnimatedTypesMenu;
        private SerializedProperty animationStepsProperty;
        private AnimationController animationController;

        private void OnEnable()
        {
            animationStepsProperty = serializedObject.FindProperty("animationSteps");
            animationController = target as AnimationController;
            reordList = new ReorderableList(serializedObject, animationStepsProperty, true, false, true, true);
            reordList.drawElementCallback += OnDrawReorderListElement;
            reordList.elementHeightCallback += OnReorderListElementHeight;
            reordList.onAddDropdownCallback += OnAddDropdownCallback;
            LoadAnimatedItemTypes();
        }

        private void OnDisable()
        {
            reordList.drawElementCallback -= OnDrawReorderListElement;
            reordList.elementHeightCallback -= OnReorderListElementHeight;
            reordList.onAddDropdownCallback -= OnAddDropdownCallback;
            DOTweenEditorPreview.Stop();
        }
        
        private void LoadAnimatedItemTypes()
        {
            List<Type> typesOfAnimatedItems = TypeUtility.GetAllSubclasses(typeof(AnimationStepBase));
            availableAnimatedTypesMenu = new GenericMenu();
            for (int i = 0; i < typesOfAnimatedItems.Count; i++)
            {
                Type animatedItemType = typesOfAnimatedItems[i];
                if (animatedItemType.IsAbstract)
                    continue;
                availableAnimatedTypesMenu.AddItem(
                    new GUIContent(animatedItemType.Name),
                    false,
                    () =>
                    {
                        AddNewAnimationStepOfType(animatedItemType);
                    }
                );
            }
        }

        private void AddNewAnimationStepOfType(Type targetAnimationType)
        {
            int targetIndex = animationStepsProperty.arraySize;
            animationStepsProperty.InsertArrayElementAtIndex(targetIndex);
            SerializedProperty arrayElementAtIndex = animationStepsProperty.GetArrayElementAtIndex(targetIndex);
            object managedReferenceValue = Activator.CreateInstance(targetAnimationType);
            arrayElementAtIndex.managedReferenceValue = managedReferenceValue;

            SerializedProperty gameObjectTargetSP = arrayElementAtIndex.FindPropertyRelative("target");
            gameObjectTargetSP.objectReferenceValue = (serializedObject.targetObject as AnimationController)?.gameObject;
            
            
            
            serializedObject.ApplyModifiedProperties();
        }

        private void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
        {
            availableAnimatedTypesMenu.ShowAsContext();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            DrawBoxedArea("Preview", DrawPreviewControls);
            DrawBoxedArea("Steps", StepsControls);
            serializedObject.ApplyModifiedProperties();
        }

        private void StepsControls()
        {
            reordList.DoLayoutList();
        }

        private void DrawPreviewControls()
        {
            if (GUILayout.Button("Preview"))
            {
                
                PreviewAnimation();
            }
        }

        private void PreviewAnimation()
        {
            DOTweenEditorPreview.Stop();
            DOTweenEditorPreview.Start();
            float time = 0;
            for (int i = 0; i < animationController.AnimationSteps.Length; i++)
            {
                AnimationStepBase animationStep = animationController.AnimationSteps[i];

                if (animationStep is DOTweenAnimationStep doTweenAnimationStep)
                {
                    for (int j = 0; j < doTweenAnimationStep.InstructionBase.Length; j++)
                    {
                        DOTweenSettingsBase doTweenSettings = doTweenAnimationStep.InstructionBase[j];

                        if (!doTweenSettings.Enabled)
                            continue;

                        Tween tween = doTweenSettings.GenerateTween(animationStep.Target, doTweenAnimationStep.Duration);

                        if (tween == null)
                            continue;
                        
                        tween.SetDelay(time + doTweenAnimationStep.Delay);


                        DOTweenEditorPreview.PrepareTweenForPreview(tween, true, false);
                    }
                    time += doTweenAnimationStep.Delay + doTweenAnimationStep.Duration;
                }
            }
        }

        private void DrawBoxedArea(string title, Action additionalInspectorGUI)
        {
            using (new EditorGUILayout.VerticalScope("FrameBox"))
            {
                Rect rect = EditorGUILayout.GetControlRect();
                rect.x -= 4;
                rect.width += 8;
                rect.y -= 4;
                GUIStyle style = GUI.skin.GetStyle("MeTransitionHead");
                style.normal.textColor = Color.white;
                GUI.Label(rect, title, style);
                EditorGUILayout.Space();
                additionalInspectorGUI.Invoke();
            }
        }


        private void OnDrawReorderListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reordList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            string elementTitle ="New Shortcut";

            EditorGUI.BeginProperty(rect, new GUIContent(elementTitle), element);
            
            EditorGUI.PropertyField(
                new Rect(rect.x += 10, rect.y, Screen.width * .8f, EditorGUIUtility.singleLineHeight),
                element,
                new GUIContent(elementTitle),
                true
            );
           
            EditorGUI.EndProperty();
            
        }

        private float OnReorderListElementHeight(int index)
        {
            float propertyHeight =
                EditorGUI.GetPropertyHeight(reordList.serializedProperty.GetArrayElementAtIndex(index), true);
        
            float spacing = EditorGUIUtility.singleLineHeight / 2;
        
            return propertyHeight + spacing;
        }
    }
}
