using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体再生用クラス
    /// </summary>
    public class ProjectilePlayer : IDisposable {
        /// <summary>
        /// 飛翔ハンドル
        /// </summary>
        public struct Handle {
            private PlayingInfo _playingInfo;

            /// <summary>有効なハンドルか</summary>
            public bool IsValid => _playingInfo != null && _playingInfo.IsValid;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(PlayingInfo info) {
                _playingInfo = info;
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            public void Stop() {
                if (!IsValid) {
                    _playingInfo = null;
                    return;
                }
                
                _playingInfo.Stop();
                _playingInfo = null;
            }
        }

        /// <summary>
        /// 再生情報
        /// </summary>
        internal abstract class PlayingInfo {
            public enum State {
                Standby,
                Started,
                Stopping,
                Stopped,
            }

            public IProjectile projectile;
            public State state;
            public LayeredTime layeredTime;
            
            public Action onStopped;

            /// <summary>有効か</summary>
            public bool IsValid => projectile != null && state != State.Stopped;

            /// <summary>
            /// 開始処理
            /// </summary>
            public void Start() {
                if (state >= State.Started) {
                    return;
                }

                projectile?.Start();
                state = State.Started;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public bool Update() {
                var deltaTime = layeredTime != null ? layeredTime.DeltaTime : Time.deltaTime;
                var continuation = projectile.Update(deltaTime);
                continuation &= UpdateInternal(deltaTime);
                return continuation;
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            public void Stop() {
                if (state >= State.Stopping) {
                    return;
                }

                projectile.Stop();
                state = State.Stopping;
            }

            /// <summary>
            /// 停止完了
            /// </summary>
            public void Stopped() {
                if (state >= State.Stopped) {
                    return;
                }
                
                onStopped?.Invoke();
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            protected abstract bool UpdateInternal(float deltaTime);
        }

        /// <summary>
        /// 再生情報(弾)
        /// </summary>
        internal class BulletPlayingInfo : PlayingInfo {
            public Action<IBulletProjectile> onUpdated;

            /// <summary>
            /// 更新処理
            /// </summary>
            protected override bool UpdateInternal(float deltaTime) {
                var bulletProjectile = (IBulletProjectile)projectile;
                onUpdated?.Invoke(bulletProjectile);
                return true;
            }
        }

        /// <summary>
        /// 再生情報(ビーム)
        /// </summary>
        internal class BeamPlayingInfo : PlayingInfo {
            public Action<IBeamProjectile> onUpdated;

            /// <summary>
            /// 更新処理
            /// </summary>
            protected override bool UpdateInternal(float deltaTime) {
                var beamProjectile = (IBeamProjectile)projectile;
                onUpdated?.Invoke(beamProjectile);
                return true;
            }
        }

        // 飛翔体再生情報リスト
        private List<PlayingInfo> _playingInfos = new List<PlayingInfo>();
        // リスト除外対象のワーク
        private List<PlayingInfo> _removePlayingInfos = new List<PlayingInfo>();

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // 全飛翔体を停止
            StopAll(true);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void Update() {
            UpdatePlayingInfos();
        }

        /// <summary>
        /// 飛翔体の開始
        /// </summary>
        /// <param name="projectile">飛翔体インスタンス</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="onUpdated">座標の更新通知</param>
        /// <param name="onStopped">飛翔完了通知</param>
        public Handle Play(IBulletProjectile projectile,
            LayeredTime layeredTime,
            Action<IBulletProjectile> onUpdated,
            Action onStopped) {
            var playingInfo = new BulletPlayingInfo {
                projectile = projectile,
                layeredTime = layeredTime,
                onUpdated = onUpdated,
                onStopped = onStopped
            };
            _playingInfos.Add(playingInfo);
            playingInfo.Start();

            var handle = new Handle(playingInfo);
            return handle;
        }

        /// <summary>
        /// 飛翔体の開始
        /// </summary>
        /// <param name="projectile">飛翔体インスタンス</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="onUpdated">座標の更新通知</param>
        /// <param name="onStopped">飛翔完了通知</param>
        public Handle Play(IBeamProjectile projectile,
            LayeredTime layeredTime,
            Action<IBeamProjectile> onUpdated,
            Action onStopped) {
            var playingInfo = new BeamPlayingInfo {
                projectile = projectile,
                layeredTime = layeredTime,
                onUpdated = onUpdated,
                onStopped = onStopped
            };
            _playingInfos.Add(playingInfo);
            playingInfo.Start();

            var handle = new Handle(playingInfo);
            return handle;
        }

        /// <summary>
        /// 全飛翔体の停止
        /// </summary>
        /// <param name="clear">即時クリア</param>
        public void StopAll(bool clear = false) {
            for (var i = 0; i < _playingInfos.Count; i++) {
                var info = _playingInfos[i];
                info.Stop();
                if (clear) {
                    info.Stopped();
                }
            }

            if (clear) {
                _playingInfos.Clear();
                _removePlayingInfos.Clear();
            }
        }

        /// <summary>
        /// Projectileの更新処理
        /// </summary>
        private void UpdatePlayingInfos() {
            // 不要なProjectileの再生情報をクリア
            for (var i = _removePlayingInfos.Count - 1; i >= 0; i--) {
                var playingInfo = _removePlayingInfos[i];
                playingInfo.Stopped();
                _playingInfos.Remove(playingInfo);
            }

            _removePlayingInfos.Clear();
            
            // 更新処理
            for (var i = 0; i < _playingInfos.Count; i++) {
                var playingInfo = _playingInfos[i];
                // 更新処理
                if (!playingInfo.Update()) {
                    // 完了終了リストに追加(コリジョン判定などもあるので、1frame遅れて消す)
                    _removePlayingInfos.Add(playingInfo);
                }
            }
        }
    }
}