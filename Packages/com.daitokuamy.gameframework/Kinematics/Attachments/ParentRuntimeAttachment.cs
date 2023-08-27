using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Transform追従用コンポーネント
    /// </summary>
    public class ParentRuntimeAttachment : RuntimeAttachment {
        private ParentAttachmentResolver _resolver;

        // Transform制御用インスタンス
        protected override AttachmentResolver Resolver => _resolver;

        // 追従設定
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