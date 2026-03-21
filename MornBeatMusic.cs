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
        [SerializeField] private AudioClip _loopClip;
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

        // --- Measure (サンプルベース) ---
        [Header("Measure")]
        [SerializeField] internal List<MornBeatMeasure> MeasureList;
        [SerializeField] internal List<MornBeatPhase> BpmList;
        [SerializeField] internal List<double> TimeList;

        // --- Properties ---
        public bool IsLoop => _isLoop;
        public int MeasureTickCount => _measureTickCount;
        public int BeatCount => _beatCount;
        public int BeatTick => MeasureTickCount / BeatCount;
        public int IntroTickSum => _introTickSum;
        public int LoopTickSum => TotalTickSum - _introTickSum;
        public int TotalTickSum => _timingList.Count;
        public AudioClip IntroClip => _introClip;
        public AudioClip Clip => _loopClip;
        public float IntroLength => _introClip == null ? 0 : _introClip.length;
        public float LoopLength => _loopClip == null ? 0 : _loopClip.length + _loopAdditionalTime;
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
            Assert.IsNotNull(_loopClip);
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
            MornBeatGlobal.SetDirty(this);
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

        /// <summary>対象のサンプル数を返します</summary>
        /// <param name="measure">何小節目か (1始まり)</param>
        /// <param name="beat">基準となる拍の何番目か (1始まり)</param>
        /// <param name="beatBase">基準となる拍が何拍子か</param>
        public long GetSample(int measure, int beat, int beatBase)
        {
            var idx = measure - 1;
            if (idx < 0 || idx >= MeasureList.Count) return 0;
            var current = MeasureList[idx];
            if (beat <= 1) return current.StartSamples;
            var nextSamples = idx + 1 < MeasureList.Count
                ? MeasureList[idx + 1].StartSamples
                : (long)_loopClip.samples;
            var span = nextSamples - current.StartSamples;
            return current.StartSamples + span * (beat - 1) / beatBase;
        }

        [Serializable]
        private struct BpmAndTimeInfo
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
        private SerializedProperty _measureListSerializedProperty;
        private SerializedProperty _bpmListSerializedProperty;
        private SerializedProperty _timeListSerializedProperty;
        private MornBeatFoldoutGroup _generateFromBpm;
        private MornBeatFoldoutGroup _generateFromTimeStamp;

        private void OnEnable()
        {
            _music = (MornBeatMusic)target;
            _measureListSerializedProperty = serializedObject.FindProperty(nameof(MornBeatMusic.MeasureList));
            _bpmListSerializedProperty = serializedObject.FindProperty(nameof(MornBeatMusic.BpmList));
            _timeListSerializedProperty = serializedObject.FindProperty(nameof(MornBeatMusic.TimeList));
            _generateFromBpm = new MornBeatFoldoutGroup(DrawGenerateFromBpm, "Generate From Bpm");
            _generateFromTimeStamp = new MornBeatFoldoutGroup(DrawGenerateFromTimeStamp, "Generate From TimeStamp");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            GUILayout.Space(10);

            // MakeBeat (タイミングリスト生成)
            if (GUILayout.Button("MakeBeat (タイミングリスト生成)"))
            {
                _music.MakeBeat();
            }

            GUILayout.Space(10);

            // Measure関連
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_measureListSerializedProperty);
            GUI.enabled = true;

            GUILayout.Space(30);
            _generateFromBpm.OnGUI();
            _generateFromTimeStamp.OnGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGenerateFromBpm()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_bpmListSerializedProperty);
            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                var isSuccess = true;
                MornBeatGlobal.Log("Generate Start");
                var result = new List<MornBeatMeasure>();
                result.Add(new MornBeatMeasure(1, 0));
                var frequency = _music.Clip.frequency;
                var virtualTime = 0d;
                const double deltaBeat = 0.000001d;
                foreach (var phase in _music.BpmList)
                {
                    if (phase.Transition.StartBpm == 0)
                    {
                        isSuccess = false;
                        MornBeatGlobal.LogError("StartBpm is 0");
                        break;
                    }

                    if (phase.Transition.EndBpm == 0)
                    {
                        isSuccess = false;
                        MornBeatGlobal.LogError("EndBpm is 0");
                        break;
                    }

                    if (phase.Length.Beat == 0)
                    {
                        isSuccess = false;
                        MornBeatGlobal.LogError("BeatCount is 0");
                        break;
                    }

                    if (phase.Length.NoteType == 0)
                    {
                        isSuccess = false;
                        MornBeatGlobal.LogError("BeatType is 0");
                        break;
                    }

                    if (phase.Length.Measure == 0)
                    {
                        isSuccess = false;
                        MornBeatGlobal.LogError("MeasureCount is 0");
                        break;
                    }

                    var startBpm = phase.Transition.StartBpm;
                    var endBpm = phase.Transition.EndBpm;
                    var difBpm = endBpm - startBpm;
                    var phaseBeat = 0d;
                    var phaseMeasureBeat = 4d * phase.Length.Beat / phase.Length.NoteType;
                    var phaseTotalBeat = phaseMeasureBeat * phase.Length.Measure;
                    var nextPhaseBeat = phaseMeasureBeat;
                    for (var i = 0; i < phase.Length.Measure; i++)
                    {
                        while (phaseBeat < nextPhaseBeat)
                        {
                            var currentBpm = startBpm + difBpm * (phaseBeat / phaseTotalBeat);
                            virtualTime += 60d / currentBpm * deltaBeat;
                            phaseBeat += deltaBeat;
                        }

                        result.Add(new MornBeatMeasure(result.Count + 1, (long)(virtualTime * frequency)));
                        nextPhaseBeat += phaseMeasureBeat;
                    }
                }

                _music.MeasureList.Clear();
                _music.MeasureList.AddRange(result);
                if (isSuccess)
                {
                    MornBeatGlobal.Log("Generate Success");
                }
                else
                {
                    MornBeatGlobal.LogError("Generate Failed. Please check the log.");
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawGenerateFromTimeStamp()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_timeListSerializedProperty);
            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                MornBeatGlobal.Log("Generate Start");
                var result = new List<MornBeatMeasure>();
                result.Add(new MornBeatMeasure(1, 0));
                var frequency = _music.Clip.frequency;
                for (var i = 0; i < _music.TimeList.Count; i++)
                {
                    var time = _music.TimeList[i];
                    result.Add(new MornBeatMeasure(2 + i, (long)(time * frequency)));
                }

                _music.MeasureList.Clear();
                _music.MeasureList.AddRange(result);
                MornBeatGlobal.Log("Generate End");
            }

            EditorGUI.indentLevel--;
        }
    }
#endif
}
