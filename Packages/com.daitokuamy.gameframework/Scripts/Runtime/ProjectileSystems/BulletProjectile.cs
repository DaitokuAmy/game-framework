using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体オブジェクト用インターフェース
    /// </summary>
    public interface IBulletProjectile : IDisposable {
        /// <summary>Transformへの参照</summary>
        Transform transform { get; }
        /// <summary>飛翔情報</summary>
        IBulletProjectileController Controller { get; }
        /// <summary>再生中か</summary>
        bool IsPlaying { get; }
        /// <summary>レイキャスト用の半径（0より大きいとSphereCast）</summary>
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
        void Start(IBulletProjectileController projectileController);

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
        /// スケールの設定
        /// </summary>
        /// <param name="scale">スケールの設定</param>
        void SetLocalScale(Vector3 scale);

        /// <summary>
        /// 衝突発生通知
        /// </summary>
        /// <param name="hit">衝突結果</param>
        void OnHitCollision(RaycastHit hit);
    }

    /// <summary>
    /// 飛翔体の実体制御用MonoBehaviour
    /// </summary>
    public class BulletProjectile : MonoBehaviour, IBulletProjectile {
        [SerializeField, Tooltip("レイキャスト用の半径(0より大きいとSphereRaycast)")]
        private float _raycastRadius = 0.0f;

        private readonly CoroutineRunner _coroutineRunner = new();

        private IBulletProjectileComponent[] _projectileComponents = Array.Empty<IBulletProjectileComponent>();
        private bool _isPlaying;
        private Coroutine _stopRoutine;

        /// <summary>再生中か</summary>
        bool IBulletProjectile.IsPlaying => _isPlaying;
        /// <summary>レイキャスト用の半径（0より大きいとSphereCast）</summary>
        float IBulletProjectile.RaycastRadius => _raycastRadius * transform.localScale.x;
        /// <summary>使用中のProjectile</summary>
        public IBulletProjectileController Controller { get; private set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            if (gameObject == null) {
                return;
            }

            _coroutineRunner.Dispose();
            _stopRoutine = null;
            foreach (var component in _projectileComponents) {
                component.Dispose();
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// 再生速度の変更
        /// </summary>
        /// <param name="speed">1.0を基準とした速度</param>
        void IBulletProjectile.SetSpeed(float speed) {
            SetSpeedInternal(speed);
            foreach (var component in _projectileComponents) {
                component.SetSpeed(speed);
            }
        }

        /// <summary>
        /// アクティブ状態の切り替え
        /// </summary>
        void IBulletProjectile.SetActive(bool active) {
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
        void IBulletProjectile.Start(IBulletProjectileController projectileController) {
            if (_isPlaying) {
                return;
            }

            _isPlaying = true;
            Controller = projectileController;
            ApplyTransform(Controller);
            
            StartInternal();
            foreach (var component in _projectileComponents) {
                component.Start(projectileController);
            }
        }

        /// <summary>
        /// Projectileの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBulletProjectile.Update(float deltaTime) {
            ApplyTransform(Controller);
            
            _coroutineRunner.Update();
            foreach (var component in _projectileComponents) {
                component.Update(deltaTime);
            }
        }

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        void IBulletProjectile.Exit() {
            if (!_isPlaying) {
                return;
            }

            if (_stopRoutine != null) {
                return;
            }

            IEnumerator Routine() {
                var list = _projectileComponents.Select(x => x.ExitRoutine())
                    .Concat(new[] { ExitRoutineInternal() });
                yield return new MergedCoroutine(list);
                Controller = null;
                _isPlaying = false;
                _stopRoutine = null;
            }

            _stopRoutine = _coroutineRunner.StartCoroutine(Routine());
        }

        /// <summary>
        /// コリジョンヒット時通知
        /// </summary>
        /// <param name="hit">当たり結果</param>
        void IBulletProjectile.OnHitCollision(RaycastHit hit) {
            OnHitCollisionInternal(hit);
            foreach (var component in _projectileComponents) {
                component.OnHitCollision(hit);
            }
        }

        /// <summary>
        /// スケールの設定
        /// </summary>
        /// <param name="scale">スケール</param>
        void IBulletProjectile.SetLocalScale(Vector3 scale) {
            var trans = transform;
            trans.localScale = scale;
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

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _projectileComponents = gameObject.GetComponentsInChildren<IBulletProjectileComponent>();
        }

        /// <summary>
        /// Transform情報の反映
        /// </summary>
        private void ApplyTransform(IBulletProjectileController projectileController)
        {
            var trans = transform;
            trans.position = projectileController.Position;
            trans.rotation = projectileController.Rotation;
        }
    }
}