using System;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    [Serializable]
    internal struct MornBeatPhaseLength
    {
        /// <summary> 何拍子か </summary>
        [SerializeField] public int Beat;
        /// <summary> 何分音符が基準か </summary>
        [SerializeField] public int NoteType;
        /// <summary> 何小節か </summary>
        [SerializeField] public int Measure;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MornBeatPhaseLength))]
    internal class MornBeatPhaseLengthDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float buttonWidth = 20;
            EditorGUI.BeginProperty(position, label, property);
            {
                // Label
                var valueRect = EditorGUI.PrefixLabel(position, new GUIContent("Beat"));

                // Serialized Properties
                var beatSp = property.FindPropertyRelative(nameof(MornBeatPhaseLength.Beat));
                var beatTypeSp = property.FindPropertyRelative(nameof(MornBeatPhaseLength.NoteType));
                var measureCountSp = property.FindPropertyRelative(nameof(MornBeatPhaseLength.Measure));

                // Rect
                var valueY = valueRect.y;
                var valueRectW = (valueRect.width - buttonWidth * 3) / 3;
                var valueRectH = valueRect.height;
                var beatValueRect = new Rect(valueRect.x, valueRect.y, valueRectW, valueRectH);
                var divideRect = new Rect(beatValueRect.x + valueRectW, valueY, buttonWidth, valueRectH);
                var beatBaseValueRect = new Rect(divideRect.x + buttonWidth, valueY, valueRectW, valueRectH);
                var multiRect = new Rect(beatBaseValueRect.x + valueRectW, valueY, buttonWidth, valueRectH);
                var measureCountValueRect = new Rect(multiRect.x + buttonWidth, valueY, valueRectW, valueRectH);

                // Draw
                EditorGUI.PropertyField(beatValueRect, beatSp, GUIContent.none);
                EditorGUI.LabelField(divideRect, "  /");
                EditorGUI.PropertyField(beatBaseValueRect, beatTypeSp, GUIContent.none);
                EditorGUI.LabelField(multiRect, "  *");
                EditorGUI.PropertyField(measureCountValueRect, measureCountSp, GUIContent.none);
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}