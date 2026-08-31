using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace MornLib
{
    [Serializable]
    internal class MornBeatAudioSourceModule
    {
        [SerializeField] private AudioMixerGroup _mixerGroup;
        private readonly List<MornBeatIntroLoopAudioSource> _audioSources = new();
        private MornBeatIntroLoopAudioSource _current;
        private const int AudioSourceSetCount = 4;

        public void Initialize(GameObject owner)
        {
            _audioSources.Clear();
            for (var i = 0; i < AudioSourceSetCount; i++)
            {
                _audioSources.Add(CreateChild(owner, ((char)('A' + i)).ToString()));
            }

            _current = _audioSources[0];
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
            var result = _current;
            if (changeSource)
            {
                _current = GetReusableOther();
            }

            return result;
        }

        public MornBeatIntroLoopAudioSource GetOther(bool changeSource = false)
        {
            var result = GetReusableOther();
            if (changeSource)
            {
                _current = result;
            }

            return result;
        }

        public void SetCurrent(MornBeatIntroLoopAudioSource source)
        {
            _current = source;
        }

        private MornBeatIntroLoopAudioSource GetReusableOther()
        {
            var reusable = _audioSources.FirstOrDefault(x => x != _current && x.CanReuse);
            if (reusable != null)
            {
                return reusable;
            }

            return _audioSources.First(x => x != _current);
        }
    }
}
