using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIAnimation再生用クラス
    /// </summary>
    public class UIAnimationPlayer : IDisposable {
        /// <summary>
        /// Animation再生制御用ハンドル
        /// </summary>
        public struct Handle : IDisposable, IEventProcess {
            /// <summary>空のHandle</summary>
            public static readonly Handle Empty = new Handle();

            private readonly PlayingInfo _playingInfo;

            /// <summary>有効なハンドルか</summary>
            public bool IsValid => _playingInfo != null && _playingInfo.IsValid;
            /// <summary>再生完了しているか</summary>
            public bool IsFinished => _playingInfo != null && _playingInfo.IsFinished;

            /// <summary>未使用</summary>
            object IEnumerator.Current => null;
            /// <summary>未使用</summary>
            Exception IProcess.Exception => null;
            /// <summary>完了チェック</summary>
            bool IProcess.IsDone => _playingInfo == null || IsFinished;
            
            /// <summary>終了通知</summary>
            event Action IEventProcess.ExitEvent {
                add {
                    if (_playingInfo == null) {
                        return;
                    }

                    _playingInfo.FinishedEvent += value;
                }
                remove {
                    if (_playingInfo == null) {
                        return;
                    }

                    _playingInfo.FinishedEvent -= value;
                }
            }
            
            /// <summary>終了通知</summary>
            public event Action FinishedEvent {
                add {
                    if (_playingInfo == null) {
                        return;
                    }

                    _playingInfo.FinishedEvent += value;
                }
                remove {
                    if (_playingInfo == null) {
                        return;
                    }

                    _playingInfo.FinishedEvent -= value;
                }
            }
        
            /// <inheritdoc/>
            public EventProcessAwaiter GetAwaiter() {
                return new EventProcessAwaiter(this);
            }

            /// <summary>
            /// アニメーションスキップ
            /// </summary>
            public void Skip() {
                if (!IsValid) {
                    return;
                }

                _playingInfo.Skip();
            }

            /// <summary>
            /// 廃棄処理
            /// </summary>
            public void Dispose() {
                if (!IsValid) {
                    return;
                }

                _playingInfo.Stop();
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(PlayingInfo playingInfo) {
                _playingInfo = playingInfo;
            }

            /// <summary>
            /// IEnumerator用
            /// </summary>
            bool IEnumerator.MoveNext() {
                return !((IProcess)this).IsDone;
            }

            /// <summary>
            /// 未使用
            /// </summary>
            void IEnumerator.Reset() {
            }
        }

        /// <summary>
        /// 再生中情報
        /// </summary>
        internal class PlayingInfo {
            public IUIAnimation Animation;
            public bool First = true;
            public float Time;
            public bool Reverse;
            public bool Loop;

            /// <summary>有効なデータか</summary>
            public bool IsValid => Animation != null;
            /// <summary>終了しているか</summary>
            public bool IsFinished => !IsValid || (!Loop && (Reverse ? Time <= 0.0f : Time >= Animation.Duration));

            /// <summary>終了通知</summary>
            public event Action FinishedEvent;

            /// <summary>
            /// 即時終了
            /// </summary>
            public void Skip() {
                if (IsFinished) {
                    return;
                }

                Time = Reverse ? 0.0f : Animation.Duration - 0.001f;
                Loop = false;
                Apply();
                Animation = null;
            }

            /// <summary>
            /// 停止
            /// </summary>
            public void Stop() {
                if (!IsValid) {
                    return;
                }

                Animation = null;
                FinishedEvent?.Invoke();
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public bool Update(float deltaTime) {
                if (!IsValid) {
                    return false;
                }

                // 初回は再生開始を通知
                if (First) {
                    Animation.OnPlay();
                    First = false;
                }

                var duration = Animation.Duration;
                Time += Reverse ? -deltaTime : deltaTime;
                if (Loop) {
                    Time = Mathf.Repeat(Time, duration);
                }
                else {
                    Time = Mathf.Clamp(Time, 0.0f, duration);
                }

                return !IsFinished;
            }

            /// <summary>
            /// アニメーションの反映
            /// </summary>
            public void Apply() {
                if (!IsValid) {
                    return;
                }

                // 初回は再生開始を通知
                if (First) {
                    Animation.OnPlay();
                    First = false;
                }

                Animation.SetTime(Time);
            }
        }

        // 再生情報リスト
        private readonly List<PlayingInfo> _playingInfos = new();
        // 再生情報削除管理用リスト
        private readonly List<int> _removePlayerInfoIndices = new();

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            foreach (var info in _playingInfos) {
                info.Stop();
            }

            _playingInfos.Clear();
            _removePlayerInfoIndices.Clear();
        }

        /// <summary>
        /// 再生
        /// </summary>
        /// <param name="animation">アニメーションインターフェース</param>
        /// <param name="reverse">逆再生</param>
        /// <param name="loop">ループ再生か</param>
        /// <param name="startOffset">開始時間オフセット</param>
        public Handle Play(IUIAnimation animation, bool reverse = false, bool loop = false, float startOffset = 0.0f) {
            if (animation == null) {
                return Handle.Empty;
            }

            // 同じUIAnimationがいたらそれは停止
            for (var i = 0; i < _playingInfos.Count; i++) {
                if (_playingInfos[i].Animation != animation) {
                    continue;
                }

                _playingInfos[i].Stop();
            }

            // 再生情報を構築してリストに登録
            var playingInfo = new PlayingInfo {
                Animation = animation,
                Time = Mathf.Clamp(reverse ? animation.Duration - startOffset : startOffset, 0.0f, animation.Duration),
                Reverse = reverse,
                Loop = loop
            };

            _playingInfos.Add(playingInfo);

            // ヘッドを調整
            playingInfo.Apply();

            return new Handle(playingInfo);
        }

        /// <summary>
        /// アニメーションの停止
        /// </summary>
        public void Stop() {
            foreach (var info in _playingInfos) {
                info.Stop();
            }

            _playingInfos.Clear();
            _removePlayerInfoIndices.Clear();
        }

        /// <summary>
        /// アニメーションの最終フレームに設定する
        /// </summary>
        /// <param name="animation">アニメーションインターフェース</param>
        /// <param name="reverse">逆再生</param>
        public void Skip(IUIAnimation animation, bool reverse = false) {
            if (animation == null) {
                return;
            }

            // 同じUIAnimationがいたらそれは停止
            for (var i = 0; i < _playingInfos.Count; i++) {
                if (_playingInfos[i].Animation != animation) {
                    continue;
                }

                _playingInfos[i].Stop();
            }

            // 再生情報を構築してリストに登録
            var playingInfo = new PlayingInfo {
                Animation = animation,
                Time = reverse ? animation.Duration : 0.0f,
                Reverse = reverse
            };

            playingInfo.Skip();
        }

        /// <summary>
        /// アニメーションの最終フレームに設定する
        /// </summary>
        public void Skip() {
            foreach (var info in _playingInfos) {
                info.Skip();
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void Update(float deltaTime) {
            for (var i = 0; i < _playingInfos.Count; i++) {
                var playingInfo = _playingInfos[i];

                // アニメーション時間更新
                var playing = playingInfo.Update(deltaTime);

                // アニメーション反映
                playingInfo.Apply();

                // 完了していた場合は停止して削除リストへ
                if (!playing) {
                    playingInfo.Stop();
                    _removePlayerInfoIndices.Add(i);
                }
            }

            // 不要なAnimationをリストから除外
            for (var i = _removePlayerInfoIndices.Count - 1; i >= 0; i--) {
                _playingInfos.RemoveAt(_removePlayerInfoIndices[i]);
            }

            _removePlayerInfoIndices.Clear();
        }
    }
}