using GameFramework.Core;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// ServiceContainerにServiceをインストールするクラス
    /// </summary>
    public class ServiceContainerInstaller : MonoBehaviour {
        [SerializeField, Tooltip("LocatorにインストールするServiceリスト"), ComponentSelector]
        private Component[] _installServices = new Component[0];

        /// <summary>
        /// インストール処理
        /// </summary>
        public void Install(IServiceContainer container) {
            foreach (var component in _installServices) {
                container.Set(component);
            }
        }
    }
}