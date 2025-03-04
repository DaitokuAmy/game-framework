using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// Gimmick管理用クラス
    /// </summary>
    [ExecuteAlways][DisallowMultipleComponent]
    public class GimmickGroup : MonoBehaviour {
        // ギミック情報
        [Serializable]
        public class GimmickInfo {
            public string key;
            public Gimmick gimmick;
        }

        [SerializeField, Tooltip("Gimmick情報")]
        private GimmickInfo[] _gimmickInfos = Array.Empty<GimmickInfo>();
        [SerializeField]
        private GameObject _gimmickRoot;

        // ギミック情報
        private Dictionary<string, List<Gimmick>> _gimmicks = new Dictionary<string, List<Gimmick>>();
        // 初期化フラグ
        private bool _initialized = false;

        // ギミック一覧
        public IReadOnlyList<GimmickInfo> GimmickInfos => _gimmickInfos;

        /// <summary>
        /// ギミックの取得
        /// </summary>
        /// <param name="key">ギミックを表すキー</param>
        public T[] GetGimmicks<T>(string key)
            where T : Gimmick {
            Initialize();

            if (!_gimmicks.TryGetValue(key, out var list)) {
                return Array.Empty<T>();
            }

            return list.OfType<T>().ToArray();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize() {
            if (_initialized) {
                return;
            }

            // Gimmick取得用辞書登録
            _gimmicks.Clear();

            foreach (var info in _gimmickInfos) {
                if (!_gimmicks.TryGetValue(info.key, out var list)) {
                    list = new List<Gimmick>();
                    _gimmicks[info.key] = list;
                }

                list.Add(info.gimmick);
            }

            _initialized = true;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            if (_gimmickRoot == null) {
                _gimmickRoot = new GameObject($"{nameof(GimmickGroup)}_gimmicks", typeof(GimmickRoot));
                _gimmickRoot.transform.SetParent(transform, false);
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            DestroyImmediate(_gimmickRoot);
            _gimmickRoot = null;
            _gimmickInfos = Array.Empty<GimmickInfo>();
        }
    }
}