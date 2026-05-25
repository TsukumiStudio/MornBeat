#if USE_MORNSTATE || USE_ARBOR
using System;
#if USE_MORNSTATE
using MornLib;
#elif USE_ARBOR
using Arbor;
#endif
using UniRx;
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
    [MornStateMenu("Beat")]
#if USE_MORNSTATE
    internal class MornBeatAnimationState : MornStateBehaviour
#elif USE_ARBOR
    internal class MornBeatAnimationState : StateBehaviour
#endif
    {
        [Inject] private readonly MornBeatController _beatController;
        [SerializeField] private BindAnimatorClip _bind;
        [SerializeField] private int _perBeat = 4;
        [SerializeField] private int _offsetTick;
        [SerializeField] private float _transition;
        [SerializeField] private bool _playOnStateBegin;
        [NonSerialized] private IDisposable _beatSubscription;

        public override void OnStateBegin()
        {
            _beatSubscription?.Dispose();
            if (_playOnStateBegin)
            {
                PlayAnimation();
            }

            _beatSubscription = _beatController.PlayModule.OnBeat
                .Where(IsTargetBeat)
                .Subscribe(_ => PlayAnimation());
        }

        public override void OnStateEnd()
        {
            _beatSubscription?.Dispose();
            _beatSubscription = null;
        }

        private bool IsTargetBeat(MornBeatTimingInfo info)
        {
            if (_perBeat <= 0)
            {
                return false;
            }

            var tickInterval = info.TickCountPerMeasure / _perBeat;
            return tickInterval > 0 && (info.CurrentTick + _offsetTick) % tickInterval == 0;
        }

        private void PlayAnimation()
        {
            _bind.Play(_transition);
        }

        protected override void OnValidate()
        {
            _perBeat = Mathf.Max(1, _perBeat);
        }
    }
}
#endif // USE_MORNSTATE || USE_ARBOR
