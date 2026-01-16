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
            return SetupLogic(logic, objectResolver, activate, scope);
        }

        /// <summary>
        /// UpdatableLogicの生成
        /// </summary>
        public static TLogic CreateLogic<TLogic>(UpdateOrder order, IObjectResolver objectResolver = null, bool activate = true, IScope scope = null)
            where TLogic : Logic, IUpdatable, new() {
            var logic = new TLogic();
            return SetupLogic(logic, order, objectResolver, activate, scope);
        }

        /// <summary>
        /// LateUpdatableLogicの生成
        /// </summary>
        public static TLogic CreateLogic<TLogic>(LateUpdateOrder order, IObjectResolver objectResolver = null, bool activate = true, IScope scope = null)
            where TLogic : Logic, ILateUpdatable, new() {
            var logic = new TLogic();
            return SetupLogic(logic, order, objectResolver, activate, scope);
        }

        /// <summary>
        /// UpdateAndLateUpdatableLogicの生成
        /// </summary>
        public static TLogic CreateLogic<TLogic>(UpdateOrder updateOrder, LateUpdateOrder lateUpdateOrder, IObjectResolver objectResolver = null, bool activate = true, IScope scope = null)
            where TLogic : Logic, IUpdatable, ILateUpdatable, new() {
            var logic = new TLogic();
            return SetupLogic(logic, updateOrder, lateUpdateOrder, objectResolver, activate, scope);
        }

        /// <summary>
        /// Logicの初期化
        /// </summary>
        public static TLogic SetupLogic<TLogic>(TLogic logic, IObjectResolver objectResolver = null, bool activate = true, IScope scope = null)
            where TLogic : Logic {
            if (scope != null) {
                logic.RegisterTo(scope);
            }

            objectResolver?.Inject(logic);

            if (activate) {
                logic.Activate();
            }

            return logic;
        }

        /// <summary>
        /// Logicの初期化
        /// </summary>
        public static TLogic SetupLogic<TLogic>(TLogic logic, UpdateOrder order, IObjectResolver objectResolver = null, bool activate = true, IScope scope = null)
            where TLogic : Logic, IUpdatable {
            logic.RegisterUpdatable(order);
            return SetupLogic(logic, objectResolver, activate, scope);
        }

        /// <summary>
        /// Logicの初期化
        /// </summary>
        public static TLogic SetupLogic<TLogic>(TLogic logic, LateUpdateOrder order, IObjectResolver objectResolver = null, bool activate = true, IScope scope = null)
            where TLogic : Logic, ILateUpdatable {
            logic.RegisterLateUpdatable(order);
            return SetupLogic(logic, objectResolver, activate, scope);
        }

        /// <summary>
        /// Logicの初期化
        /// </summary>
        public static TLogic SetupLogic<TLogic>(TLogic logic, UpdateOrder updateOrder, LateUpdateOrder lateUpdateOrder, IObjectResolver objectResolver = null,
            bool activate = true, IScope scope = null)
            where TLogic : Logic, IUpdatable, ILateUpdatable {
            logic.RegisterUpdatable(updateOrder);
            logic.RegisterLateUpdatable(lateUpdateOrder);
            return SetupLogic(logic, objectResolver, activate, scope);
        }
    }
}