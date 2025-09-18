using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// UIScreenを制御する前提のSituation基底
    /// </summary>
    public abstract class ScreenSituation<TUIService> : Situation
        where TUIService : UIService {
        private readonly List<AnimationHandle> _animationHandles = new();
        private readonly List<UIScreen> _screens = new();

        /// <summary>利用するサービスへの参照</summary>
        protected TUIService UIService => ServiceResolver.Resolve<UIManager>().GetService<TUIService>();

        /// <inheritdoc/>
        protected override IEnumerator OpenRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            _screens.Clear();
            GetScreens(UIService, _screens);

            _animationHandles.Clear();
            foreach (var screen in _screens) {
                _animationHandles.Add(screen.OpenAsync(handle.Direction));
            }

            foreach (var animationHandle in _animationHandles) {
                yield return animationHandle;
            }
        }

        /// <inheritdoc/>
        protected override void PostOpenInternal(TransitionHandle<Situation> handle, IScope scope) {
            _screens.Clear();
            GetScreens(UIService, _screens);

            foreach (var screen in _screens) {
                screen.OpenAsync(handle.Direction, true);
            }
        }

        /// <inheritdoc/>
        protected override IEnumerator CloseRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            _screens.Clear();
            GetScreens(UIService, _screens);

            _animationHandles.Clear();
            foreach (var screen in _screens) {
                _animationHandles.Add(screen.CloseAsync(handle.Direction));
            }

            foreach (var animationHandle in _animationHandles) {
                yield return animationHandle;
            }
        }

        /// <inheritdoc/>
        protected override void PostCloseInternal(TransitionHandle<Situation> handle) {
            _screens.Clear();
            GetScreens(UIService, _screens);

            foreach (var screen in _screens) {
                screen.CloseAsync(handle.Direction, true);
            }
        }

        /// <summary>
        /// 制御対象のScreenを取得
        /// </summary>
        protected abstract void GetScreens(TUIService service, List<UIScreen> screens);
    }
}