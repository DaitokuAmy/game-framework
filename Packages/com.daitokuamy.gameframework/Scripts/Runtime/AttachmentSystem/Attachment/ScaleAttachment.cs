using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// 拡縮追従
    /// </summary>
    public class ScaleAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private ScaleAttachmentResolver.ResolverSettings _settings = new();

        private ScaleAttachmentResolver _resolver;

        /// <summary>制御用設定</summary>
        public ScaleAttachmentResolver.ResolverSettings Settings {
            set {
                _settings = value;
                _resolver.Settings = _settings;
            }
        }
        /// <summary>Transform制御用クラス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            _resolver = new ScaleAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}
