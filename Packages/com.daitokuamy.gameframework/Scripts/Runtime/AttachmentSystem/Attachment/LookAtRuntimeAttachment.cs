using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// 注視追従コンポーネント
    /// </summary>
    public class LookAtRuntimeAttachment : RuntimeAttachment {
        private LookAtAttachmentResolver _resolver;

        /// <summary>Transform制御用インスタンス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <summary>追従設定</summary>
        public LookAtAttachmentResolver.ResolverSettings Settings {
            get => _resolver.Settings;
            set => _resolver.Settings = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LookAtRuntimeAttachment(Transform owner) {
            _resolver = new LookAtAttachmentResolver(owner);
        }
    }
}
