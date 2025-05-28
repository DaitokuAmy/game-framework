using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.TableSystems {
    /// <summary>
    /// 要素をID管理するのテーブルデータ
    /// </summary>
    public class ScriptableTableData<TElement> : ScriptableObject
        where TElement : ITableElement {
        [Tooltip("要素リスト")]
        public TElement[] elements;

        private Dictionary<int, TElement> _elementCaches;

        /// <summary>
        /// IDを元に要素を検索
        /// </summary>
        public TElement FindById(int id) {
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
            _elementCaches = new Dictionary<int, TElement>();
            foreach (var element in elements) {
                _elementCaches[element.Id] = element;
            }
        }
    }
}