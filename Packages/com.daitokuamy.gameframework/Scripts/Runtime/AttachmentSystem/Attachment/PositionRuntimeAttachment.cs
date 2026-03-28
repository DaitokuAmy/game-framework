using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// 座標追従
    /// </summary>
    public class PositionRuntimeAttachment : RuntimeAttachment {
        private PositionAttachmentResolver _resolver;

        /// <summary>Transform制御用インスタンス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <summary>追従設定</summary>
        public PositionAttachmentResolver.ResolverSettings Settings {
            get => _resolver.Settings;
            set => _resolver.Settings = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PositionRuntimeAttachment(Transform owner) {
            _resolver = new PositionAttachmentResolver(owner);
        }
    }
}
