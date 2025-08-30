using System.Collections;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// 動的遷移用のオプションSituation
    /// </summary>
    public class OptionSituation : Situation {
        /// <inheritdoc/>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);
            
            // todo:とりあえずログ
            DebugLog.Info($"Setup {nameof(OptionSituation)}");
        }

        /// <inheritdoc/>
        protected override void CleanupInternal(TransitionHandle<Situation> handle) {
            // todo:とりあえずログ
            DebugLog.Info($"Cleanup {nameof(OptionSituation)}");
            
            base.CleanupInternal(handle);
        }
    }
}