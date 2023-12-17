using System;
using System.Collections;
using GameFramework.CollisionSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
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
        void Start(IBulletProjectile projectile);

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
        /// 飛翔物の更新
        /// </summary>
        /// <param name="projectile">飛翔物の情報</param>
        void UpdateProjectile(IBulletProjectile projectile);

        /// <summary>
        /// 衝突発生通知
        /// </summary>
        /// <param name="result">衝突結果</param>
        void OnHitCollision(RaycastHitResult result);
    }

    /// <summary>
    /// 飛翔体オブジェクトの挙動拡張用の基底MonoBehaviour
    /// </summary>
    [RequireComponent(typeof(BulletProjectileObject))]
    public abstract class BulletProjectileComponent : MonoBehaviour, IBulletProjectileComponent {
        // 使用中のProjectile
        protected IProjectile Projectile { get; private set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            DisposeInternal();
        }

        /// <summary>
        /// 再生速度の変更
        /// </summary>
        /// <param name="speed">1.0を基準とした速度</param>
        void IBulletProjectileComponent.SetSpeed(float speed) {
            SetSpeedInternal(speed);
        }

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        void IBulletProjectileComponent.Start(IBulletProjectile projectile) {
            Projectile = projectile;
            StartProjectileInternal();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBulletProjectileComponent.Update(float deltaTime) {
            UpdateInternal(deltaTime);
        }

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        IEnumerator IBulletProjectileComponent.ExitRoutine() {
            yield return ExitProjectileRoutine();
        }

        /// <summary>
        /// Transformの更新
        /// </summary>
        void IBulletProjectileComponent.UpdateProjectile(IBulletProjectile projectile) {
            UpdateTransformInternal(projectile);
        }

        /// <summary>
        /// コリジョンヒット時通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        void IBulletProjectileComponent.OnHitCollision(RaycastHitResult result) {
            OnHitCollisionInternal(result);
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
        protected virtual void StartProjectileInternal() {
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
        protected virtual IEnumerator ExitProjectileRoutine() {
            yield break;
        }
        
        /// <summary>
        /// 内部用Transform更新処理
        /// </summary>
        protected virtual void UpdateTransformInternal(IBulletProjectile projectile) {
        }

        /// <summary>
        /// コリジョンヒット通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        protected virtual void OnHitCollisionInternal(RaycastHitResult result) {
        }
    }
}