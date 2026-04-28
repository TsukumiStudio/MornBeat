using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornBeat
{
    public static class MornBeatUtil
    {
        internal const char OpenSplit = '[';
        internal const char CloseSplit = ']';

        public static MornBeatMemoSo Create(
            AudioClip clip,
            List<float> timingList,
            List<double> bpms,
            List<double> bpmTimes,
            int measureTickCount = 8,
            int beatCount = 4,
            float volume = 1f,
            float offset = 0f,
            float loopAdditionalTime = 0f,
            bool isLoop = false,
            int introTickSum = 0,
            AudioClip introClip = null)
        {
            var memo = ScriptableObject.CreateInstance<MornBeatMemoSo>();
            memo.InitForRuntime(
                clip,
                introClip,
                isLoop,
                introTickSum,
                measureTickCount,
                beatCount,
                volume,
                offset,
                loopAdditionalTime,
                timingList,
                bpms,
                bpmTimes);
            return memo;
        }

        internal static double InverseLerp(double a, double b, double value)
        {
            var dif = b - a;
            return (value - a) / dif;
        }

        internal static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        public async static UniTask LoadAsync(this AudioClip clip, CancellationToken ct = default)
        {
            if (clip == null)
            {
                return;
            }

            if (clip.preloadAudioData || clip.loadState == AudioDataLoadState.Loaded)
            {
                MornBeatGlobal.Log($"ロード済み！: {clip.name}");
                return;
            }

            MornBeatGlobal.Log($"ロード開始...: {clip.name}");
            clip.LoadAudioData();
            while (clip.loadState != AudioDataLoadState.Loaded)
            {
                await UniTask.Yield(cancellationToken: ct);
            }

            MornBeatGlobal.Log($"ロード完了！: {clip.name}");
        }

        public async static UniTask UnloadAsync(this AudioClip clip, CancellationToken ct = default)
        {
            if (clip == null)
            {
                return;
            }

            if (clip.preloadAudioData || clip.loadState == AudioDataLoadState.Unloaded)
            {
                MornBeatGlobal.Log($"アンロード不要！: {clip.name}");
                return;
            }

            MornBeatGlobal.Log($"アンロード開始...: {clip.name}");
            clip.UnloadAudioData();
            while (clip.loadState != AudioDataLoadState.Unloaded)
            {
                await UniTask.Yield(cancellationToken: ct);
            }

            MornBeatGlobal.Log($"アンロード完了！: {clip.name}");
        }

        public static bool BitHas(this int self, int flag)
        {
            return (self & flag) != 0;
        }

        public static bool BitEqual(this int self, int flag)
        {
            return (self & flag) == flag;
        }

        public static int BitAdd(this int self, int flag)
        {
            return self | flag;
        }

        public static int BitRemove(this int self, int flag)
        {
            return self & ~flag;
        }

        public static int BitXor(this int self, int flag)
        {
            return self ^ flag;
        }
    }
}