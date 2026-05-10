using System.Threading;
using System;
#if !USE_ARBOR
using MornLib;
#else
using Arbor;
#endif
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
#if !USE_ARBOR
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
