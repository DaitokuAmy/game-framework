using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.ProjectileSystem {
    /// <summary>
    /// 飛翔体再生用クラス
    /// </summary>
    public class ProjectilePlayer : IDisposable {
        /// <summary>
        /// 飛翔ハンドル
        /// </summary>
        public readonly struct Handle : IEventProcess, IDisposable {
            private readonly ProjectilePlayer _player;
            private readonly int _handleId;

            /// <inheritdoc/>
            object IEnumerator.Current => null;

            /// <inheritdoc/>
            Exception IProcess.Exception => null;

            /// <inheritdoc/>
            bool IProcess.IsDone => !TryGetPlayingInfo(out var playingInfo) || playingInfo.IsDone;

            /// <inheritdoc/>
            event Action IEventProcess.ExitEvent {
                add {
                    if (TryGetPlayingInfo(out var playingInfo)) {
                        playingInfo.ExitEvent += value;
                    }
                }
                remove {
                    if (TryGetPlayingInfo(out var playingInfo)) {
                        playingInfo.ExitEvent -= value;
                    }
                }
            }

            /// <summary>有効なハンドルか</summary>
            public bool IsValid => TryGetPlayingInfo(out var playingInfo) && playingInfo.IsValid;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(ProjectilePlayer player, int handleId) {
                _player = player;
                _handleId = handleId;
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (!IsValid) {
                    return;
                }

                _player.DisposeHandle(_handleId);
            }

            /// <inheritdoc/>
            bool IEnumerator.MoveNext() {
                return !((IProcess)this).IsDone;
            }

            /// <inheritdoc/>
            void IEnumerator.Reset() {
            }
        
            /// <inheritdoc/>
            public EventProcessAwaiter GetAwaiter() {
                return new EventProcessAwaiter(this);
            }

            /// <summary>
            /// コリジョン判定に使うレイを取得
            /// </summary>
            public (Ray ray, float distance) GetCollisionRay() {
                if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.IsValid) {
                    return default;
                }
                
                return playingInfo.GetCollisionRay();
            }

            /// <summary>
            /// 衝突処理
            /// </summary>
            public void Hit(RaycastHit hit) {
                if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.IsValid) {
                    return;
                }

                playingInfo.Hit(hit);
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            public void Stop(Vector3? stopPosition = null) {
                if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.IsValid) {
                    return;
                }

                playingInfo.Stop(stopPosition);
            }

            /// <summary>
            /// 再生情報の取得
            /// </summary>
            private bool TryGetPlayingInfo(out PlayingInfo playingInfo) {
                if (_player == null) {
                    playingInfo = null;
                    return false;
                }

                return _player.TryGetPlayingInfo(_handleId, out playingInfo);
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
            public int HandleId { get; set; }

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
            protected abstract void PlayInternal(IProjectileController projectileController);

            /// <summary>
            /// 更新処理
            /// </summary>
            protected abstract bool TickInternal(float deltaTime);

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
            /// 即時停止処理
            /// </summary>
            protected abstract void StopImmediateInternal();

            /// <summary>
            /// タイムスケールの変更
            /// </summary>
            protected abstract void ChangedTimeScaleInternal(float timeScale);

            /// <summary>
            /// 開始処理
            /// </summary>
            public void Play(IProjectileController projectileController) {
                if (CurrentState >= State.Started) {
                    return;
                }

                if (LayeredTime != null) {
                    LayeredTime.ChangedTimeScaleEvent += ChangedTimeScaleInternal;
                }

                projectileController.Play();
                PlayInternal(projectileController);
                CurrentState = State.Started;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public bool Tick() {
                var deltaTime = LayeredTime?.DeltaTime ?? Time.deltaTime;
                if (!ProjectileController.Tick(deltaTime)) {
                    Stop(null);
                }

                return TickInternal(deltaTime);
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
            /// 即時停止処理
            /// </summary>
            public void StopImmediate(Vector3? stopPosition) {
                if (CurrentState >= State.Stopped) {
                    return;
                }

                if (CurrentState < State.Stopping) {
                    ProjectileController.Stop(stopPosition);
                    CurrentState = State.Stopping;
                }

                StopImmediateInternal();
                Stopped();
            }

            /// <summary>
            /// 停止完了
            /// </summary>
            public void Stopped() {
                if (CurrentState >= State.Stopped) {
                    return;
                }

                if (LayeredTime != null) {
                    LayeredTime.ChangedTimeScaleEvent -= ChangedTimeScaleInternal;
                }

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

            /// <inheritdoc/>
            protected override bool TickInternal(float deltaTime) {
                var prevPos = Projectile.transform.position;
                Projectile.Tick(deltaTime);
                var nextPos = Projectile.transform.position;
                
                // コリジョン情報更新
                _collisionDistance = Vector3.Distance(prevPos, nextPos);
                _collisionRay.origin = prevPos;
                _collisionRay.direction = nextPos - prevPos;
                return Projectile.IsPlaying;
            }

            /// <inheritdoc/>
            protected override void PlayInternal(IProjectileController projectileController) {
                Projectile.Play((IBulletProjectileController)projectileController);
                
                // コリジョン情報更新
                var pos = Projectile.transform.position;
                _collisionRay = new Ray(pos, pos);
            }

            /// <inheritdoc/>
            protected override (Ray, float) GetCollisionRayInternal() {
                return (_collisionRay, _collisionDistance);
            }

            /// <inheritdoc/>
            protected override void HitInternal(RaycastHit hit) {
                Projectile.OnHitCollision(hit);
            }

            /// <inheritdoc/>
            protected override void StopInternal() {
                Projectile.Stop();
                
                // コリジョン情報更新
                var nextPos = Projectile.transform.position;
                _collisionDistance = Vector3.Distance(_collisionRay.origin, nextPos);
                _collisionRay.direction = nextPos - _collisionRay.origin;
            }

            /// <inheritdoc/>
            protected override void StopImmediateInternal() {
                Projectile.StopImmediate();

                var nextPos = Projectile.transform.position;
                _collisionDistance = Vector3.Distance(_collisionRay.origin, nextPos);
                _collisionRay.direction = nextPos - _collisionRay.origin;
            }

            /// <inheritdoc/>
            protected override void StoppedInternal() {
                StoppedEvent?.Invoke(Projectile);
                StoppedEvent = null;
            }

            /// <inheritdoc/>
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

            /// <inheritdoc/>
            protected override bool TickInternal(float deltaTime) {
                Projectile.Tick(deltaTime);

                // コリジョン情報更新
                var controller = Projectile.Controller;
                _collisionRay.origin = controller.TailPosition;
                _collisionRay.direction = controller.HeadPosition - controller.TailPosition;
                _collisionDistance = controller.Distance;
                
                return Projectile.IsPlaying;
            }

            /// <inheritdoc/>
            protected override void PlayInternal(IProjectileController projectileController) {
                var controller = (IBeamProjectileController)projectileController;
                Projectile.Play(controller);

                // コリジョン情報更新
                _collisionRay.origin = controller.TailPosition;
                _collisionRay.direction = controller.HeadPosition - controller.TailPosition;
                _collisionDistance = controller.Distance;
            }

            /// <inheritdoc/>
            protected override (Ray, float) GetCollisionRayInternal() {
                return (_collisionRay, _collisionDistance);
            }

            /// <inheritdoc/>
            protected override void HitInternal(RaycastHit hit) {
                Projectile.OnHitCollision(hit);
            }

            /// <inheritdoc/>
            protected override void StopInternal() {
                Projectile.Stop();

                // コリジョン情報更新
                var controller = Projectile.Controller;
                _collisionRay.origin = controller.TailPosition;
                _collisionRay.direction = controller.HeadPosition - controller.TailPosition;
                _collisionDistance = controller.Distance;
            }

            /// <inheritdoc/>
            protected override void StopImmediateInternal() {
                var controller = Projectile.Controller;
                _collisionRay.origin = controller.TailPosition;
                _collisionRay.direction = controller.HeadPosition - controller.TailPosition;
                _collisionDistance = controller.Distance;
                Projectile.StopImmediate();
            }

            /// <inheritdoc/>
            protected override void StoppedInternal() {
                StoppedEvent?.Invoke(Projectile);
                StoppedEvent = null;
            }

            /// <inheritdoc/>
            protected override void ChangedTimeScaleInternal(float timeScale) {
                Projectile.SetSpeed(timeScale);
            }
        }

        private readonly List<PlayingInfo> _playingInfos = new();
        private readonly List<PlayingInfo> _removePlayingInfos = new();
        private readonly Dictionary<int, PlayingInfo> _playingInfoMap = new();
        private int _nextHandleId = 1;

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
        public void Tick() {
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
                HandleId = _nextHandleId++,
                Projectile = projectile,
                LayeredTime = layeredTime,
            };
            playingInfo.StoppedEvent += onStopped;
            _playingInfos.Add(playingInfo);
            _playingInfoMap.Add(playingInfo.HandleId, playingInfo);
            playingInfo.Play(projectileController);

            var handle = new Handle(this, playingInfo.HandleId);
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
                HandleId = _nextHandleId++,
                Projectile = projectile,
                LayeredTime = layeredTime
            };
            playingInfo.StoppedEvent += onStopped;
            _playingInfos.Add(playingInfo);
            _playingInfoMap.Add(playingInfo.HandleId, playingInfo);
            playingInfo.Play(projectileController);

            var handle = new Handle(this, playingInfo.HandleId);
            return handle;
        }

        /// <summary>
        /// 全飛翔体の停止
        /// </summary>
        /// <param name="clear">即時クリア</param>
        public void StopAll(bool clear = false) {
            for (var i = _playingInfos.Count - 1; i >= 0; i--) {
                var info = _playingInfos[i];
                if (clear) {
                    UnregisterPlayingInfo(info);
                    info.StopImmediate(null);
                    continue;
                }

                info.Stop(null);
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
                UnregisterPlayingInfo(playingInfo);
                playingInfo.Stopped();
            }

            _removePlayingInfos.Clear();

            // 更新処理
            for (var i = 0; i < _playingInfos.Count; i++) {
                var playingInfo = _playingInfos[i];
                // 更新処理
                if (!playingInfo.Tick()) {
                    // 完了終了リストに追加(コリジョン判定などもあるので、1frame遅れて消す)
                    _removePlayingInfos.Add(playingInfo);
                }
            }
        }

        /// <summary>
        /// 再生情報の取得
        /// </summary>
        private bool TryGetPlayingInfo(int handleId, out PlayingInfo playingInfo) {
            if (handleId <= 0) {
                playingInfo = null;
                return false;
            }

            return _playingInfoMap.TryGetValue(handleId, out playingInfo);
        }

        /// <summary>
        /// Handle経由の廃棄
        /// </summary>
        private void DisposeHandle(int handleId) {
            if (!TryGetPlayingInfo(handleId, out var playingInfo)) {
                return;
            }

            UnregisterPlayingInfo(playingInfo);
            playingInfo.StopImmediate(null);
        }

        /// <summary>
        /// 再生情報の管理解除
        /// </summary>
        private void UnregisterPlayingInfo(PlayingInfo playingInfo) {
            if (playingInfo == null) {
                return;
            }

            _playingInfoMap.Remove(playingInfo.HandleId);
            _playingInfos.Remove(playingInfo);
            _removePlayingInfos.Remove(playingInfo);
        }
    }
}
