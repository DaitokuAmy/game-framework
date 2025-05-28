using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// エイム追従コンポーネント
    /// </summary>
    public class AimRuntimeAttachment : RuntimeAttachment {
        private AimAttachmentResolver _resolver;

        // Transform制御用インスタンス
        protected override AttachmentResolver Resolver => _resolver;

        // 追従設定
        public AimAttachmentResolver.ResolverSettings Settings {
            get => _resolver.Settings;
            set => _resolver.Settings = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AimRuntimeAttachment(Transform owner) {
            _resolver = new AimAttachmentResolver(owner);
        }
    }
}