using System.Threading;
using System;
#if USE_MORNSTATE || USE_ARBOR
#if USE_MORNSTATE
using MornLib;
#elif USE_ARBOR
using Arbor;
#endif
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
#if USE_MORNSTATE
    internal class BeatStopState : MornStateBehaviour
#elif USE_ARBOR
    internal class BeatStopState : StateBehaviour
#endif
    {
        [SerializeField] private StateLink _onComplete;
        [SerializeField] private float _stopDuration;
        [SerializeField] private bool _isIsolate;
        [Inject] private MornBeatController _beatController;

        public override async void OnStateBegin()
        {
            CancellationToken? ct = _isIsolate ? null : CancellationTokenOnEnd;
            await _beatController.StopBeatAsync(_stopDuration, ct);
            Transition(_onComplete);
        }
    }
}
#endif // USE_MORNSTATE || USE_ARBOR
