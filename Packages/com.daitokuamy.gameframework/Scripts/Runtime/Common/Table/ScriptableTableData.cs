using System.Collections.Generic;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// 要素をID管理するのテーブルデータ
    /// </summary>
    public class ScriptableTableData<TKey, TElement> : ScriptableObject
        where TElement : ITableElement<TKey> {
        [Tooltip("要素リスト")]
        public TElement[] elements;

        private Dictionary<TKey, TElement> _elementCaches;

        /// <summary>
        /// IDを元に要素を検索
        /// </summary>
        public TElement FindById(TKey id) {
            if (_elementCaches == null) {
                RefreshCaches();
            }

            if (_elementCaches != null && _elementCaches.TryGetValue(id, out var element)) {
                return element;
            }

            return default;
        }

        /// <summary>
        /// 更新通知
        /// </summary>
        private void OnValidate() {
            RefreshCaches();
        }

        /// <summary>
        /// キャッシュのリフレッシュ
        /// </summary>
        private void RefreshCaches() {
            _elementCaches = new Dictionary<TKey, TElement>();
            foreach (var element in elements) {
                _elementCaches[element.Id] = element;
            }
        }
    }
}