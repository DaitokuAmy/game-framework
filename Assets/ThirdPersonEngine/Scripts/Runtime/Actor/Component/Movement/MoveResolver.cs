using System;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 移動解決用クラス基底
    /// </summary>
    public abstract class MoveResolver : IMoveResolver {
        private bool _isMoving;
        private Vector3 _velocity;

        /// <summary>移動中か</summary>
        bool IMoveResolver.IsMoving => _isMoving;
        /// <summary>移動速度</summary>
        Vector3 IMoveResolver.Velocity => _velocity;

        /// <summary>距離誤差</summary>
        public float Tolerance { get; set; } = 0.01f;
        /// <summary>距離誤差の累乗</summary>
        protected float TolerancePow2 => Tolerance * Tolerance;
        
        /// <summary>制御対象のアクター</summary>
        protected IMovableActor Actor { get; private set; }
        
        /// <summary>移動終了イベント(boolは達成したか)</summary>
        public event Action<bool> EndMoveEvent;

        void IMoveResolver.Initialize(IMovableActor actor) {
            Actor = actor;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void IMoveResolver.Update(float deltaTime, float speedMultiplier) {
            var prevPos = Actor.GetPosition();
            UpdateInternal(deltaTime, speedMultiplier);
            var nextPos = Actor.GetPosition();
            if (_isMoving && deltaTime > float.Epsilon) {
                _velocity = (nextPos - prevPos) / deltaTime;
            }
        }

        /// <summary>
        /// 移動キャンセル
        /// </summary>
        public void Cancel() {
            if (!_isMoving) {
                return;
            }
            
            CancelInternal();
            EndMove(false);
        }

        /// <summary>
        /// 移動スキップ
        /// </summary>
        public void Skip() {
            if (!_isMoving) {
                return;
            }
            
            SkipInternal();
            EndMove(true);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected virtual void UpdateInternal(float deltaTime, float speedMultiplayer) {
        }

        /// <summary>
        /// 移動キャンセル
        /// </summary>
        protected virtual void CancelInternal() {
        }

        /// <summary>
        /// 移動スキップ
        /// </summary>
        protected virtual void SkipInternal() {
        }
        
        /// <summary>
        /// 移動の開始
        /// </summary>
        protected void StartMove() {
            if (_isMoving) {
                return;
            }

            _isMoving = true;
        }
        
        /// <summary>
        /// 移動の終了
        /// </summary>
        protected void EndMove(bool completed) {
            if (!_isMoving) {
                return;
            }

            _isMoving = false;
            _velocity = Vector3.zero;
            EndMoveEvent?.Invoke(completed);
            EndMoveEvent = null;
        }

        /// <summary>
        /// 距離を計算
        /// </summary>
        protected float GetDistance(Vector3 point, bool ignoreY = true) {
            var vector = point - Actor.GetPosition();
            if (ignoreY) {
                vector.y = 0.0f;
            }

            return vector.magnitude;
        }

        /// <summary>
        /// 距離の二乗を計算
        /// </summary>
        protected float GetSqrDistance(Vector3 point, bool ignoreY = true) {
            var vector = point - Actor.GetPosition();
            if (ignoreY) {
                vector.y = 0.0f;
            }

            var sqrDistance = vector.sqrMagnitude;
            if (float.IsNaN(sqrDistance)) {
                return 0.0f;
            }

            return sqrDistance;
        }
    }
}
