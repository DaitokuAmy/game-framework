using System;
using System.Collections.Generic;

namespace GameFramework.UISystems {
    /// <summary>
    /// ISelectableItemを管理するためのクラス
    /// </summary>
    public class SelectableItemGroup : IDisposable {
        private readonly List<ISelectableItem> _items = new();
        private readonly ISelectableItemGroupHandler _handler;

        private int _currentIndex;

        /// <summary>選択中の項目</summary>
        public ISelectableItem CurrentItem => _currentIndex < 0 || _currentIndex >= _items.Count ? null : _items[_currentIndex];
        /// <summary>選択中項目のIndex</summary>
        public int CurrentItemIndex => _currentIndex;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectableItemGroup(ISelectableItemGroupHandler handler = null) {
            _handler = handler;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            Clear();
        }

        /// <summary>
        /// 項目の追加
        /// </summary>
        public void Add(ISelectableItem item) {
            _items.Add(item);
            if (CurrentItem == null) {
                _currentIndex = _items.Count - 1;
                item.OnSelectedItem();
                _handler?.OnChangedCurrentItem(_currentIndex, item);
            }
        }

        /// <summary>
        /// 項目の除外
        /// </summary>
        public bool Remove(ISelectableItem item) {
            var result = _items.Remove(item);
            if (result) {
                if (CurrentItem == null) {
                    _currentIndex--;
                    _handler?.OnChangedCurrentItem(_currentIndex, CurrentItem);
                }
            }

            return result;
        }

        /// <summary>
        /// 項目の全クリア
        /// </summary>
        public void Clear() {
            CurrentItem?.OnDeselectedItem();
            _items.Clear();
            _currentIndex = -1;
            _handler?.OnChangedCurrentItem(_currentIndex, CurrentItem);
        }

        /// <summary>
        /// 選択
        /// </summary>
        public void Select(int index) {
            if (index < 0 || index >= _items.Count) {
                DebugLog.Warning($"Invalid selectable index. [{index}]");
                return;
            }

            if (index == _currentIndex) {
                return;
            }

            // 既存の項目があれば選択解除
            CurrentItem?.OnDeselectedItem();

            // 新しい項目の選択
            var item = _items[index];
            _currentIndex = index;
            item.OnSelectedItem();
            
            _handler?.OnChangedCurrentItem(_currentIndex, CurrentItem);
        }
    }
}