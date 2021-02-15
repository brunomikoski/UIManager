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
    public static class AnimationControllerUtility
    {
        
    }
    //https://github.com/Link-SD/Unity3D-Extending-The-Editor/blob/master/Assets/Editor/Scripts/TutorialShortcutCustomEditor.cs
    [CustomEditor(typeof(AnimationController))]
    public class AnimationControllerCustomEditor : Editor
    {
        private GenericMenu availableAnimatedTypesMenu;
        private SerializedProperty animationStepsProperty;
        private AnimationController animationController;
        
        private void OnEnable()
        {
            animationStepsProperty = serializedObject.FindProperty("animationSteps");
            animationController = target as AnimationController;
            GenerateStepsOptions();
        }

        private void OnDisable()
        {
            DOTweenEditorPreview.Stop();
        }
        
        private void GenerateStepsOptions()
        {
            List<Type> typesOfAnimatedItems = TypeUtility.GetAllSubclasses(typeof(BaseAnimationStep));
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


        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            DrawBoxedArea("Preview", DrawPreviewControls);
            EditorGUI.BeginChangeCheck();
            DrawBoxedArea("Steps", StepsControls);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        private void StepsControls()
        {
            for (int i = 0; i < animationStepsProperty.arraySize; i++)
            {
                SerializedProperty animationStepSP = animationStepsProperty.GetArrayElementAtIndex(i);
                DrawAnimationStep(animationController.AnimationSteps[i], animationStepSP, i);

            }
            DrawControls();
        }

        private void DrawAnimationStep(BaseAnimationStep baseAnimationStep, SerializedProperty animationStepSP, int index)
        {
            using (new EditorGUILayout.VerticalScope("FrameBox"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    Rect controlRect = EditorGUILayout.GetControlRect();
                    controlRect.x += 14;
                    controlRect.width -= 18;
                    animationStepSP.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(controlRect, animationStepSP.isExpanded, 
                        $"{index}. {baseAnimationStep.Target.name}: {baseAnimationStep}");

                    if (animationStepSP.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(animationStepSP);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }
        }

        private void DrawControls()
        {
            if (GUILayout.Button("+"))
            {
                availableAnimatedTypesMenu.ShowAsContext();
            }
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
                BaseAnimationStep baseAnimationStep = animationController.AnimationSteps[i];

                if (baseAnimationStep is DOTweenBaseAnimationStep doTweenAnimationStep)
                {
                    for (int j = 0; j < doTweenAnimationStep.InstructionBase.Length; j++)
                    {
                        DOTweenSettingsBase doTweenSettings = doTweenAnimationStep.InstructionBase[j];

                        if (!doTweenSettings.Enabled)
                            continue;

                        Tween tween = doTweenSettings.GenerateTween(baseAnimationStep.Target, doTweenAnimationStep.Duration);

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
            // SerializedProperty element = reordList.serializedProperty.GetArrayElementAtIndex(index);
            // rect.y += 2;
            //
            // string elementTitle ="New Shortcut";
            //
            // EditorGUI.BeginProperty(rect, new GUIContent(elementTitle), element);
            //
            // EditorGUI.PropertyField(
            //     new Rect(rect.x += 10, rect.y, Screen.width * .8f, EditorGUIUtility.singleLineHeight),
            //     element,
            //     new GUIContent(elementTitle),
            //     true
            // );
            //
            // GUILayout.FlexibleSpace();
            //
            // using (new GUILayout.VerticalScope("Box"))
            // {
            //     EditorGUI.LabelField(rect, "Test");
            // }
            //
            // EditorGUI.EndProperty();
            
        }

        // private float OnReorderListElementHeight(int index)
        // {
        //     float propertyHeight =
        //         EditorGUI.GetPropertyHeight(reordList.serializedProperty.GetArrayElementAtIndex(index), true);
        //     
        //     float spacing = EditorGUIUtility.singleLineHeight / 2;
        //     
        //     return propertyHeight + spacing;
        // }
    }
}
