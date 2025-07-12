using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体再生用クラス
    /// </summary>
    public class ProjectilePlayer : IDisposable {
        /// <summary>
        /// 飛翔ハンドル
        /// </summary>
        public readonly struct Handle : IEventProcess, IDisposable {
            private readonly PlayingInfo _playingInfo;

            /// <inheritdoc/>
            object IEnumerator.Current => null;

            /// <inheritdoc/>
            Exception IProcess.Exception => null;

            /// <inheritdoc/>
            bool IProcess.IsDone => _playingInfo?.IsDone ?? true;

            /// <inheritdoc/>
            event Action IEventProcess.ExitEvent {
                add {
                    if (_playingInfo != null) {
                        _playingInfo.ExitEvent += value;
                    }
                }
                remove {
                    if (_playingInfo != null) {
                        _playingInfo.ExitEvent -= value;
                    }
                }
            }

            /// <summary>有効なハンドルか</summary>
            public bool IsValid => _playingInfo?.IsValid ?? false;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(PlayingInfo info) {
                _playingInfo = info;
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (!IsValid) {
                    return;
                }

                _playingInfo.Stop(null);
                _playingInfo.Stopped();
            }

            /// <inheritdoc/>
            bool IEnumerator.MoveNext() {
                return !((IProcess)this).IsDone;
            }

            /// <inheritdoc/>
            void IEnumerator.Reset() {
            }

            /// <summary>
            /// コリジョン判定に使うレイを取得
            /// </summary>
            public (Ray ray, float distance) GetCollisionRay() {
                if (!IsValid) {
                    return default;
                }
                
                return _playingInfo.GetCollisionRay();
            }

            /// <summary>
            /// 衝突処理
            /// </summary>
            public void Hit(RaycastHit hit) {
                if (!IsValid) {
                    return;
                }

                _playingInfo.Hit(hit);
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            public void Stop(Vector3? stopPosition = null) {
                if (!IsValid) {
                    return;
                }

                _playingInfo.Stop(stopPosition);
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

            public State CurrentState;
            public LayeredTime LayeredTime;

            /// <summary>飛翔情報</summary>
            public abstract IProjectileController ProjectileController { get; }

            /// <summary>有効か</summary>
            public bool IsValid => ProjectileController != null;

            /// <summary>完了</summary>
            public bool IsDone => !IsValid || CurrentState == State.Stopped;

            /// <summary>終了通知</summary>
            public event Action ExitEvent;

            /// <summary>
            /// 開始処理
            /// </summary>
            protected abstract void StartInternal(IProjectileController projectileController);

            /// <summary>
            /// 更新処理
            /// </summary>
            protected abstract bool UpdateInternal(float deltaTime);

            /// <summary>
            /// 当たり判定用レイの取得
            /// </summary>
            protected abstract (Ray, float) GetCollisionRayInternal();

            /// <summary>
            /// ヒット処理
            /// </summary>
            protected abstract void HitInternal(RaycastHit hit);

            /// <summary>
            /// 停止処理
            /// </summary>
            protected abstract void StopInternal();

            /// <summary>
            /// 完全停止処理
            /// </summary>
            protected abstract void StoppedInternal();

            /// <summary>
            /// タイムスケールの変更
            /// </summary>
            protected abstract void ChangedTimeScaleInternal(float timeScale);

            /// <summary>
            /// 開始処理
            /// </summary>
            public void Start(IProjectileController projectileController) {
                if (CurrentState >= State.Started) {
                    return;
                }

                LayeredTime.ChangedTimeScaleEvent += ChangedTimeScaleInternal;

                projectileController.Start();
                StartInternal(projectileController);
                CurrentState = State.Started;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public bool Update() {
                var deltaTime = LayeredTime?.DeltaTime ?? Time.deltaTime;
                if (!ProjectileController.Update(deltaTime)) {
                    Stop(null);
                }

                return UpdateInternal(deltaTime);
            }

            /// <summary>
            /// コリジョン判定に使うレイを取得
            /// </summary>
            public (Ray, float) GetCollisionRay() {
                if (CurrentState < State.Started) {
                    return default;
                }

                return GetCollisionRayInternal();
            }

            /// <summary>
            /// 衝突処理
            /// </summary>
            public void Hit(RaycastHit hit) {
                if (CurrentState != State.Started) {
                    return;
                }

                HitInternal(hit);
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            public void Stop(Vector3? stopPosition) {
                if (CurrentState >= State.Stopping) {
                    return;
                }

                ProjectileController.Stop(stopPosition);
                StopInternal();
                CurrentState = State.Stopping;
            }

            /// <summary>
            /// 停止完了
            /// </summary>
            public void Stopped() {
                if (CurrentState >= State.Stopped) {
                    return;
                }

                LayeredTime.ChangedTimeScaleEvent -= ChangedTimeScaleInternal;
                ExitEvent?.Invoke();
                ExitEvent = null;
                StoppedInternal();
                CurrentState = State.Stopped;
            }
        }

        /// <summary>
        /// 再生情報(弾)
        /// </summary>
        internal class BulletPlayingInfo : PlayingInfo {
            public IBulletProjectile Projectile;

            private Ray _collisionRay;
            private float _collisionDistance;

            /// <summary>停止完了イベント</summary>
            public event Action<IBulletProjectile> StoppedEvent;

            /// <summary>飛翔情報</summary>
            public override IProjectileController ProjectileController => Projectile?.Controller;

            /// <summary>
            /// 更新処理
            /// </summary>
            protected override bool UpdateInternal(float deltaTime) {
                var prevPos = Projectile.transform.position;
                Projectile.Update(deltaTime);
                var nextPos = Projectile.transform.position;
                
                // コリジョン情報更新
                _collisionDistance = Vector3.Distance(prevPos, nextPos);
                _collisionRay.origin = prevPos;
                _collisionRay.direction = nextPos - prevPos;
                return Projectile.IsPlaying;
            }

            /// <summary>
            /// 開始処理
            /// </summary>
            protected override void StartInternal(IProjectileController projectileController) {
                Projectile.Start((IBulletProjectileController)projectileController);
                
                // コリジョン情報更新
                var pos = Projectile.transform.position;
                _collisionRay = new Ray(pos, pos);
            }

            /// <summary>
            /// 当たり判定用レイの取得
            /// </summary>
            protected override (Ray, float) GetCollisionRayInternal() {
                return (_collisionRay, _collisionDistance);
            }

            /// <summary>
            /// ヒット処理
            /// </summary>
            protected override void HitInternal(RaycastHit hit) {
                Projectile.OnHitCollision(hit);
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            protected override void StopInternal() {
                Projectile.Exit();
                
                // コリジョン情報更新
                var nextPos = Projectile.transform.position;
                _collisionDistance = Vector3.Distance(_collisionRay.origin, nextPos);
                _collisionRay.direction = nextPos - _collisionRay.origin;
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            protected override void StoppedInternal() {
                StoppedEvent?.Invoke(Projectile);
                StoppedEvent = null;
            }

            /// <summary>
            /// タイムスケールの変更
            /// </summary>
            protected override void ChangedTimeScaleInternal(float timeScale) {
                Projectile.SetSpeed(timeScale);
            }
        }

        /// <summary>
        /// 再生情報(ビーム)
        /// </summary>
        internal class BeamPlayingInfo : PlayingInfo {
            public IBeamProjectile Projectile;
            
            private Ray _collisionRay;
            private float _collisionDistance;

            /// <summary>停止完了イベント</summary>
            public event Action<IBeamProjectile> StoppedEvent;

            /// <summary>飛翔情報</summary>
            public override IProjectileController ProjectileController => Projectile?.Controller;

            /// <summary>
            /// 更新処理
            /// </summary>
            protected override bool UpdateInternal(float deltaTime) {
                Projectile.Update(deltaTime);

                // コリジョン情報更新
                var controller = Projectile.Controller;
                _collisionRay.origin = controller.TailPosition;
                _collisionRay.direction = controller.HeadPosition - controller.TailPosition;
                _collisionDistance = controller.Distance;
                
                return Projectile.IsPlaying;
            }

            /// <summary>
            /// 開始処理
            /// </summary>
            protected override void StartInternal(IProjectileController projectileController) {
                var controller = (IBeamProjectileController)projectileController;
                Projectile.Start(controller);

                // コリジョン情報更新
                _collisionRay.origin = controller.TailPosition;
                _collisionRay.direction = controller.HeadPosition - controller.TailPosition;
                _collisionDistance = controller.Distance;
            }

            /// <summary>
            /// 当たり判定用レイの取得
            /// </summary>
            protected override (Ray, float) GetCollisionRayInternal() {
                return (_collisionRay, _collisionDistance);
            }

            /// <summary>
            /// ヒット処理
            /// </summary>
            protected override void HitInternal(RaycastHit hit) {
                Projectile.OnHitCollision(hit);
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            protected override void StopInternal() {
                Projectile.Exit();

                // コリジョン情報更新
                var controller = Projectile.Controller;
                _collisionRay.origin = controller.TailPosition;
                _collisionRay.direction = controller.HeadPosition - controller.TailPosition;
                _collisionDistance = controller.Distance;
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            protected override void StoppedInternal() {
                StoppedEvent?.Invoke(Projectile);
                StoppedEvent = null;
            }

            /// <summary>
            /// タイムスケールの変更
            /// </summary>
            protected override void ChangedTimeScaleInternal(float timeScale) {
                Projectile.SetSpeed(timeScale);
            }
        }

        private readonly List<PlayingInfo> _playingInfos = new();
        private readonly List<PlayingInfo> _removePlayingInfos = new();

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
        public void Update() {
            UpdatePlayingInfos();
        }

        /// <summary>
        /// 飛翔体の開始
        /// </summary>
        /// <param name="projectile">飛翔体インスタンス</param>
        /// <param name="projectileController">飛翔情報</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="onStopped">停止完了処理</param>
        public Handle Play(
            IBulletProjectile projectile,
            IBulletProjectileController projectileController,
            LayeredTime layeredTime,
            Action<IBulletProjectile> onStopped) {
            var playingInfo = new BulletPlayingInfo {
                Projectile = projectile,
                LayeredTime = layeredTime,
            };
            playingInfo.StoppedEvent += onStopped;
            _playingInfos.Add(playingInfo);
            playingInfo.Start(projectileController);

            var handle = new Handle(playingInfo);
            return handle;
        }

        /// <summary>
        /// 飛翔体の開始
        /// </summary>
        /// <param name="projectile">飛翔体インスタンス</param>
        /// <param name="projectileController">飛翔情報</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="onStopped">停止完了処理</param>
        public Handle Play(
            IBeamProjectile projectile,
            IBeamProjectileController projectileController,
            LayeredTime layeredTime,
            Action<IBeamProjectile> onStopped) {
            var playingInfo = new BeamPlayingInfo {
                Projectile = projectile,
                LayeredTime = layeredTime
            };
            playingInfo.StoppedEvent += onStopped;
            _playingInfos.Add(playingInfo);
            playingInfo.Start(projectileController);

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
                info.Stop(null);
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