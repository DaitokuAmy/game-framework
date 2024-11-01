using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Transform追従用コンポーネント
    /// </summary>
    public class ParentAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private ParentAttachmentResolver.ResolverSettings _settings = null;

        private ParentAttachmentResolver _resolver;

        // 制御用設定
        public ParentAttachmentResolver.ResolverSettings Settings {
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
            _resolver = new ParentAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}