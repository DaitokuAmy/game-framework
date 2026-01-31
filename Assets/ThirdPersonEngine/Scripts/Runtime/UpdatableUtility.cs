using GameFramework;
using GameFramework;

namespace ThirdPersonEngine {
    /// <summary>
    /// Updtable関連のUtility
    /// </summary>
    public static class UpdatableUtility {
        /// <summary>利用するScheduler</summary>
        private static UpdateScheduler UpdateScheduler { get; set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public static void Initialize(UpdateScheduler updateScheduler) {
            UpdateScheduler = updateScheduler;
        }

        /// <summary>
        /// 登録
        /// </summary>
        public static void RegisterUpdatable(this IUpdatable source, UpdateOrder order) {
            if (source == null || UpdateScheduler == null) {
                return;
            }

            UpdateScheduler.RegisterUpdatable(source, order);
        }

        /// <summary>
        /// 登録
        /// </summary>
        public static void RegisterLateUpdatable(this ILateUpdatable source, LateUpdateOrder order) {
            if (source == null || UpdateScheduler == null) {
                return;
            }

            UpdateScheduler.RegisterLateUpdatable(source, order);
        }

        /// <summary>
        /// 登録
        /// </summary>
        public static void RegisterFixedUpdatable(this IFixedUpdatable source, UpdateOrder order) {
            if (source == null || UpdateScheduler == null) {
                return;
            }

            UpdateScheduler.RegisterFixedUpdatable(source, order);
        }

        /// <summary>
        /// 登録
        /// </summary>
        public static void RegisterUpdatableAll(this IUpdatable source, UpdateOrder updateOrder, LateUpdateOrder lateUpdateOrder, FixedUpdateOrder fixedUpdateOrder = FixedUpdateOrder.None) {
            if (source == null || UpdateScheduler == null) {
                return;
            }

            if (updateOrder != UpdateOrder.None) {
                UpdateScheduler.RegisterUpdatable(source, updateOrder);
            }

            if (lateUpdateOrder != LateUpdateOrder.None && source is ILateUpdatable lateUpdatable) {
                UpdateScheduler.RegisterLateUpdatable(lateUpdatable, lateUpdateOrder);
            }

            if (fixedUpdateOrder != FixedUpdateOrder.None && source is IFixedUpdatable fixedUpdatable) {
                UpdateScheduler.RegisterFixedUpdatable(fixedUpdatable, fixedUpdateOrder);
            }
        }

        /// <summary>
        /// 登録除外
        /// </summary>
        public static void UnregisterUpdatable(this IUpdatable source) {
            if (source == null || UpdateScheduler == null) {
                return;
            }

            UpdateScheduler.UnregisterUpdatable(source);
        }

        /// <summary>
        /// 登録除外
        /// </summary>
        public static void UnregisterLateUpdatable(this ILateUpdatable source) {
            if (source == null || UpdateScheduler == null) {
                return;
            }

            UpdateScheduler.UnregisterLateUpdatable(source);
        }

        /// <summary>
        /// 登録除外
        /// </summary>
        public static void UnregisterFixedUpdatable(this IFixedUpdatable source) {
            if (source == null || UpdateScheduler == null) {
                return;
            }

            UpdateScheduler.UnregisterFixedUpdatable(source);
        }

        /// <summary>
        /// 登録除外
        /// </summary>
        public static void UnregisterUpdatableAll(this IUpdatable source) {
            if (source == null || UpdateScheduler == null) {
                return;
            }

            UpdateScheduler.UnregisterUpdatable(source);
            if (source is ILateUpdatable lateUpdatable) {
                UpdateScheduler.UnregisterLateUpdatable(lateUpdatable);
            }

            if (source is IFixedUpdatable fixedUpdatable) {
                UpdateScheduler.UnregisterFixedUpdatable(fixedUpdatable);
            }
        }
    }
}