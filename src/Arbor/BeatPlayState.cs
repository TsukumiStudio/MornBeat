#if USE_ARBOR || USE_MORNSTATE
#if USE_MORNSTATE
using MornLib;
using StateLink = MornLib.StateLink;
#else
using Arbor;
#endif
using System;
using UnityEngine;
using VContainer;

namespace MornLib
{
    [Serializable]
#if USE_MORNSTATE
    internal class BeatPlayState : MornStateBehaviour
#else
    internal class BeatPlayState : StateBehaviour
#endif
    {
        [SerializeField] private MornBeatMusic _music;
        [SerializeField] private bool _executeIsolated;
        [SerializeField] private StateLink _onComplete;
        [Inject] private MornBeatController _beatController;

        public override async void OnStateBegin()
        {
            var ct = _executeIsolated ? Application.exitCancellationToken : CancellationTokenOnEnd;
            await _beatController.StartAsync(new MornBeatStartInfo { Music = _music, Ct = ct, });
            Transition(_onComplete);
        }
    }
}
#endif
