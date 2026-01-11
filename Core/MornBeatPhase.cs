using System;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    /// <summary> フェーズの単位 </summary>
    [Serializable]
    internal struct MornBeatPhase
    {
        [SerializeField] public MornBeatPhaseTransition Transition;
        [SerializeField] public MornBeatPhaseLength Length;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MornBeatPhase))]
    internal class MornBeatPhasePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            {
                // Serialized Properties
                var transitionSp = property.FindPropertyRelative(nameof(MornBeatPhase.Transition));
                var lengthSp = property.FindPropertyRelative(nameof(MornBeatPhase.Length));

                // Rect
                var transitionRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                var lengthRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

                // Draw
                EditorGUI.PropertyField(transitionRect, transitionSp);
                EditorGUI.PropertyField(lengthRect, lengthSp);
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }
    }
#endif
}