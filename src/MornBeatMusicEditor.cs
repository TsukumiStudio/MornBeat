#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    [CustomEditor(typeof(MornBeatMusic))]
    internal sealed class MornBeatMusicEditor : Editor
    {
        private MornBeatMusic _music;

        private void OnEnable()
        {
            _music = (MornBeatMusic)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);
            if (GUILayout.Button("MakeBeat", GUILayout.Height(30)))
            {
                _music.MakeBeat();
            }
        }
    }
}
#endif
