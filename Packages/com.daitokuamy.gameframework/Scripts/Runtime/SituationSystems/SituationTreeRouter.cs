using System;
using System.Runtime.CompilerServices;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移に使うツリー
    /// </summary>
    public sealed class SituationTreeRouter : StateTreeRouter<Type, Situation, SituationContainer.TransitionOption> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationTreeRouter(SituationContainer container, string label = "", [CallerFilePath] string caller = "")
            : base(container, string.IsNullOrEmpty(label) ? $"[SituationTree]{PathUtility.GetRelativePath(caller)}" : $"[SituationTree]{label}") {
        }
    }
}