using System.Collections;

namespace GameFramework.NavigationSystem {
    /// <summary>
    /// ScreenとなるNavNode
    /// ※Session or Screen直下にのみ置けるNode
    /// </summary>
    public abstract class ScreenNode : NavNode, IScreenNode {
        private DisposableScope _animationScope;
        private bool _opened;
        
        /// <inheritdoc/>
        void IScreenNode.PreOpen(TransitionHandle<INavNode> handle) {
            PreOpen(handle);
            _animationScope = new DisposableScope();
        }

        /// <inheritdoc/>
        IEnumerator IScreenNode.OpenRoutine(TransitionHandle<INavNode> handle) {
            yield return OpenRoutine(handle, _animationScope);
        }

        /// <inheritdoc/>
        void IScreenNode.PostOpen(TransitionHandle<INavNode> handle) {
            _animationScope?.Dispose();
            _animationScope = null;
            PostOpen(handle);
            _opened = true;
        }

        /// <inheritdoc/>
        void IScreenNode.PreClose(TransitionHandle<INavNode> handle) {
            _opened = false;
            PreClose(handle);
            _animationScope = new DisposableScope();
        }

        /// <inheritdoc/>
        IEnumerator IScreenNode.CloseRoutine(TransitionHandle<INavNode> handle) {
            yield return CloseRoutine(handle, _animationScope);
        }

        /// <inheritdoc/>
        void IScreenNode.PostClose(TransitionHandle<INavNode> handle) {
            _animationScope?.Dispose();
            _animationScope = null;
            PostClose(handle);
        }
        
        /// <inheritdoc/>
        protected sealed override void Shutdown(TransitionHandle<INavNode> handle) {
            _animationScope?.Dispose();
            _animationScope = null;
            
            if (_opened) {
                PreClose(handle);
                PostClose(handle);
            }
        }

        /// <summary>
        /// 開く前処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void PreOpen(TransitionHandle<INavNode> handle) {
        }
        
        /// <summary>
        /// 開くアニメルーチン
        /// ※Immediate時は呼ばれない
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">アニメーションキャンセル用Scope</param>
        protected virtual IEnumerator OpenRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield break;
        }

        /// <summary>
        /// 開いた後処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void PostOpen(TransitionHandle<INavNode> handle) {
        }
        
        /// <summary>
        /// 閉じる前処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void PreClose(TransitionHandle<INavNode> handle) {
        }
        
        /// <summary>
        /// 閉じるアニメルーチン
        /// ※Immediate時は呼ばれない
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">アニメーションキャンセル用Scope</param>
        protected virtual IEnumerator CloseRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield break;
        }
        
        /// <summary>
        /// 閉じた後処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void PostClose(TransitionHandle<INavNode> handle) {
        }
    }
}