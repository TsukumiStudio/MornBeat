using MornLib;
using UnityEngine;

namespace MornBeat
{
    internal sealed class MornBeatGlobal : MornGlobalPureBase<MornBeatGlobal>
    {
        protected override string ModuleName => nameof(MornBeat);

        internal static void Log(string message)
        {
            Logger.Log(message);
        }

        internal static void LogWarning(string message)
        {
            Logger.LogWarning(message);
        }

        internal static void LogError(string message)
        {
            Logger.LogError(message);
        }

        internal static void SetDirty(Object obj)
        {
            MornGlobalUtil.SetDirty(obj);
        }

        internal static void LogAndSetDirty(string message, Object obj)
        {
            Log(message);
            SetDirty(obj);
        }
    }
}