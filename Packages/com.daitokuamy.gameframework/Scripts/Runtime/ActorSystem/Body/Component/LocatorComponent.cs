using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// Transform管理クラス
    /// </summary>
    public sealed class LocatorComponent : BodyComponent {
        // ロケーター管理クラスのリスト
        private List<LocatorParts> _locatorPartsList = new List<LocatorParts>();

        /// <summary>Locator取得用アクセサ</summary>
        public Transform this[string key] {
            get {
                for (var i = 0; i < _locatorPartsList.Count; i++) {
                    var result = _locatorPartsList[i][key];
                    if (result != null) {
                        return result;
                    }
                }

                return Body.Transform;
            }
        }

        /// <summary>
        /// 制御キーの一覧を取得
        /// </summary>
        public string[] GetKeys() {
            return _locatorPartsList.SelectMany(x => x.Keys).Distinct().ToArray();
        }

        /// <inheritdoc/>
        protected override void InitializeInternal(IScope scope) {
            var meshController = Body.GetBodyComponent<MeshComponent>();
            meshController.RefreshedEvent += RefreshLocatorPartsList;
            RefreshLocatorPartsList();
        }

        /// <summary>
        /// LocatorPartsリストのリフレッシュ
        /// </summary>
        private void RefreshLocatorPartsList() {
            _locatorPartsList.Clear();
            _locatorPartsList.AddRange(Body.GetComponentsInChildren<LocatorParts>(true));
        }
    }
}
