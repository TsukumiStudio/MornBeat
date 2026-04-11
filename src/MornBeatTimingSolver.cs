using System;
using UniRx;
using UnityEngine;

namespace MornLib
{
    public class MornBeatTimingSolver
    {
        private MornBeatMusic _music;
        private double _currentBpm = 120;
        private int _tick;
        private bool _waitLoop;
        private double _loopStartDspTime;
        private double _startDspTime;
        private double _offsetTime;
        private double _pausingTime;
        private double _pauseOffset;
        private readonly Subject<MornBeatTimingInfo> _beatSubject = new();
        private readonly Subject<Unit> _loopSubject = new();
        private readonly Subject<Unit> _endBeatSubject = new();
        public float SpeedScale => (float)_currentBpm / 120f;
        public double CurrentBpm => _currentBpm;
        public float BeatLengthF => (float)(60d / CurrentBpm);
        public double CurrentBeatLength => 60d / CurrentBpm;
        public double StartDspTime => _startDspTime;
        /// <summary> ループ時に0から初期化（単位：秒）</summary>
        public double MusicPlayingTime => AudioSettings.dspTime
                                          - _loopStartDspTime
                                          + _offsetTime
                                          - _pauseOffset
                                          - _pausingTime
                                          + (_music != null ? _music.Offset : 0);
        /// <summary> ループ後に値を継続（単位：秒）</summary>
        public double MusicPlayingTimeNoRepeat => AudioSettings.dspTime
                                                  - _startDspTime
                                                  + _offsetTime
                                                  - _pauseOffset
                                                  - _pausingTime
                                                  + (_music != null ? _music.Offset : 0);
        /// <summary> ループ時に0から初期化（単位：拍）</summary>
        public double MusicBeatTime => MusicPlayingTime / CurrentBeatLength;
        /// <summary> ループ後に値を継続（単位：拍）</summary>
        public double MusicBeatTimeNoRepeat => MusicPlayingTimeNoRepeat / CurrentBeatLength;
        public double OffsetTime => _offsetTime;
        public IObservable<MornBeatTimingInfo> OnBeat => _beatSubject;
        public IObservable<Unit> OnLoop => _loopSubject;
        public IObservable<Unit> OnEndBeat => _endBeatSubject;

        internal void SetMusic(MornBeatSetInfo setInfo)
        {
            _music = setInfo.Music;
            _tick = 0;
            _waitLoop = false;
            _startDspTime = setInfo.StartDspTime;
            _loopStartDspTime = _startDspTime;
            _pauseOffset = 0;
            _pausingTime = 0;
            _currentBpm = setInfo.Music.GetBpm(0);
        }

        internal void Reset()
        {
            _music = null;
            _tick = 0;
            _waitLoop = false;
            _startDspTime = AudioSettings.dspTime;
            _loopStartDspTime = _startDspTime;
            _currentBpm = 120;
            _pauseOffset = 0;
            _pausingTime = 0;
        }

        internal void SetOffsetTime(double offsetTime)
        {
            _offsetTime = offsetTime;
        }

        internal void UpdateBeat()
        {
            if (_music == null)
            {
                return;
            }
            
            var time = MusicPlayingTime;
            if (_waitLoop)
            {
                if (time >= _music.TotalLength)
                {
                    _loopStartDspTime += _music.LoopLength;
                    time -= _music.LoopLength;
                    _loopSubject.OnNext(Unit.Default);
                    _waitLoop = false;
                }
                else
                {
                    return;
                }
            }

            if (time < _music.GetBeatTiming(_tick))
            {
                return;
            }

            _currentBpm = _music.GetBpm(time);
            _beatSubject.OnNext(new MornBeatTimingInfo(_tick, _music.MeasureTickCount));
            _tick++;
            if (_tick == _music.TotalTickSum)
            {
                if (_music.IsLoop)
                {
                    _tick = _music.IntroTickSum;
                }

                _waitLoop = true;
                _endBeatSubject.OnNext(Unit.Default);
            }
        }

        internal int GetNearTick(out double nearDif)
        {
            if (_music == null)
            {
                nearDif = double.MaxValue;
                return -1;
            }
            
            var preTick = _tick;
            var nexTick = preTick + 1;
            var preTime = _music.GetBeatTiming(preTick);
            var nexTime = _music.GetBeatTiming(nexTick);
            var curTime = MusicPlayingTime;

            // preTimeが現在時刻より手前に来るよう調整する
            while (curTime < preTime && preTick - 1 >= 0)
            {
                preTick -= 1;
                nexTick -= 1;
                preTime = _music.GetBeatTiming(preTick);
                nexTime = _music.GetBeatTiming(nexTick);
            }

            // nexTimeが現在時刻より後に来るよう調整する
            while (nexTime < curTime && nexTick + 1 < _music.TotalTickSum)
            {
                preTick += 1;
                nexTick += 1;
                preTime = _music.GetBeatTiming(preTick);
                nexTime = _music.GetBeatTiming(nexTick);
            }

            var prevIsCloser = curTime < (preTime + nexTime) / 2f;
            var aimTime = prevIsCloser ? preTime : nexTime;
            var aimTick = prevIsCloser ? preTick : nexTick;
            nearDif = aimTime - curTime;
            return aimTick;
        }

        internal void UpdatePausing(double pausingTime)
        {
            _pausingTime = pausingTime;
        }

        internal void EndPausing(double pausingTime)
        {
            _pausingTime = 0;
            _pauseOffset += pausingTime;
        }
    }
}