using System;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    /// <summary> 小節の単位 </summary>
    [Serializable]
    internal struct MornBeatMeasure
    {
        /// <summary> 何小節目か(0始まり) </summary>
        [SerializeField] public int Index;
        /// <summary> 何サンプル目スタートか </summary>
        [SerializeField] public long StartSamples;

        public MornBeatMeasure(int index, long startSamples)
        {
            Index = index;
            StartSamples = startSamples;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MornBeatMeasure))]
    internal sealed class MornBeatMeasureDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var indexSp = property.FindPropertyRelative(nameof(MornBeatMeasure.Index));
            var measureSp = property.FindPropertyRelative(nameof(MornBeatMeasure.StartSamples));
            var width = position.width / 2f;
            var indexRect = new Rect(position.x, position.y, width, position.height);
            var measureRect = new Rect(position.x + width, position.y, width, position.height);
            EditorGUI.PropertyField(indexRect, indexSp, new GUIContent("Measure"));
            EditorGUI.PropertyField(measureRect, measureSp, new GUIContent("Samples"));
        }
    }
#endif
}