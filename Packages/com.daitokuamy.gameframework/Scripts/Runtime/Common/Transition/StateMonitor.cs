using System.Collections.Generic;
using System.Diagnostics;

namespace GameFramework {
    /// <summary>
    /// ステート監視用クラス
    /// </summary>
    public class StateMonitor {
        private static StateMonitor s_instance;

        private readonly List<IMonitoredStateContainer> _containers = new();
        private readonly List<IMonitoredStateRouter> _routers = new();

        /// <summary>シングルトンインスタンス取得</summary>
        private static StateMonitor Instance {
            get {
                if (s_instance == null) {
                    s_instance = new StateMonitor();
                }

                return s_instance;
            }
        }

        /// <summary>コンテナの一覧</summary>
        public static IReadOnlyList<IMonitoredStateContainer> Containers => Instance._containers;
        /// <summary>ルーターの一覧</summary>
        public static IReadOnlyList<IMonitoredStateRouter> Routers => Instance._routers;

        /// <summary>
        /// コンテナの登録
        /// </summary>
        [Conditional("GF_DEBUG")]
        public static void AddContainer(IMonitoredStateContainer container) {
            Instance._containers.Add(container);
        }

        /// <summary>
        /// コンテナの登録解除
        /// </summary>
        [Conditional("GF_DEBUG")]
        public static void RemoveContainer(IMonitoredStateContainer container) {
            Instance._containers.Remove(container);
        }

        /// <summary>
        /// ルーターの登録
        /// </summary>
        [Conditional("GF_DEBUG")]
        public static void AddRouter(IMonitoredStateRouter router) {
            Instance._routers.Add(router);
        }

        /// <summary>
        /// ルーターの登録解除
        /// </summary>
        [Conditional("GF_DEBUG")]
        public static void RemoveRouter(IMonitoredStateRouter router) {
            Instance._routers.Remove(router);
        }
    }
}