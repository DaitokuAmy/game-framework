using System;
using System.Collections;
using System.Linq;
using GameFramework.CollisionSystems;
using GameFramework.CoroutineSystems;
using UnityEngine;
using Coroutine = GameFramework.CoroutineSystems.Coroutine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// ビームオブジェクト用インターフェース
    /// </summary>
    public interface IBeamProjectile : IDisposable {
        /// <summary>Transform参照</summary>
        Transform transform { get; }
        /// <summary>飛翔情報</summary>
        IBeamProjectileController Controller { get; }
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
        /// <param name="projectileController">飛翔物の情報</param>
        void Start(IBeamProjectileController projectileController);

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
        /// 衝突発生通知
        /// </summary>
        /// <param name="hit">衝突結果</param>
        void OnHitCollision(RaycastHit hit);

        /// <summary>
        /// スケールの設定
        /// </summary>
        /// <param name="scale">スケールの設定</param>
        void SetLocalScale(Vector3 scale);
    }

    /// <summary>
    /// ビームの実体制御用MonoBehaviour
    /// </summary>
    public class BeamProjectile : MonoBehaviour, IBeamProjectile {
        [SerializeField, Tooltip("レイキャスト用の半径(0より大きいとSphereRaycast)")]
        private float _raycastRadius = 0.0f;

        private readonly CoroutineRunner _coroutineRunner = new();

        private IBeamProjectileComponent[] _projectileComponents = Array.Empty<IBeamProjectileComponent>();
        private bool _isPlaying;
        private Coroutine _stopRoutine;

        /// <summary>再生中か</summary>
        bool IBeamProjectile.IsPlaying => _isPlaying;
        /// <summary>レイキャスト用の半径（0より大きいとSphereCast）</summary>
        float IBeamProjectile.RaycastRadius => _raycastRadius * transform.localScale.x;

        /// <summary>使用中のProjectile</summary>
        public IBeamProjectileController Controller { get; private set; }

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
        void IBeamProjectile.SetSpeed(float speed) {
            SetSpeedInternal(speed);
            foreach (var component in _projectileComponents) {
                component.SetSpeed(speed);
            }
        }

        /// <summary>
        /// アクティブ状態の切り替え
        /// </summary>
        void IBeamProjectile.SetActive(bool active) {
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
        void IBeamProjectile.Start(IBeamProjectileController projectileController) {
            if (_isPlaying) {
                return;
            }

            if (!(_stopRoutine?.IsDone ?? true)) {
                _coroutineRunner.StopCoroutine(_stopRoutine);
                _stopRoutine = null;
            }

            _isPlaying = true;
            Controller = projectileController;
            ApplyTransform();
            
            StartInternal();
            foreach (var component in _projectileComponents) {
                component.Start(projectileController);
            }
        }

        /// <summary>
        /// Projectileの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBeamProjectile.Update(float deltaTime) {
            ApplyTransform();
            
            _coroutineRunner.Update();
            foreach (var component in _projectileComponents) {
                component.Update(deltaTime);
            }
        }

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        void IBeamProjectile.Exit() {
            if (!_isPlaying) {
                return;
            }

            if (_stopRoutine != null) {
                return;
            }
            
            ApplyTransform();

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
        void IBeamProjectile.OnHitCollision(RaycastHit hit) {
            OnHitCollisionInternal(hit);
            foreach (var component in _projectileComponents) {
                component.OnHitCollision(hit);
            }
        }

        /// <summary>
        /// スケールの設定
        /// </summary>
        /// <param name="scale">スケール</param>
        void IBeamProjectile.SetLocalScale(Vector3 scale) {
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
            _projectileComponents = gameObject.GetComponentsInChildren<IBeamProjectileComponent>();
        }

        /// <summary>
        /// Transform情報の反映
        /// </summary>
        private void ApplyTransform()
        {
            var trans = transform;
            trans.position = Controller.HeadPosition;
            trans.rotation = Controller.Rotation;
        }
    }
}