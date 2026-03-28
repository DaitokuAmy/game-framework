using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// エイム追従コンポーネント
    /// </summary>
    public class AimAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private AimAttachmentResolver.ResolverSettings _settings = new();

        private AimAttachmentResolver _resolver;

        /// <summary>制御用設定</summary>
        public AimAttachmentResolver.ResolverSettings Settings {
            set {
                _settings = value;
                _resolver.Settings = _settings;
            }
        }
        /// <summary>Transform制御用クラス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            _resolver = new AimAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}
