using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// 姿勢追従
    /// </summary>
    public class RotationAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private RotationAttachmentResolver.ResolverSettings _settings = new();

        private RotationAttachmentResolver _resolver;

        /// <summary>制御用設定</summary>
        public RotationAttachmentResolver.ResolverSettings Settings {
            set {
                _settings = value;
                _resolver.Settings = _settings;
            }
        }
        /// <summary>Transform制御用クラス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            _resolver = new RotationAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}
