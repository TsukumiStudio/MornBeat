using System;
using UniRx;
using UnityEngine;

namespace MornLib
{
    [Serializable]
    public sealed class MornBeatPlayModule
    {
        private readonly MornBeatTimingSolver _offsetTiming = new();
        private readonly MornBeatTimingSolver _pureTiming = new();
        private readonly Subject<MornBeatSetInfo> _initializeBeatSubject = new();
        private MornBeatMusic _music;
        private PreparationClockScope _preparationClock;
        public bool IsClockSuspendedForPreparation => _preparationClock != null;
        internal bool IsControllerPaused { get; set; }
        public MornBeatMusic Music => _music;
        public float SpeedScale => _offsetTiming.SpeedScale;
        public double CurrentBpm => _offsetTiming.CurrentBpm;
        public float BeatLengthF => _offsetTiming.BeatLengthF;
        public double CurrentBeatLength => _offsetTiming.CurrentBeatLength;
        public double StartDspTime => _offsetTiming.StartDspTime;
        public int BeatCount => _music?.BeatCount ?? 0;
        public int BeatTick => _music?.BeatTick ?? 0;
        public int MeasureTick => _music?.MeasureTickCount ?? 0;
        /// <summary> ループ時に0から初期化（単位：秒）</summary>
        public double MusicPlayingTime => _offsetTiming.MusicPlayingTime;
        /// <summary> ループ後に値を継続（単位：秒）</summary>
        public double MusicPlayingTimeNoRepeat => _offsetTiming.MusicPlayingTimeNoRepeat;
        /// <summary> ループ時に0から初期化（単位：拍）</summary>
        public double MusicBeatTime => _offsetTiming.MusicBeatTime;
        /// <summary> ループ後に値を継続（単位：拍）</summary>
        public double MusicBeatTimeNoRepeat => _offsetTiming.MusicBeatTimeNoRepeat;
        public double OffsetTime => _offsetTiming.OffsetTime;
        public IObservable<MornBeatSetInfo> OnInitializeBeat => _initializeBeatSubject;
        public IObservable<MornBeatTimingInfo> OnBeat => _offsetTiming.OnBeat;
        public IObservable<MornBeatTimingInfo> OnPureBeat => _pureTiming.OnBeat;
        public IObservable<Unit> OnLoop => _offsetTiming.OnLoop;
        public IObservable<Unit> OnEndBeat => _offsetTiming.OnEndBeat;

        /// <summary>
        /// 描画準備用に拍の進行を止めるスコープを返す (メインスレッド専用)。拍の通知を止めて DSP 時刻を固定するが、
        /// 音声の一時停止や破棄時の再生位置の復元はしない。Reset でもスコープは終わる。準備後は曲を設定し直してから再生する。
        /// 通常の Pause/Resume やコントローラーからの再生開始とは重ねられない
        /// </summary>
        public IDisposable SuspendClockForPreparation()
        {
            if (IsControllerPaused)
            {
                throw new InvalidOperationException("Cannot prepare a paused controller");
            }

            if (_preparationClock != null)
            {
                throw new InvalidOperationException("Preparation clock already suspended");
            }

            return _preparationClock = new PreparationClockScope(this);
        }

        internal void SetMusic(MornBeatSetInfo setInfo)
        {
            _music = setInfo.Music;
            _offsetTiming.SetMusic(setInfo);
            _pureTiming.SetMusic(setInfo);
            _initializeBeatSubject.OnNext(setInfo);
        }

        /// <summary>曲を設定し、指定した拍から刻みを始める (途中から再生する演出用)。描画準備中でも呼べる</summary>
        /// <param name="dspAtTick">譜面時刻 GetBeatTiming(tick) に対応するクリップ位置が鳴る DSP 時刻 (Offset 補正前)</param>
        public void SetMusicAt(MornBeatMusic music, int tick, double dspAtTick)
        {
            // SetMusic が一時停止のオフセットを初期化するため、再開時に時計が飛ぶ
            if (IsControllerPaused)
            {
                throw new InvalidOperationException("一時停止中は SetMusicAt を呼べません");
            }

            if (tick < 0 || music.TotalTickSum <= tick)
            {
                throw new ArgumentOutOfRangeException(nameof(tick), tick, "曲の範囲外の拍です");
            }

            // 通常再生と同じく、曲の先頭が鳴る時刻を開始時刻とする (Offset は MusicPlayingTime 側で加わる)
            var tickTime = music.GetBeatTiming(tick);
            SetMusic(new MornBeatSetInfo(music, dspAtTick - tickTime));
            var bpm = music.GetBpm(tickTime);
            _offsetTiming.SeekTick(tick, bpm);
            _pureTiming.SeekTick(tick, bpm);
        }

        internal void EnsureNotPreparing()
        {
            if (IsClockSuspendedForPreparation)
            {
                throw new InvalidOperationException("Normal playback/Pause/Resume cannot overlap preparation");
            }
        }

        internal void Reset()
        {
            _preparationClock?.Dispose();
            _music = null;
            _offsetTiming.Reset();
            _pureTiming.Reset();
        }

        internal void UpdateBeat()
        {
            if (IsClockSuspendedForPreparation)
            {
                return;
            }

            _offsetTiming.UpdateBeat();
            _pureTiming.UpdateBeat();
        }

        public void SetOffsetTime(double offsetTime)
        {
            _offsetTiming.SetOffsetTime(offsetTime);
            // _pureTiming.SetOffsetTime(offsetTime);
        }

        public float ConvertToTime(int tick)
        {
            if (_music == null)
            {
                return Mathf.Infinity;
            }

            return _music.GetBeatTiming(tick);
        }

        public int GetNearTick(out double nearDif)
        {
            return _offsetTiming.GetNearTick(out nearDif);
        }

        internal void UpdatePausing(double pausingTime)
        {
            _offsetTiming.UpdatePausing(pausingTime);
            _pureTiming.UpdatePausing(pausingTime);
        }

        internal void EndPausing(double pausingTime)
        {
            _offsetTiming.EndPausing(pausingTime);
            _pureTiming.EndPausing(pausingTime);
        }

        private sealed class PreparationClockScope : IDisposable
        {
            private MornBeatPlayModule _owner;

            public PreparationClockScope(MornBeatPlayModule owner)
            {
                _owner = owner;
                var dsp = AudioSettings.dspTime;
                owner._offsetTiming.PreparationDspTime = dsp;
                owner._pureTiming.PreparationDspTime = dsp;
            }

            public void Dispose()
            {
                if (_owner == null)
                {
                    return;
                }

                var owner = _owner;
                _owner = null;
                if (!ReferenceEquals(owner._preparationClock, this))
                {
                    return;
                }

                owner._offsetTiming.PreparationDspTime = null;
                owner._pureTiming.PreparationDspTime = null;
                owner._preparationClock = null;
            }
        }
    }
}
