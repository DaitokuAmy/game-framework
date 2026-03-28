using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// エイム追従コンポーネント
    /// </summary>
    public class AimRuntimeAttachment : RuntimeAttachment {
        private AimAttachmentResolver _resolver;

        /// <summary>Transform制御用インスタンス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <summary>追従設定</summary>
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
