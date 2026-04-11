using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace MornLib
{
    [CreateAssetMenu(menuName = "Morn/Beat/" + nameof(MornBeatMusic))]
    public sealed class MornBeatMusic : ScriptableObject
    {
        // --- Audio ---
        [Header("Audio")]
        [SerializeField] private AudioClip _introClip;
        [SerializeField] private AudioClip _clip;
        [SerializeField] private bool _isLoop;
        [SerializeField] [Range(0, 1f)] private float _volume = 1f;
        [SerializeField] private float _offset;
        [SerializeField] private float _loopAdditionalTime;

        // --- Timing ---
        [Header("Timing")]
        [SerializeField] private List<float> _timingList;
        [SerializeField] private int _introTickSum;
        [SerializeField] private int _measureTickCount = 8;
        [SerializeField] private int _beatCount = 4;
        [SerializeField] private double _interval = 0.000001d;
        [SerializeField] private List<BpmAndTimeInfo> _bpmAndTimeInfoList;

        // --- Properties ---
        public bool IsLoop => _isLoop;
        public int MeasureTickCount => _measureTickCount;
        public int BeatCount => _beatCount;
        public int BeatTick => MeasureTickCount / BeatCount;
        public int IntroTickSum => _introTickSum;
        public int LoopTickSum => TotalTickSum - _introTickSum;
        public int TotalTickSum => _timingList.Count;
        public AudioClip IntroClip => _introClip;
        public AudioClip Clip => _clip;
        public float IntroLength => _introClip == null ? 0 : _introClip.length;
        public float LoopLength => _clip == null ? 0 : _clip.length + _loopAdditionalTime;
        public float TotalLength => IntroLength + LoopLength;
        public float Volume => _volume;
        internal float Offset => _offset;

        public void OverrideTimingList(List<float> timingList)
        {
            _timingList = timingList;
        }

        public float GetBeatTiming(int index)
        {
            if (index < 0 || TotalTickSum <= index) return Mathf.Infinity;
            return _timingList[index];
        }

        internal void MakeBeat()
        {
            Assert.IsNotNull(_clip);
            var beat = 0d;
            var time = 0d;
            _introTickSum = 0;
            _interval = Math.Max(0.000001f, _interval);
            _timingList.Clear();
            _timingList.Add(0);
            var totalLength = TotalLength;
            while (time < totalLength)
            {
                var bpm = GetBpm(time);
                var dif = bpm / 60 * _measureTickCount / _beatCount * _interval;
                if (Math.Floor(beat) < Math.Floor(beat + dif))
                {
                    _timingList.Add((float)time % totalLength);
                    if (time < IntroLength) _introTickSum++;
                }

                beat += dif;
                time += _interval;
            }

            var remove = _timingList.Count % _measureTickCount;
            for (var i = 0; i < remove; i++) _timingList.RemoveAt(_timingList.Count - 1);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public double GetBpm(double time)
        {
            switch (_bpmAndTimeInfoList.Count)
            {
                case 0:
                    return 60;
                case 1:
                    return _bpmAndTimeInfoList[0].Bpm;
            }

            if (time <= _bpmAndTimeInfoList[0].Time) return _bpmAndTimeInfoList[0].Bpm;
            for (var i = 1; i < _bpmAndTimeInfoList.Count; i++)
            {
                if (_bpmAndTimeInfoList[i].Time <= time) continue;
                var begin = _bpmAndTimeInfoList[i - 1];
                var end = _bpmAndTimeInfoList[i];
                var t1 = MornBeatUtil.InverseLerp(begin.Time, end.Time, time);
                return MornBeatUtil.Lerp(begin.Bpm, end.Bpm, t1);
            }

            return _bpmAndTimeInfoList[^1].Bpm;
        }

        public float GetMinBpm()
        {
            if (_bpmAndTimeInfoList.Count == 0) return 60f;
            var minBpm = _bpmAndTimeInfoList[0].Bpm;
            for (var i = 1; i < _bpmAndTimeInfoList.Count; i++)
            {
                if (_bpmAndTimeInfoList[i].Bpm < minBpm)
                {
                    minBpm = _bpmAndTimeInfoList[i].Bpm;
                }
            }

            return (float)minBpm;
        }

        public float GetMaxBpm()
        {
            if (_bpmAndTimeInfoList.Count == 0) return 60f;
            var maxBpm = _bpmAndTimeInfoList[0].Bpm;
            for (var i = 1; i < _bpmAndTimeInfoList.Count; i++)
            {
                if (_bpmAndTimeInfoList[i].Bpm > maxBpm)
                {
                    maxBpm = _bpmAndTimeInfoList[i].Bpm;
                }
            }

            return (float)maxBpm;
        }

        [Serializable]
        internal struct BpmAndTimeInfo
        {
            public double Bpm;
            public double Time;
        }
    }

#if UNITY_EDITOR
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

    [CustomPropertyDrawer(typeof(MornBeatMusic.BpmAndTimeInfo))]
    internal sealed class BpmAndTimeInfoDrawer : PropertyDrawer
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
#endif
}
