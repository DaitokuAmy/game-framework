using System;
using System.Collections;
using GameFramework.CollisionSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
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
        /// <param name="projectile">飛翔物の情報</param>
        void Start(IBeamProjectile projectile);

        /// <summary>
        /// 飛翔更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void Update(float deltaTime);

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        IEnumerator ExitRoutine();

        /// <summary>
        /// 飛翔物の更新
        /// </summary>
        /// <param name="projectile">飛翔物の情報</param>
        void UpdateProjectile(IBeamProjectile projectile);

        /// <summary>
        /// 衝突発生通知
        /// </summary>
        /// <param name="result">衝突結果</param>
        void OnHitCollision(RaycastHitResult result);
    }

    /// <summary>
    /// ビームオブジェクトの拡張用MonoBehaviour
    /// </summary>
    public abstract class BeamProjectileComponent : MonoBehaviour, IBeamProjectileComponent {
        /// <summary>使用中のProjectile</summary>
        protected IBeamProjectile Projectile { get; private set; }

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
        void IBeamProjectileComponent.SetSpeed(float speed) {
            SetSpeedInternal(speed);
        }

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        void IBeamProjectileComponent.Start(IBeamProjectile projectile) {
            Projectile = projectile;
            StartProjectileInternal();
        }

        /// <summary>
        /// Projectileの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBeamProjectileComponent.Update(float deltaTime) {
            UpdateInternal(deltaTime);
        }

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        IEnumerator IBeamProjectileComponent.ExitRoutine() {
            yield return ExitProjectileRoutine();
        }

        /// <summary>
        /// Transformの更新
        /// </summary>
        void IBeamProjectileComponent.UpdateProjectile(IBeamProjectile projectile) {
            UpdateTransformInternal(projectile);
        }

        /// <summary>
        /// コリジョンヒット時通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        void IBeamProjectileComponent.OnHitCollision(RaycastHitResult result) {
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
        protected virtual void UpdateTransformInternal(IBeamProjectile projectile) {
        }

        /// <summary>
        /// コリジョンヒット通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        protected virtual void OnHitCollisionInternal(RaycastHitResult result) {
        }
    }
}