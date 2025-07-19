using System.Collections.Generic;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション監視用クラス
    /// </summary>
    public class SituationMonitor {
        private static SituationMonitor s_instance;

        private readonly List<IMonitoredContainer> _containers = new();
        private readonly List<IMonitoredFlow> _flows = new();

        /// <summary>シングルトンインスタンス取得</summary>
        private static SituationMonitor Instance {
            get {
                if (s_instance == null) {
                    s_instance = new SituationMonitor();
                }

                return s_instance;
            }
        }

        /// <summary>コンテナの一覧</summary>
        public static IReadOnlyList<IMonitoredContainer> Containers => Instance._containers;
        /// <summary>フローの一覧</summary>
        public static IReadOnlyList<IMonitoredFlow> Flows => Instance._flows;

        /// <summary>
        /// コンテナの登録
        /// </summary>
        public static void AddContainer(IMonitoredContainer container) {
            Instance._containers.Add(container);
        }

        /// <summary>
        /// コンテナの登録解除
        /// </summary>
        public static void RemoveContainer(IMonitoredContainer container) {
            Instance._containers.Remove(container);
        }

        /// <summary>
        /// フローの登録
        /// </summary>
        public static void AddFlow(IMonitoredFlow flow) {
            Instance._flows.Add(flow);
        }

        /// <summary>
        /// フローの登録解除
        /// </summary>
        public static void RemoveFlow(IMonitoredFlow flow) {
            Instance._flows.Remove(flow);
        }
    }
}