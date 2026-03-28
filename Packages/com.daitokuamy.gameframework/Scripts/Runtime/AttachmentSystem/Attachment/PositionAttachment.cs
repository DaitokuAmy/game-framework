using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// 座標追従
    /// </summary>
    public class PositionAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private PositionAttachmentResolver.ResolverSettings _settings = new();

        private PositionAttachmentResolver _resolver;

        /// <summary>制御用設定</summary>
        public PositionAttachmentResolver.ResolverSettings Settings {
            set {
                _settings = value;
                _resolver.Settings = _settings;
            }
        }
        /// <summary>Transform制御用クラス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            _resolver = new PositionAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}
