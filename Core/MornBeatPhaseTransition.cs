using System;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    [Serializable]
    internal struct MornBeatPhaseTransition
    {
        /// <summary> 開始時BPM </summary>
        [SerializeField] public double StartBpm;
        /// <summary> 終了時BPM </summary>
        [SerializeField] public double EndBpm;
        /// <summary> BPMの遷移タイプ </summary>
        [SerializeField] public MornBeatPhaseBpmTransitionType PhaseBpmTransitionType;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MornBeatPhaseTransition))]
    internal class MornBeatPhaseTransitionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float buttonWidth = 20;
            EditorGUI.BeginProperty(position, label, property);
            {
                // Label
                var valueRect = EditorGUI.PrefixLabel(position, new GUIContent("Bpm"));

                // Serialized Properties
                var startBpmSp = property.FindPropertyRelative(nameof(MornBeatPhaseTransition.StartBpm));
                var endBpmSp = property.FindPropertyRelative(nameof(MornBeatPhaseTransition.EndBpm));
                var bpmTypeSp = property.FindPropertyRelative(nameof(MornBeatPhaseTransition.PhaseBpmTransitionType));

                // Rect
                var bpmRect = new Rect(valueRect.x, valueRect.y, valueRect.width - buttonWidth, valueRect.height);
                var bpmTypeRect = new Rect(valueRect.x + valueRect.width - buttonWidth, valueRect.y, buttonWidth, valueRect.height);

                // Draw
                switch (bpmTypeSp.enumValueIndex)
                {
                    case (int)MornBeatPhaseBpmTransitionType.Constant:
                        EditorGUI.PropertyField(bpmRect, startBpmSp, GUIContent.none);
                        endBpmSp.doubleValue = startBpmSp.doubleValue;
                        break;
                    case (int)MornBeatPhaseBpmTransitionType.Lerp:
                        var valueY = bpmRect.y;
                        var valueW = (bpmRect.width - buttonWidth) / 2;
                        var valueH = bpmRect.height;
                        var startBpmRect = new Rect(bpmRect.x, valueY, valueW, valueH);
                        var barRect = new Rect(startBpmRect.x + valueW, valueY, buttonWidth, valueH);
                        var endBpmRect = new Rect(barRect.x + buttonWidth, valueY, valueW, valueH);
                        EditorGUI.PropertyField(startBpmRect, startBpmSp, GUIContent.none);
                        EditorGUI.LabelField(barRect, "  -");
                        EditorGUI.PropertyField(endBpmRect, endBpmSp, GUIContent.none);
                        break;
                }

                EditorGUI.PropertyField(bpmTypeRect, bpmTypeSp, GUIContent.none);
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}