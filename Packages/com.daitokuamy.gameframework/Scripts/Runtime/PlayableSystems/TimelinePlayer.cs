using System;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using GameFramework.Core;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// タイムライン再生するためのPlayer
    /// </summary>
    public class TimelinePlayer {
        /// <summary>
        /// 再生制御用ハンドル
        /// </summary>
        public readonly struct Handle : IEventProcess {
            public static readonly Handle Empty = new();

            private readonly PlayingInfo _playingInfo;

            /// <inheritdoc/>
            object IEnumerator.Current => null;
            /// <inheritdoc/>
            bool IProcess.IsDone => _playingInfo == null || _playingInfo.IsDone;
            /// <inheritdoc/>
            Exception IProcess.Exception => null;

            /// <summary>終了通知</summary>
            event Action IEventProcess.ExitEvent {
                add {
                    if (_playingInfo != null) {
                        _playingInfo.StopEvent += value;
                    }
                }
                remove {
                    if (_playingInfo != null) {
                        _playingInfo.StopEvent -= value;
                    }
                }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(PlayingInfo info) {
                _playingInfo = info;
            }

            /// <summary>
            /// 継続チェック
            /// </summary>
            bool IEnumerator.MoveNext() {
                return !((IProcess)this).IsDone;
            }

            /// <summary>
            /// 未使用
            /// </summary>
            void IEnumerator.Reset() {
            }
        
            /// <inheritdoc/>
            public EventProcessAwaiter GetAwaiter() {
                return new EventProcessAwaiter(this);
            }
        }

        /// <summary>
        /// 再生中情報
        /// </summary>
        internal class PlayingInfo {
            private readonly PlayableDirector _playableDirector;

            private bool _stopped;

            /// <summary>完了しているか</summary>
            public bool IsDone => _stopped;
            /// <summary>停止通知</summary>
            public event Action StopEvent;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public PlayingInfo(PlayableDirector playableDirector) {
                _playableDirector = playableDirector;
                _playableDirector.stopped += OnStopped;
            }

            /// <summary>
            /// 停止時処理
            /// </summary>
            private void OnStopped(PlayableDirector playableDirector) {
                _stopped = true;
                _playableDirector.stopped -= OnStopped;
                StopEvent?.Invoke();
                StopEvent = null;
            }
        }

        private readonly PlayableDirector _playableDirector;

        private PlayingInfo _playingInfo;
        private float _speed = 1.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TimelinePlayer(PlayableDirector playableDirector, DirectorUpdateMode updateMode) {
            _playableDirector = playableDirector;
            _playableDirector.playOnAwake = false;
            _playableDirector.timeUpdateMode = updateMode;
            _playableDirector.time = 0.0f;
            _playableDirector.Stop();
        }

        /// <summary>
        /// マニュアル更新処理
        /// </summary>
        public void ManualUpdate(float deltaTime) {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) {
                if (_playableDirector.state == PlayState.Playing) {
                    _playableDirector.time += deltaTime;
                    _playableDirector.Evaluate();
                }
            }
        }

        /// <summary>
        /// 再生
        /// </summary>
        public Handle Play(TimelineAsset timelineAsset, bool loop = false) {
            if (timelineAsset == null) {
                return Handle.Empty;
            }

            _playableDirector.Stop();
            _playingInfo = new PlayingInfo(_playableDirector);
            _playableDirector.Play(timelineAsset, loop ? DirectorWrapMode.Loop : DirectorWrapMode.None);
            _playableDirector.Evaluate();
            SetSpeed(_speed);
            return new Handle(_playingInfo);
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop() {
            _playableDirector.Stop();
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        public void SetSpeed(float speed) {
            _speed = speed;

            if (!_playableDirector.playableGraph.IsValid()) {
                return;
            }

            _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }
    }
}