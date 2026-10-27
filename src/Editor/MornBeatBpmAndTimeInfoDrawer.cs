using UnityEditor;
using UnityEngine;

namespace MornLib
{
    [CustomPropertyDrawer(typeof(MornBeatMusic.BpmAndTimeInfo))]
    internal sealed class MornBeatBpmAndTimeInfoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);
            var fieldX = position.x + EditorGUIUtility.labelWidth + 2;
            var fieldW = (position.width - EditorGUIUtility.labelWidth - 2 - 20) / 2;
            var bpmSp = property.FindPropertyRelative(nameof(MornBeatMusic.BpmAndTimeInfo.Bpm));
            var timeSp = property.FindPropertyRelative(nameof(MornBeatMusic.BpmAndTimeInfo.Time));
            var bpmLabelW = 30f;
            var timeLabelW = 30f;
            var gap = 4f;
            EditorGUI.LabelField(new Rect(fieldX, position.y, bpmLabelW, position.height), "Bpm");
            EditorGUI.PropertyField(
                new Rect(fieldX + bpmLabelW, position.y, fieldW - bpmLabelW, position.height),
                bpmSp, GUIContent.none);
            EditorGUI.LabelField(
                new Rect(fieldX + fieldW + gap, position.y, timeLabelW, position.height), "Time");
            EditorGUI.PropertyField(
                new Rect(fieldX + fieldW + gap + timeLabelW, position.y, fieldW - timeLabelW, position.height),
                timeSp, GUIContent.none);
            EditorGUI.EndProperty();
        }
    }
}
