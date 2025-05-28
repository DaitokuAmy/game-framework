using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標追従
    /// </summary>
    public class PositionRuntimeAttachment : RuntimeAttachment {
        private PositionAttachmentResolver _resolver;

        // Transform制御用インスタンス
        protected override AttachmentResolver Resolver => _resolver;

        // 追従設定
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