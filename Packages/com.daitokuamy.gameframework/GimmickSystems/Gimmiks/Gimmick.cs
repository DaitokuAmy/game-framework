using System;
using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// Body内に仕込むGimmickのインターフェース
    /// </summary>
    public interface IGimmick : IDisposable {
        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize();

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void UpdateGimmick(float deltaTime);

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void LateUpdateGimmick(float deltaTime);
    }
    
    /// <summary>
    /// Body内に仕込むGimmickの基底
    /// </summary>
    public abstract class Gimmick : MonoBehaviour, IGimmick {
        private bool _initialized;
        private bool _disposed;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        void IGimmick.Initialize() {
            if (_initialized) {
                return;
            }

            _initialized = true;
            InitializeInternal();
            PostInitializeInternal();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            DisposeInternal();
        }
        
        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IGimmick.UpdateGimmick(float deltaTime) {
            UpdateInternal(deltaTime);
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IGimmick.LateUpdateGimmick(float deltaTime) {
            LateUpdateInternal(deltaTime);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected virtual void InitializeInternal() {
        }

        /// <summary>
        /// 初期化後処理
        /// </summary>
        protected virtual void PostInitializeInternal() {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void LateUpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// Validate処理
        /// </summary>
        protected virtual void OnValidateInternal() {
        }

        /// <summary>
        /// Validate処理
        /// </summary>
        private void OnValidate() {
            // Inspectorに表示しない
            hideFlags |= HideFlags.HideInInspector;

            // Prefabに対しては処理しない
            if (gameObject.scene.IsValid()) {
                OnValidateInternal();
            }
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            ((IGimmick)this).Initialize();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            ((IDisposable)this).Dispose();
        }
    }
}