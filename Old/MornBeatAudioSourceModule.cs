using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MornLib
{
    [Serializable]
    internal class MornBeatAudioSourceModule
    {
        [SerializeField] private AudioMixerGroup _mixerGroup;
        [ReadOnly] [SerializeField] private bool _isUsingAudioSourceA;
        private MornBeatIntroLoopAudioSource _audioSourceA;
        private MornBeatIntroLoopAudioSource _audioSourceB;

        public void Initialize(GameObject owner)
        {
            _audioSourceA = CreateChild(owner, "A");
            _audioSourceB = CreateChild(owner, "B");
        }

        private MornBeatIntroLoopAudioSource CreateChild(GameObject owner, string label)
        {
            var child = new GameObject($"MornBeat_{label}");
            child.transform.SetParent(owner.transform);
            var source = child.AddComponent<MornBeatIntroLoopAudioSource>();
            source.Initialize(_mixerGroup);
            return source;
        }

        public MornBeatIntroLoopAudioSource GetCurrent(bool changeSource = false)
        {
            var result = _isUsingAudioSourceA ? _audioSourceA : _audioSourceB;
            if (changeSource)
            {
                _isUsingAudioSourceA = !_isUsingAudioSourceA;
            }

            return result;
        }

        public MornBeatIntroLoopAudioSource GetOther(bool changeSource = false)
        {
            var result = _isUsingAudioSourceA ? _audioSourceB : _audioSourceA;
            if (changeSource)
            {
                _isUsingAudioSourceA = !_isUsingAudioSourceA;
            }

            return result;
        }
    }
}
