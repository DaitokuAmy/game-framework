using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 姿勢追従
    /// </summary>
    public class RotationAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private RotationAttachmentResolver.ResolverSettings _settings = new();

        private RotationAttachmentResolver _resolver;

        // 制御用設定
        public RotationAttachmentResolver.ResolverSettings Settings {
            set {
                _settings = value;
                _resolver.Settings = _settings;
            }
        }
        // Transform制御用クラス
        protected override AttachmentResolver Resolver => _resolver;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            _resolver = new RotationAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}