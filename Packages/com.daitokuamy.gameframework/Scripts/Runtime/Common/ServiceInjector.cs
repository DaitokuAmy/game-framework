using GameFramework.Core;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// Inject対象とするServiceを設定するコンポーネント
    /// </summary>
    public sealed class ServiceInjector : MonoBehaviour {
        [SerializeField, Tooltip("サービスを注入させたいMonoBehaviourのリスト")]
        private MonoBehaviour[] _targets;

        /// <summary>
        /// DI処理
        /// </summary>
        public void Inject(IServiceResolver resolver) {
            foreach (var target in _targets) {
                if (target == null) {
                    continue;
                }

                resolver.Inject(target);
            }
        }
    }
}