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
        void Start(IBulletProjectileController projectileController);

        /// <summary>
        /// 飛翔更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void Update(float deltaTime);

        /// <summary>
        /// 飛翔終了コルーチン
        /// </summary>
        IEnumerator ExitRoutine();

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
        void IBulletProjectileComponent.Start(IBulletProjectileController projectileController) {
            ProjectileController = projectileController;
            StartInternal();
        }

        /// <inheritdoc/>
        void IBulletProjectileComponent.Update(float deltaTime) {
            UpdateInternal(deltaTime);
        }

        /// <inheritdoc/>
        IEnumerator IBulletProjectileComponent.ExitRoutine() {
            yield return ExitRoutineInternal();
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
        protected virtual void StartInternal() {
        }

        /// <summary>
        /// 飛翔更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(float deltaTime) {
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
