using System;
using System.Collections;
using GameFramework.CollisionSystem;
using UnityEngine;

namespace GameFramework.ProjectileSystem {
    /// <summary>
    /// ビームオブジェクト拡張用インターフェース
    /// </summary>
    public interface IBeamProjectileComponent : IDisposable {
        /// <summary>
        /// 再生速度の変更
        /// </summary>
        /// <param name="speed">1.0を基準とした速度</param>
        void SetSpeed(float speed);

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        /// <param name="projectileController">飛翔物の情報</param>
        void Start(IBeamProjectileController projectileController);

        /// <summary>
        /// 飛翔更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void Tick(float deltaTime);

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        IEnumerator ExitRoutine();

        /// <summary>
        /// 衝突発生通知
        /// </summary>
        /// <param name="hit">衝突結果</param>
        void OnHitCollision(RaycastHit hit);
    }

    /// <summary>
    /// ビームオブジェクトの拡張用MonoBehaviour
    /// </summary>
    [RequireComponent(typeof(BeamProjectile))]
    public abstract class BeamProjectileComponent : MonoBehaviour, IBeamProjectileComponent {
        /// <summary>使用中のProjectile</summary>
        protected IBeamProjectileController ProjectileController { get; private set; }

        /// <inheritdoc/>
        void IDisposable.Dispose() {
            DisposeInternal();
        }

        /// <inheritdoc/>
        void IBeamProjectileComponent.SetSpeed(float speed) {
            SetSpeedInternal(speed);
        }

        /// <inheritdoc/>
        void IBeamProjectileComponent.Start(IBeamProjectileController projectileController) {
            ProjectileController = projectileController;
            StartInternal();
        }

        /// <inheritdoc/>
        void IBeamProjectileComponent.Tick(float deltaTime) {
            TickInternal(deltaTime);
        }

        /// <inheritdoc/>
        IEnumerator IBeamProjectileComponent.ExitRoutine() {
            yield return ExitRoutineInternal();
        }

        /// <inheritdoc/>
        void IBeamProjectileComponent.OnHitCollision(RaycastHit hit) {
            OnHitCollisionInternal(hit);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 再生速度の変更
        /// </summary>
        /// <param name="speed">1.0を基準とした速度</param>
        protected virtual void SetSpeedInternal(float speed) {
        }

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        protected virtual void StartInternal() {
        }

        /// <summary>
        /// 飛翔更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void TickInternal(float deltaTime) {
        }

        /// <summary>
        /// 飛翔終了子ルーチン処理
        /// </summary>
        protected virtual IEnumerator ExitRoutineInternal() {
            yield break;
        }

        /// <summary>
        /// コリジョンヒット通知
        /// </summary>
        /// <param name="hit">当たり結果</param>
        protected virtual void OnHitCollisionInternal(RaycastHit hit) {
        }
    }
}
