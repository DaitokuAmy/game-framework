using System;
using System.Collections;
using GameFramework.CollisionSystem;
using UnityEngine;

namespace GameFramework.ProjectileSystem {
    /// <summary>
    /// 飛翔体オブジェクトの挙動拡張用インターフェース
    /// </summary>
    public interface IBulletProjectileComponent : IDisposable {
        /// <summary>
        /// 再生速度の変更
        /// </summary>
        /// <param name="speed">1.0を基準とした速度</param>
        void SetSpeed(float speed);

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        void Play(IBulletProjectileController projectileController);

        /// <summary>
        /// 飛翔更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void Tick(float deltaTime);

        /// <summary>
        /// 飛翔終了コルーチン
        /// </summary>
        IEnumerator StopRoutine();

        /// <summary>
        /// 衝突発生通知
        /// </summary>
        /// <param name="hit">衝突結果</param>
        void OnHitCollision(RaycastHit hit);
    }

    /// <summary>
    /// 飛翔体オブジェクトの挙動拡張用の基底MonoBehaviour
    /// </summary>
    [RequireComponent(typeof(BulletProjectile))]
    public abstract class BulletProjectileComponent : MonoBehaviour, IBulletProjectileComponent {
        /// <summary>使用中のProjectile</summary>
        protected IBulletProjectileController ProjectileController { get; private set; }

        /// <inheritdoc/>
        void IDisposable.Dispose() {
            DisposeInternal();
        }

        /// <inheritdoc/>
        void IBulletProjectileComponent.SetSpeed(float speed) {
            SetSpeedInternal(speed);
        }

        /// <inheritdoc/>
        void IBulletProjectileComponent.Play(IBulletProjectileController projectileController) {
            ProjectileController = projectileController;
            PlayInternal();
        }

        /// <inheritdoc/>
        void IBulletProjectileComponent.Tick(float deltaTime) {
            TickInternal(deltaTime);
        }

        /// <inheritdoc/>
        IEnumerator IBulletProjectileComponent.StopRoutine() {
            yield return StopRoutineInternal();
        }

        /// <inheritdoc/>
        void IBulletProjectileComponent.OnHitCollision(RaycastHit hit) {
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
        protected virtual void PlayInternal() {
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
        protected virtual IEnumerator StopRoutineInternal() {
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
