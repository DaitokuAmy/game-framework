using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 拡縮追従
    /// </summary>
    public class ScaleAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private ScaleAttachmentResolver.ResolverSettings _settings = null;

        private ScaleAttachmentResolver _resolver;

        // 制御用設定
        public ScaleAttachmentResolver.ResolverSettings Settings {
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
            _resolver = new ScaleAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}