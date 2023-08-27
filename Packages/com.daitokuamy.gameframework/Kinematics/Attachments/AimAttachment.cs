using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// エイム追従コンポーネント
    /// </summary>
    public class AimAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private AimAttachmentResolver.ResolverSettings _settings = null;

        private AimAttachmentResolver _resolver;

        // 制御用設定
        public AimAttachmentResolver.ResolverSettings Settings {
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
        protected override void Initialize() {
            _resolver = new AimAttachmentResolver(transform);
            _resolver.Settings = _settings;
        }

        /// <summary>
        /// シリアライズ値更新時処理
        /// </summary>
        protected override void OnValidateInternal() {
            Settings = _settings;
        }
    }
}