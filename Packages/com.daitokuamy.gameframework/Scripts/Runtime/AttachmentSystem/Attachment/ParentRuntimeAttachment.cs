using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// Transform追従用コンポーネント
    /// </summary>
    public class ParentRuntimeAttachment : RuntimeAttachment {
        private ParentAttachmentResolver _resolver;

        /// <summary>Transform制御用インスタンス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <summary>追従設定</summary>
        public ParentAttachmentResolver.ResolverSettings Settings {
            get => _resolver.Settings;
            set => _resolver.Settings = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ParentRuntimeAttachment(Transform owner) {
            _resolver = new ParentAttachmentResolver(owner);
        }
    }
}
