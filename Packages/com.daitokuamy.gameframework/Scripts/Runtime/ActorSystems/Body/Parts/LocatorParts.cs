using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Transform管理クラス
    /// </summary>
    public class LocatorParts : MonoBehaviour {
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

        // Locatorキー一覧
        public string[] Keys {
            get {
                Initialize();

                return _locators.Keys.ToArray();
            }
        }

        // ロケーター情報のアクセサ
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