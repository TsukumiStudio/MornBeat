#if UNITY_EDITOR
using System;
using UnityEditor;

namespace MornLib
{
    internal sealed class MornBeatFoldoutGroup
    {
        private bool _isFoldout;
        private readonly Action _guiAction;
        private readonly string _label;

        public MornBeatFoldoutGroup(Action guiAction, string label)
        {
            _guiAction = guiAction;
            _label = label;
        }

        public void OnGUI()
        {
            _isFoldout = EditorGUILayout.Foldout(_isFoldout, _label, true);
            if (_isFoldout)
            {
                _guiAction?.Invoke();
            }
        }
    }
}
#endif