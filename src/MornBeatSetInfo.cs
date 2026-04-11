namespace MornLib
{
    public readonly struct MornBeatSetInfo
    {
        public readonly MornBeatMusic Music;
        public readonly double StartDspTime;

        public MornBeatSetInfo(MornBeatMusic beatMemo, double startDspTime)
        {
            Music = beatMemo;
            StartDspTime = startDspTime;
        }
    }
}