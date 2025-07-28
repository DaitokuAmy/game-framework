using System.Collections.Generic;

namespace GameFramework.Core {
    /// <summary>
    /// Service状況監視用クラス
    /// </summary>
    public class ServiceMonitor {
        private static ServiceMonitor s_instance;

        private readonly List<IMonitoredServiceContainer> _containers = new();

        /// <summary>シングルトンインスタンス取得</summary>
        private static ServiceMonitor Instance {
            get {
                if (s_instance == null) {
                    s_instance = new ServiceMonitor();
                }

                return s_instance;
            }
        }

        /// <summary>コンテナの一覧</summary>
        public static IReadOnlyList<IMonitoredServiceContainer> Containers => Instance._containers;

        /// <summary>
        /// コンテナの登録
        /// </summary>
        public static void AddContainer(IMonitoredServiceContainer serviceContainer) {
            Instance._containers.Add(serviceContainer);
        }

        /// <summary>
        /// コンテナの登録解除
        /// </summary>
        public static void RemoveContainer(IMonitoredServiceContainer serviceContainer) {
            Instance._containers.Remove(serviceContainer);
        }
    }
}