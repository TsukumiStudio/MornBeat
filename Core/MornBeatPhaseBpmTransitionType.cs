namespace MornLib
{
    /// <summary> フェーズのBpm遷移 </summary>
    internal enum MornBeatPhaseBpmTransitionType
    {
        /// <summary> 一定 </summary>
        Constant,
        /// <summary> 線形変化 </summary>
        Lerp,
    }
}