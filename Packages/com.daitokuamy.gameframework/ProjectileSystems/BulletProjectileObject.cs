using System;
using System.Collections;
using System.Linq;
using GameFramework.CollisionSystems;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体オブジェクト用インターフェース
    /// </summary>
    public interface IBulletProjectileObject : IDisposable {
        /// <summary>Transformへの参照</summary>
        Transform transform { get; }

        /// <summary>
        /// 再生中か
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// レイキャスト用の半径（0より大きいとSphereCast）
        /// </summary>
        float RaycastRadius { get; }

        /// <summary>
        /// 再生速度の変更
        /// </summary>
        /// <param name="speed">1.0を基準とした速度</param>
        void SetSpeed(float speed);

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

        private readonly CoroutineRunner _coroutineRunner = new();

        private IBulletProjectileComponent[] _projectileComponents = Array.Empty<IBulletProjectileComponent>();
        private bool _isPlaying;

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

            _coroutineRunner.Dispose();
            foreach (var component in _projectileComponents) {
                component.Dispose();
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// 再生速度の変更
        /// </summary>
        /// <param name="speed">1.0を基準とした速度</param>
        void IBulletProjectileObject.SetSpeed(float speed) {
            SetSpeedInternal(speed);
            foreach (var component in _projectileComponents) {
                component.SetSpeed(speed);
            }
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

            _isPlaying = true;
            Projectile = projectile;
            StartProjectileInternal();
            foreach (var component in _projectileComponents) {
                component.Start(projectile);
            }

            ((IBulletProjectileObject)this).UpdateProjectile(projectile);
        }

        /// <summary>
        /// Projectileの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBulletProjectileObject.Update(float deltaTime) {
            _coroutineRunner.Update();
            foreach (var component in _projectileComponents) {
                component.Update(deltaTime);
            }
        }

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        void IBulletProjectileObject.Exit() {
            if (!_isPlaying) {
                return;
            }

            IEnumerator Routine() {
                var list = _projectileComponents.Select(x => x.ExitRoutine())
                    .Concat(new[] { ExitProjectileRoutine() });
                yield return new MergedCoroutine(list);
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

            UpdateTransformInternal(projectile);
            foreach (var component in _projectileComponents) {
                component.UpdateProjectile(projectile);
            }
        }

        /// <summary>
        /// コリジョンヒット時通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        void IBulletProjectileObject.OnHitCollision(RaycastHitResult result) {
            OnHitCollisionInternal(result);
            foreach (var component in _projectileComponents) {
                component.OnHitCollision(result);
            }
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

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _projectileComponents = gameObject.GetComponentsInChildren<IBulletProjectileComponent>();
        }
    }
}