using UnityEngine;

namespace MornLib
{
    [CreateAssetMenu(menuName = "Morn/Beat/" + nameof(MornBeatGlobal))]
    internal sealed class MornBeatGlobal : MornGlobalBase<MornBeatGlobal>
    {
        protected override string ModuleName => "MornBeat";

        [SerializeField] internal MornBeatScaleSettings DefaultScaleSettings;
    }
}
