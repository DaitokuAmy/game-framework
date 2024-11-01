using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Attachmentの基底
    /// </summary>
    [ExecuteAlways]
    public abstract class Attachment : MonoBehaviour, IAttachment {
        // 更新モード
        public enum Mode {
            Update,
            LateUpdate,
            Manual,
        }

        [SerializeField, Tooltip("有効化")]
        private bool _active = false;
        [SerializeField, Tooltip("ロック状態")]
        private bool _lock = false;
        [SerializeField, Tooltip("更新モード")]
        private Mode _updateMode = Mode.LateUpdate;
        [SerializeField, Tooltip("ターゲットリスト")]
        private AttachmentResolver.TargetSource[] _sources = Array.Empty<AttachmentResolver.TargetSource>();

        // 初期化済みか
        private bool _initialized;

        /// <summary>更新モード</summary>
        public Mode UpdateMode {
            get => _updateMode;
            set => _updateMode = value;
        }
        /// <summary>ターゲットリスト</summary>
        public AttachmentResolver.TargetSource[] Sources {
            set {
                Initialize();

                _sources = value;
                Resolver.Sources = _sources;
            }
        }
        /// <summary>Transform制御用インスタンス</summary>
        protected abstract AttachmentResolver Resolver { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected abstract void InitializeInternal();

        /// <summary>
        /// 更新処理
        /// </summary>
        public void ManualUpdate() {
            if (_updateMode != Mode.Manual) {
                return;
            }

            UpdateInternal();
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public void TransferOffset() {
            Initialize();
            Resolver.TransferOffset();
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public void ResetOffset() {
            Initialize();
            Resolver.ResetOffset();
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public void ApplyTransform() {
            Initialize();
            Resolver.Resolve();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize() {
            if (_initialized) {
                return;
            }

            _initialized = true;
            InitializeInternal();
        }

        /// <summary>
        /// 更新処理(内部用)
        /// </summary>
        private void UpdateInternal() {
            if (_sources.Length <= 0) {
                return;
            }

            if (!(_active && _lock)) {
                return;
            }

            ApplyTransform();
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            Sources = _sources;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            if (Application.isPlaying) {
                if (_updateMode != Mode.Update) {
                    return;
                }
            }
            else {
                // 非再生時はManualUpdateをここで回す
                if (_updateMode != Mode.Update && _updateMode != Mode.Manual) {
                    return;
                }
            }

            UpdateInternal();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            if (_updateMode != Mode.LateUpdate) {
                return;
            }

            UpdateInternal();
        }

        /// <summary>
        /// 値変更時の処理
        /// </summary>
        private void OnValidate() {
            _initialized = false;            
            Sources = _sources;
        }
    }
}