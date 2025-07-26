using System;
using System.Runtime.CompilerServices;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移に使うスタック
    /// </summary>
    public sealed class SituationStackRouter : StateStackRouter<Type, Situation, SituationContainer.TransitionOption> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationStackRouter(SituationContainer container, string label = "", [CallerFilePath] string caller = "")
            : base(container, string.IsNullOrEmpty(label) ? $"[SituationStack]{caller}" : $"[SituationStack]{label}") {
        }
    }
}