using System.Threading;

namespace MornLib
{
    public struct MornBeatStartInfo
    {
        public MornBeatMusic Music;
        public double? StartDspTime;
        public float? FadeDuration;
        public bool? IsForceInitialize;
        public CancellationToken Ct;
        
    }
}