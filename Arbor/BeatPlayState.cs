#if USE_ARBOR
using Arbor;
using UnityEngine;
using VContainer;

namespace MornLib
{
    internal class BeatPlayState : StateBehaviour
    {
        [SerializeField] private MornBeatMemoSo _beatMemo;
        [SerializeField] private bool _executeIsolated;
        [SerializeField] private StateLink _onComplete;
        [Inject] private MornBeatControllerMono _beatController;

        public override async void OnStateBegin()
        {
            var ct = _executeIsolated ? MornApp.QuitToken : CancellationTokenOnEnd;
            await _beatController.StartAsync(new MornBeatStartInfo { BeatMemo = _beatMemo, Ct = ct, });
            Transition(_onComplete);
        }
    }
}
#endif