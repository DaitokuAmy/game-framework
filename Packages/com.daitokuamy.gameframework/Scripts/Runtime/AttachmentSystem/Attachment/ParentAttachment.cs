using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// Transform追従用コンポーネント
    /// </summary>
    public class ParentAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private ParentAttachmentResolver.ResolverSettings _settings = new();

        private ParentAttachmentResolver _resolver;

        /// <summary>制御用設定</summary>
        public ParentAttachmentResolver.ResolverSettings Settings {
            set {
                _settings = value;
                _resolver.Settings = _settings;
            }
        }
        /// <summary>Transform制御用クラス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            _resolver = new ParentAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}
