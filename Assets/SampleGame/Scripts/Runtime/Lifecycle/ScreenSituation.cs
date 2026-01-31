using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework;
using GameFramework.NavigationSystem;
using GameFramework.UISystem;
using SampleGame.Application;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// UIScreenを制御する前提のScreenNode基底
    /// </summary>
    public abstract class ScreenNode<TUIService> : ScreenNode
        where TUIService : UIService {
        private readonly List<AnimationHandle> _animationHandles = new();
        private readonly List<UIScreen> _screens = new();
        
        /// <summary>UI管理クラス</summary>
        [Inject]
        protected UIManager UIManager { get; private set; }
        /// <summary>アプリ遷移制御クラス</summary>
        [Inject]
        protected IAppNavigator AppNavigator { get; private set; }

        /// <summary>利用するサービスへの参照</summary>
        protected TUIService UIService => UIManager.GetService<TUIService>();

        /// <inheritdoc/>
        protected override IEnumerator OpenRoutine(TransitionHandle<INavNode> handle, IScope animationScope) {
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
        protected override void PostOpen(TransitionHandle<INavNode> handle) {
            _screens.Clear();
            GetScreens(UIService, _screens);

            foreach (var screen in _screens) {
                screen.OpenAsync(handle.Direction, true);
            }
        }

        /// <inheritdoc/>
        protected override IEnumerator CloseRoutine(TransitionHandle<INavNode> handle, IScope animationScope) {
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
        protected override void PostClose(TransitionHandle<INavNode> handle) {
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