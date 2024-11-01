using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標追従
    /// </summary>
    public class PositionAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private PositionAttachmentResolver.ResolverSettings _settings = null;

        private PositionAttachmentResolver _resolver;

        // 制御用設定
        public PositionAttachmentResolver.ResolverSettings Settings {
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
            _resolver = new PositionAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }
    }
}