// using System;
// using System.Collections.Generic;
// using BrunoMikoski.ScriptableObjectCollections;
// using UnityEditor;
// using UnityEngine;
//
// namespace BrunoMikoski.UIManager.Animation
// {
//     [CustomPropertyDrawer(typeof(DOTweenAnimationStep), true)]
//     public sealed class DOTweenAnimationStepPropertyDrawer : PropertyDrawer
//     {
//         private bool initialized;
//         private SerializedProperty tweenTypeEnumMaskSP;
//         private SerializedProperty targetSP;
//         private SerializedProperty flowEnumSP;
//         private SerializedProperty delaySP;
//         private SerializedProperty durationSP;
//
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             label = EditorGUI.BeginProperty(position, label, property);
//             Initialize(property);
//
//
//             property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(position, property.isExpanded, label);
//             if (property.isExpanded)
//             {
//                 DrawBaseItem(ref position);
//                 DrawTweenActions(ref position);
//             }
//             EditorGUI.EndFoldoutHeaderGroup();
//             EditorGUI.EndProperty();
//         }
//
//         private void DrawTweenActions(ref Rect position)
//         {
//             DOTweenAction currentFlags = (DOTweenAction) tweenTypeEnumMaskSP.intValue;
//             
//             DOTweenAction[] availableTypes = ((DOTweenAction[]) Enum.GetValues(typeof(DOTweenAction)));
//             for (int i = 0; i < availableTypes.Length; i++)
//             {
//                 DOTweenAction availableAction = availableTypes[i];
//             }
//         }
//
//         private void DrawBaseItem(ref Rect position)
//         {
//             position.y += 4;
//             position.y += EditorGUIUtility.singleLineHeight;
//             EditorGUI.PropertyField(position, tweenTypeEnumMaskSP);
//             position.y += 4;
//             position.y += EditorGUIUtility.singleLineHeight;
//             EditorGUI.PropertyField(position, flowEnumSP);
//             position.y += 10;
//             position.y += EditorGUIUtility.singleLineHeight;
//             EditorGUI.PropertyField(position, targetSP);
//             position.y += 2;
//             position.y += EditorGUIUtility.singleLineHeight;
//             EditorGUI.PropertyField(position, delaySP);
//             position.y += 2;
//             position.y += EditorGUIUtility.singleLineHeight;
//             EditorGUI.PropertyField(position, durationSP);
//         }
//
//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             if (property.isExpanded)
//             {
//                 return 100;
//             }
//             
//             return base.GetPropertyHeight(property, label);
//         }
//
//         private void Initialize(SerializedProperty serializedProperty)
//         {
//             if (initialized)
//                 return;
//
//             targetSP = serializedProperty.FindPropertyRelative("target");
//             flowEnumSP = serializedProperty.FindPropertyRelative("flow");
//             tweenTypeEnumMaskSP = serializedProperty.FindPropertyRelative("tweenActions");
//             delaySP = serializedProperty.FindPropertyRelative("delay");
//             durationSP = serializedProperty.FindPropertyRelative("duration");
//
//
//             Dictionary<DOTweenAction, Type> instructionTypeToType = new Dictionary<DOTweenAction, Type>();
//             List<Type> tweenSettings = TypeUtility.GetAllSubclasses(typeof(DOTweenSettingsBase));
//             for (int i = 0; i < tweenSettings.Count; i++)
//             {
//                 Type type = tweenSettings[i];
//                 object instance = Activator.CreateInstance(type);
//                 object tweenAction =  type.GetProperty("Action")?.GetValue(instance);
//
//                 instructionTypeToType.Add((DOTweenAction) tweenAction, type);
//             }
//
//             initialized = true;
//         }
//     }
// }
