#if USE_ARBOR
using Arbor;
using UnityEngine;
using VContainer;

namespace MornLib
{
    internal class BeatPlayState : StateBehaviour
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
