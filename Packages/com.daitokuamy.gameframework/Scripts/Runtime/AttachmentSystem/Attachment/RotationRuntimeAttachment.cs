using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// 姿勢追従
    /// </summary>
    public class RotationRuntimeAttachment : RuntimeAttachment {
        private RotationAttachmentResolver _resolver;

        /// <summary>Transform制御用インスタンス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <summary>追従設定</summary>
        public RotationAttachmentResolver.ResolverSettings Settings {
            get => _resolver.Settings;
            set => _resolver.Settings = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RotationRuntimeAttachment(Transform owner) {
            _resolver = new RotationAttachmentResolver(owner);
        }
    }
}
