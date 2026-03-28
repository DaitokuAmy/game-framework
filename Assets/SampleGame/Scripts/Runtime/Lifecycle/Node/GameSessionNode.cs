using System.Collections;
using GameFramework;
using GameFramework.NavigationSystem;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// ゲームプレイ中の共通セッション
    /// </summary>
    public class GameSessionNode : SessionNode {
        /// <inheritdoc/>
        protected override IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield return base.InitializeRoutine(handle, scope);

            DebugLog.Info("Enter GameSession");
        }

        /// <inheritdoc/>
        protected override void Terminate(TransitionHandle<INavNode> handle) {
            DebugLog.Info("Exit GameSession");
            
            base.Terminate(handle);
        }
    }
}