using System;
using GameFramework.ActorSystems;
using GameFramework.Core;

namespace ThirdPersonEngine {
    /// <summary>
    /// AnimationEvent制御用コントローラー
    /// </summary>
    public class AnimationEventComponent : BodyComponent {
        /// <summary>
        /// 空のProvider
        /// </summary>
        private class EmptyProvider : IAnimationEventProvider {
            public event Action<int> FootstepEvent;
            public event Action<int> LandingEvent;
            public event Action<int> DashEvent;
            public event Action<int> BrakingEvent;
        }

        /// <summary>アニメーションイベント提供用</summary>
        public IAnimationEventProvider Provider { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            Provider = Body.GetComponent<AnimationEventReceiver>();
            if (Provider == null) {
                Provider = new EmptyProvider();
            }
        }
    }
}