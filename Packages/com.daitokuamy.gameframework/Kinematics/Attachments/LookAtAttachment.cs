using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 注視追従コンポーネント
    /// </summary>
    public class LookAtAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private LookAtAttachmentResolver.ResolverSettings _settings = new();

        private LookAtAttachmentResolver _resolver;

        // 制御用設定
        public LookAtAttachmentResolver.ResolverSettings Settings {
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
            _resolver = new LookAtAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}