using UnityEngine;

namespace MornLib
{
    internal sealed class MornBeatGlobal : MornGlobalPureBase<MornBeatGlobal>
    {
        protected override string ModuleName => "MornBeat";

        private static MornGlobalLogger _logger;
        internal static MornGlobalLogger Logger => _logger ??= new MornGlobalLogger(I);

        [SerializeField] internal MornBeatScaleSettings DefaultScaleSettings;

        internal static void SetDirty(Object obj)
        {
            I.SetDirtyInternal(obj);
        }
    }
}
