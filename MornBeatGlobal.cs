using UnityEngine;

namespace MornLib
{
    internal sealed class MornBeatGlobal : MornGlobalPureBase<MornBeatGlobal>
    {
        protected override string ModuleName => "MornBeat";

        [SerializeField] internal MornBeatScaleSettings DefaultScaleSettings;

        internal static void SetDirty(Object obj)
        {
            I.SetDirtyInternal(obj);
        }
    }
}
