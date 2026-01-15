using System.Collections.Generic;
using System.Linq;
using GameFramework;
using GameFramework.Core;
using VContainer;

namespace ThirdPersonEngine {
    /// <summary>
    /// ロジックのユーティリティクラス
    /// </summary>
    public static class LogicUtility {
        /// <summary>
        /// Logicの生成
        /// </summary>
        public static TLogic CreateLogic<TLogic>(IObjectResolver objectResolver = null, bool activate = true, IScope scope = null)
            where TLogic : Logic, new() {
            var logic = new TLogic();
            logic.RegisterTask(TaskOrder.Logic);
            if (scope != null) {
                logic.RegisterTo(scope);
            }

            objectResolver?.Inject(logic);

            if (activate) {
                logic.Activate();
            }

            return logic;
        }
    }
}