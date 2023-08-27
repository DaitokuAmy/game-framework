using System;
using System.Collections;
using GameFramework.CollisionSystems;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// ビームオブジェクト用インターフェース
    /// </summary>
    public interface IBeamProjectileObject : IDisposable {
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
        /// スケールの設定
        /// </summary>
        /// <param name="scale">スケールの設定</param>
        void SetLocalScale(Vector3 scale);

        /// <summary>
        /// 衝突発生通知
        /// </summary>
        /// <param name="result">衝突結果</param>
        void OnHitCollision(RaycastHitResult result);
    }

    /// <summary>
    /// ビームの実体制御用MonoBehaviour
    /// </summary>
    public class BeamProjectileObject : MonoBehaviour, IBeamProjectileObject {
        [SerializeField, Tooltip("レイキャスト用の半径(0より大きいとSphereRaycast)")]
        private float _raycastRadius = 0.0f;

        private bool _isPlaying;
        private CoroutineRunner _coroutineRunner = new CoroutineRunner();

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

            Destroy(gameObject);
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

            Projectile = projectile;
            ((IBeamProjectileObject)this).UpdateProjectile(projectile);
            _isPlaying = true;
            StartProjectileInternal();
        }

        /// <summary>
        /// Projectileの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBeamProjectileObject.Update(float deltaTime) {
            _coroutineRunner.Update();
        }

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        void IBeamProjectileObject.Exit() {
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
        void IBeamProjectileObject.UpdateProjectile(IBeamProjectile projectile) {
            var trans = transform;
            trans.position = projectile.HeadPosition;
            trans.rotation = projectile.Rotation;

            UpdateTransformInternal(projectile);
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
        /// コリジョンヒット時通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        void IBeamProjectileObject.OnHitCollision(RaycastHitResult result) {
            OnHitCollision(result);
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
    }
}