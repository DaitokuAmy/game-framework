using System.Collections;
using GameFramework.SituationSystems;
using ThirdPersonEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// GraphicsRaycastをブロックするTransitionEffect
    /// </summary>
    public class BlockTransitionEffect : ITransitionEffect {
        private BlockUIScreen.Handle _handle;
        
        /// <summary>
        /// 遷移開始
        /// </summary>
        void ITransitionEffect.Begin(){
            _handle = ResidentUIUtility.StartBlocking();
        }
        
        /// <summary>
        /// 開始ルーチン
        /// </summary>
        IEnumerator ITransitionEffect.EnterRoutine() {
            yield break;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ITransitionEffect.Update() {
        }

        /// <summary>
        /// 終了ルーチン
        /// </summary>
        IEnumerator ITransitionEffect.ExitRoutine() {
            yield break;
        }
        
        /// <summary>
        /// 遷移終了
        /// </summary>
        void ITransitionEffect.End(){
            _handle.Dispose();
        }
    }
}