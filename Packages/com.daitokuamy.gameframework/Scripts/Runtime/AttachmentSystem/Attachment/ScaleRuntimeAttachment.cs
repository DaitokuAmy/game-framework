using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// 拡大縮小追従
    /// </summary>
    public class ScaleRuntimeAttachment : RuntimeAttachment {
        private ScaleAttachmentResolver _resolver;

        /// <summary>Transform制御用インスタンス</summary>
        protected override AttachmentResolver Resolver => _resolver;

        /// <summary>追従設定</summary>
        public ScaleAttachmentResolver.ResolverSettings Settings {
            get => _resolver.Settings;
            set => _resolver.Settings = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScaleRuntimeAttachment(Transform owner) {
            _resolver = new ScaleAttachmentResolver(owner);
        }
    }
}
