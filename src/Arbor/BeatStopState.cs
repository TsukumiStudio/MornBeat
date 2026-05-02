#if USE_ARBOR || USE_MORNSTATE
using System.Threading;
using System;
#if USE_MORNSTATE
using MornLib;
using StateLink = MornLib.Connection;
#else
using Arbor;
#endif
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
#if USE_MORNSTATE
    internal class BeatStopState : MornStateBehaviour
#else
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
#endif
