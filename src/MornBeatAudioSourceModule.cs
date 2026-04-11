using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MornLib
{
    [Serializable]
    internal class MornBeatAudioSourceModule
    {
        [SerializeField] private AudioMixerGroup _mixerGroup;
        private bool _isUsingAudioSourceA;
        private MornBeatIntroLoopAudioSource _audioSourceA;
        private MornBeatIntroLoopAudioSource _audioSourceB;

        public void Initialize(GameObject owner)
        {
            _audioSourceA = CreateChild(owner, "A");
            _audioSourceB = CreateChild(owner, "B");
        }

        private MornBeatIntroLoopAudioSource CreateChild(GameObject owner, string label)
        {
            var intro = CreateAudioSource(owner, $"MornBeat_{label}_Intro");
            var loop = CreateAudioSource(owner, $"MornBeat_{label}_Loop");
            return new MornBeatIntroLoopAudioSource(intro, loop);
        }

        private AudioSource CreateAudioSource(GameObject owner, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(owner.transform);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = _mixerGroup;
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
