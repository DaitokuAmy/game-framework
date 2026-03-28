using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// Transform管理クラス
    /// </summary>
    public sealed class LocatorParts : MonoBehaviour {
        // ロケーター情報
        [Serializable]
        private class LocatorInfo {
            public string key;
            public Transform transform;
        }

        [SerializeField, Tooltip("Locator情報")]
        private LocatorInfo[] _locatorInfos = Array.Empty<LocatorInfo>();

        // ロケーター情報
        private Dictionary<string, Transform> _locators;

        /// <summary>Locatorキー一覧</summary>
        public string[] Keys {
            get {
                Initialize();

                return _locators.Keys.ToArray();
            }
        }

        /// <summary>ロケーター情報のアクセサ</summary>
        public Transform this[string key] {
            get {
                Initialize();
                
                if (_locators.TryGetValue(key, out var result)) {
                    return result;
                }

                return null;
            }
        }

        /// <summary>
        /// スクリプトリロード
        /// </summary>
        private void OnValidate() {
            _locators = null;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize() {
            if (_locators == null) {
                _locators = _locatorInfos.ToDictionary(x => x.key, x => x.transform);
            }
        }
    }
}
