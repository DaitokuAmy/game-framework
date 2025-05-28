using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// ServiceContainerにServiceをインストールするクラス
    /// </summary>
    public class ServiceContainerInstaller : MonoBehaviour {
        [SerializeField, Tooltip("LocatorにインストールするServiceリスト"), ComponentSelector]
        private Component[] _installServices = Array.Empty<Component>();

        /// <summary>
        /// インストール処理
        /// </summary>
        public void Install(IServiceContainer container, IScope scope = null) {
            foreach (var component in _installServices) {
                container.RegisterInstance(component).RegisterTo(scope);
            }
        }
    }
}