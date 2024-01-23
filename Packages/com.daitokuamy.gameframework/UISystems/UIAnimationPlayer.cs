using System;
using System.Collections;
using System.Collections.Generic;
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
        public struct Handle : IDisposable, IProcess {
            /// <summary>空のHandle</summary>
            public static readonly Handle Empty = new Handle();

            private PlayingInfo _playingInfo;

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
            public IUIAnimation animation;
            public bool first = true;
            public float time;
            public bool reverse;
            public bool loop;

            /// <summary>有効なデータか</summary>
            public bool IsValid => animation != null;
            /// <summary>終了しているか</summary>
            public bool IsFinished => !IsValid || (!loop && (reverse ? time <= 0.0f : time >= animation.Duration));

            /// <summary>
            /// 即時終了
            /// </summary>
            public void Skip() {
                if (IsFinished) {
                    return;
                }

                time = reverse ? 0.0f : animation.Duration;
                loop = false;
                Apply();
                animation = null;
            }

            /// <summary>
            /// 停止
            /// </summary>
            public void Stop() {
                animation = null;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public bool Update(float deltaTime) {
                if (!IsValid) {
                    return false;
                }

                // 初回は再生開始を通知
                if (first) {
                    animation.OnPlay();
                    first = false;
                }

                var duration = animation.Duration;
                time += reverse ? -deltaTime : deltaTime;
                if (loop) {
                    time = Mathf.Repeat(time, duration);
                }
                else {
                    time = Mathf.Clamp(time, 0.0f, duration);
                }

                return true;
            }

            /// <summary>
            /// アニメーションの反映
            /// </summary>
            public void Apply() {
                if (!IsValid) {
                    return;
                }

                // 初回は再生開始を通知
                if (first) {
                    animation.OnPlay();
                    first = false;
                }

                animation.SetTime(time);
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
        public Handle Play(IUIAnimation animation, bool reverse = false, bool loop = false) {
            if (animation == null) {
                return Handle.Empty;
            }

            // 同じUIAnimationがいたらそれは停止
            for (var i = 0; i < _playingInfos.Count; i++) {
                if (_playingInfos[i].animation != animation) {
                    continue;
                }

                _playingInfos[i].Stop();
            }

            // 再生情報を構築してリストに登録
            var playingInfo = new PlayingInfo {
                animation = animation,
                time = reverse ? animation.Duration : 0.0f,
                reverse = reverse,
                loop = loop
            };

            _playingInfos.Add(playingInfo);

            // ヘッドを調整
            playingInfo.Apply();

            return new Handle(playingInfo);
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
                if (_playingInfos[i].animation != animation) {
                    continue;
                }

                _playingInfos[i].Stop();
            }

            // 再生情報を構築してリストに登録
            var playingInfo = new PlayingInfo {
                animation = animation,
                time = reverse ? animation.Duration : 0.0f,
                reverse = reverse
            };
            
            playingInfo.Skip();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void Update(float deltaTime) {
            for (var i = 0; i < _playingInfos.Count; i++) {
                var playingInfo = _playingInfos[i];

                // アニメーション時間更新
                if (!playingInfo.Update(deltaTime)) {
                    // 完了していた場合は削除リストへ移す
                    _removePlayerInfoIndices.Add(i);
                }

                // アニメーション反映
                playingInfo.Apply();
            }

            // 不要なAnimationをリストから除外
            for (var i = _removePlayerInfoIndices.Count - 1; i >= 0; i--) {
                _playingInfos.RemoveAt(_removePlayerInfoIndices[i]);
            }

            _removePlayerInfoIndices.Clear();
        }
    }
}