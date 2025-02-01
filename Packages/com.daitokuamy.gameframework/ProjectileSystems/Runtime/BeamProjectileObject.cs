using System;
using System.Collections;
using System.Linq;
using GameFramework.CollisionSystems;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// ビームオブジェクト用インターフェース
    /// </summary>
    public interface IBeamProjectileObject : IDisposable {
        /// <summary>Transform参照</summary>
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
        void Exit();

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

        /// <summary>
        /// スケールの設定
        /// </summary>
        /// <param name="scale">スケールの設定</param>
        void SetLocalScale(Vector3 scale);
    }

    /// <summary>
    /// ビームの実体制御用MonoBehaviour
    /// </summary>
    public class BeamProjectileObject : MonoBehaviour, IBeamProjectileObject {
        [SerializeField, Tooltip("レイキャスト用の半径(0より大きいとSphereRaycast)")]
        private float _raycastRadius = 0.0f;

        private readonly CoroutineRunner _coroutineRunner = new();

        private IBeamProjectileComponent[] _projectileComponents = Array.Empty<IBeamProjectileComponent>();
        private bool _isPlaying;

        // 再生中か
        bool IBeamProjectileObject.IsPlaying => _isPlaying;
        // レイキャスト用の半径（0より大きいとSphereCast）
        float IBeamProjectileObject.RaycastRadius => _raycastRadius * transform.localScale.x;

        /// <summary>使用中のProjectile</summary>
        protected IBeamProjectile Projectile { get; private set; }

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
        void IBeamProjectileObject.SetSpeed(float speed) {
            SetSpeedInternal(speed);
            foreach (var component in _projectileComponents) {
                component.SetSpeed(speed);
            }
        }

        /// <summary>
        /// アクティブ状態の切り替え
        /// </summary>
        void IBeamProjectileObject.SetActive(bool active) {
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
        void IBeamProjectileObject.Start(IBeamProjectile projectile) {
            if (_isPlaying) {
                return;
            }

            _isPlaying = true;
            Projectile = projectile;
            StartProjectileInternal();
            foreach (var component in _projectileComponents) {
                component.Start(projectile);
            }

            ((IBeamProjectileObject)this).UpdateProjectile(projectile);
        }

        /// <summary>
        /// Projectileの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBeamProjectileObject.Update(float deltaTime) {
            _coroutineRunner.Update();
            foreach (var component in _projectileComponents) {
                component.Update(deltaTime);
            }
        }

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        void IBeamProjectileObject.Exit() {
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
        void IBeamProjectileObject.UpdateProjectile(IBeamProjectile projectile) {
            var trans = transform;
            trans.position = projectile.HeadPosition;
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
        void IBeamProjectileObject.OnHitCollision(RaycastHitResult result) {
            OnHitCollision(result);
            foreach (var component in _projectileComponents) {
                component.OnHitCollision(result);
            }
        }

        /// <summary>
        /// スケールの設定
        /// </summary>
        /// <param name="scale">スケール</param>
        void IBeamProjectileObject.SetLocalScale(Vector3 scale) {
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
        protected virtual void StartProjectileInternal() {
        }

        /// <summary>
        /// 内部用Transform更新処理
        /// </summary>
        protected virtual void UpdateTransformInternal(IBeamProjectile projectile) {
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

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _projectileComponents = gameObject.GetComponentsInChildren<IBeamProjectileComponent>();
        }
    }
}