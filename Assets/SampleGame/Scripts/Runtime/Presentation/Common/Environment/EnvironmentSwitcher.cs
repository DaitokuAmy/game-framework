using System;
using System.Collections.Generic;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// 環境設定切り替え用コンポーネント
    /// </summary>
    public class EnvironmentSwitcher : MonoBehaviour {
        /// <summary>
        /// 環境情報
        /// </summary>
        [Serializable]
        private class EnvironmentInfo {
            [Tooltip("制御キー")]
            public string key = "Default";
            [Tooltip("環境設定")]
            public EnvironmentSettings environmentSettings;
        }

        [SerializeField, Tooltip("デフォルトキー")]
        private string _defaultKey = "Default";
        [SerializeField, Tooltip("環境情報リスト")]
        private EnvironmentInfo[] _environmentInfos;

        private Dictionary<string, EnvironmentInfo> _environmentInfoDict;
        private string _currentKey;

        /// <summary>初期化済みか</summary>
        private bool IsInitialized => _environmentInfoDict != null;

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Start() {
            Initialize();
        }

        /// <summary>
        /// 切り替え
        /// </summary>
        /// <param name="key">切り替える対象のキー</param>
        public void Switch(string key) {
            Initialize();

            // 同じなら無視
            if (_currentKey == key) {
                return;
            }

            // 現在の環境情報を無効化
            if (_environmentInfoDict.TryGetValue(_currentKey, out var prevInfo)) {
                DeactivateEnvironmentInfo(prevInfo);
            }

            // 新しい環境情報を有効化
            _currentKey = key;
            if (_environmentInfoDict.TryGetValue(_currentKey, out var nextInfo)) {
                ActivateEnvironmentInfo(nextInfo);
            }
            // 存在しないならDefaultを使用
            else {
                _currentKey = _defaultKey;
                if (_environmentInfoDict.TryGetValue(_currentKey, out nextInfo)) {
                    ActivateEnvironmentInfo(nextInfo);
                }
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize() {
            if (_environmentInfoDict != null) {
                return;
            }

            _environmentInfoDict = new();
            foreach (var info in _environmentInfos) {
                _environmentInfoDict[info.key] = info;
            }
            
            // 一旦すべて無効化
            foreach (var info in _environmentInfos) {
                DeactivateEnvironmentInfo(info);
            }
            
            // デフォルトキーのみ有効化
            if (_environmentInfoDict.TryGetValue(_defaultKey, out var defaultInfo)) {
                ActivateEnvironmentInfo(defaultInfo);
            }

            _currentKey = _defaultKey;
        }

        /// <summary>
        /// 環境情報のアクティブ化
        /// </summary>
        private void ActivateEnvironmentInfo(EnvironmentInfo info) {
            if (info.environmentSettings != null) {
                info.environmentSettings.enabled = true;
            }
        }

        /// <summary>
        /// 環境情報の非アクティブ化
        /// </summary>
        private void DeactivateEnvironmentInfo(EnvironmentInfo info) {
            if (info.environmentSettings != null) {
                info.environmentSettings.enabled = false;
            }
        }
    }
}