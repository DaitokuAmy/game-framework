using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// 注視追従コンポーネント
    /// </summary>
    public class LookAtAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private LookAtAttachmentResolver.ResolverSettings _settings = new();

        private LookAtAttachmentResolver _resolver;

        /// <summary>制御用設定</summary>
        public LookAtAttachmentResolver.ResolverSettings Settings {
            set {
                _settings = value;
                _resolver.Settings = _settings;
            }
        }
        /// <summary>Transform制御用クラス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            _resolver = new LookAtAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}
