using System;
using System.Collections;
using GameFramework.CollisionSystems;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体オブジェクト用インターフェース
    /// </summary>
    public interface IBulletProjectileObject : IDisposable {
        /// <summary>
        /// 再生中か
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// レイキャスト用の半径（0より大きいとSphereCast）
        /// </summary>
        float RaycastRadius { get; }

        /// <summary>
        /// アクティブ状態の切り替え
        /// </summary>
        /// <param name="active">アクティブか</param>
        void SetActive(bool active);

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
        /// 飛翔終了処理
        /// </summary>
        void Exit();

        /// <summary>
        /// 飛翔物の更新
        /// </summary>
        void UpdateProjectile(IBulletProjectile projectile);

        /// <summary>
        /// スケールの設定
        /// </summary>
        /// <param name="scale">スケールの設定</param>
        void SetLocalScale(Vector3 scale);

        /// <summary>
        /// 座標の設定
        /// </summary>
        /// <param name="position">設定する座標</param>
        void SetPosition(Vector3 position);

        /// <summary>
        /// 衝突発生通知
        /// </summary>
        /// <param name="result">衝突結果</param>
        void OnHitCollision(RaycastHitResult result);
    }

    /// <summary>
    /// 飛翔体の実体制御用MonoBehaviour
    /// </summary>
    public class BulletProjectileObject : MonoBehaviour, IBulletProjectileObject {
        [SerializeField, Tooltip("レイキャスト用の半径(0より大きいとSphereRaycast)")]
        private float _raycastRadius = 0.0f;

        private bool _isPlaying;
        private CoroutineRunner _coroutineRunner = new CoroutineRunner();

        // 再生中か
        bool IBulletProjectileObject.IsPlaying => _isPlaying;
        // レイキャスト用の半径（0より大きいとSphereCast）
        float IBulletProjectileObject.RaycastRadius => _raycastRadius * transform.localScale.x;
        // 使用中のProjectile
        protected IProjectile Projectile { get; private set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            if (gameObject == null) {
                return;
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// アクティブ状態の切り替え
        /// </summary>
        void IBulletProjectileObject.SetActive(bool active) {
            if (gameObject == null) {
                return;
            }

            if (gameObject.activeSelf == active) {
                return;
            }

            gameObject.SetActive(active);
        }

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        void IBulletProjectileObject.Start(IBulletProjectile projectile) {
            if (_isPlaying) {
                return;
            }

            Projectile = projectile;
            ((IBulletProjectileObject)this).UpdateProjectile(projectile);
            _isPlaying = true;
            StartProjectileInternal();
        }

        /// <summary>
        /// Projectileの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBulletProjectileObject.Update(float deltaTime) {
            _coroutineRunner.Update();
        }

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        void IBulletProjectileObject.Exit() {
            if (!_isPlaying) {
                return;
            }

            IEnumerator Routine() {
                yield return ExitProjectileRoutine();
                Projectile = null;
                _isPlaying = false;
            }

            _coroutineRunner.StartCoroutine(Routine());
        }

        /// <summary>
        /// Transformの更新
        /// </summary>
        void IBulletProjectileObject.UpdateProjectile(IBulletProjectile projectile) {
            var trans = transform;
            trans.position = projectile.Position;
            trans.rotation = projectile.Rotation;
        }

        /// <summary>
        /// スケールの設定
        /// </summary>
        /// <param name="scale">スケール</param>
        void IBulletProjectileObject.SetLocalScale(Vector3 scale) {
            var trans = transform;
            trans.localScale = scale;
        }

        /// <summary>
        /// 座標の設定
        /// </summary>
        /// <param name="position">更新後の座標</param>
        void IBulletProjectileObject.SetPosition(Vector3 position) {
            var trans = transform;
            trans.position = position;
        }

        /// <summary>
        /// コリジョンヒット時通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        void IBulletProjectileObject.OnHitCollision(RaycastHitResult result) {
            OnHitCollision(result);
        }

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        protected virtual void StartProjectileInternal() {
        }

        /// <summary>
        /// 飛翔終了子ルーチン処理
        /// </summary>
        protected virtual IEnumerator ExitProjectileRoutine() {
            yield break;
        }

        /// <summary>
        /// コリジョンヒット通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        protected virtual void OnHitCollision(RaycastHitResult result) {
        }
    }
}