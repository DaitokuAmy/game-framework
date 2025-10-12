using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEngine.Pool;

namespace GameFramework.UISystems {
    /// <summary>
    /// ISelectableItemを管理するためのクラス
    /// </summary>
    public class SelectableItemGroup : IDisposable {
        /// <summary>
        /// 項目情報
        /// </summary>
        private class ItemInfo {
            public ISelectableItem Item;
            public bool Disabled;
        }

        private readonly List<ItemInfo> _itemInfos = new();
        private readonly ISelectableItemGroupHandler _handler;
        private readonly ObjectPool<ItemInfo> _itemInfoPool;

        private int _currentIndex;

        /// <summary>選択中の項目</summary>
        public ISelectableItem CurrentItem => _currentIndex < 0 || _currentIndex >= _itemInfos.Count ? null : _itemInfos[_currentIndex].Item;
        /// <summary>選択中項目のIndex</summary>
        public int CurrentItemIndex => _currentIndex;
        /// <summary>項目数</summary>
        public int ItemCount => _itemInfos.Count;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectableItemGroup(ISelectableItemGroupHandler handler = null) {
            _handler = handler;
            _itemInfoPool = new ObjectPool<ItemInfo>(() => new ItemInfo(), actionOnRelease: info => {
                info.Item = null;
                info.Disabled = false;
            });
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            Clear();
            _itemInfoPool.Dispose();
        }

        /// <summary>
        /// 項目の追加
        /// </summary>
        public void Add(ISelectableItem item, bool disabled = false) {
            var itemInfo = _itemInfoPool.Get();
            itemInfo.Item = item;
            itemInfo.Disabled = disabled;
            _itemInfos.Add(itemInfo);
            if (CurrentItem == null && !disabled) {
                _currentIndex = _itemInfos.Count - 1;
                item.OnSelectedItem();
                _handler?.OnChangedCurrentItem(_currentIndex, item);
            }
        }

        /// <summary>
        /// 項目の除外
        /// </summary>
        public bool Remove(ISelectableItem item) {
            var success = false;
            for (var i = 0; i < _itemInfos.Count; i++) {
                if (_itemInfos[i].Item == item) {
                    _itemInfoPool.Release(_itemInfos[i]);
                    _itemInfos.RemoveAt(i);
                    success = true;
                    break;
                }
            }

            if (success) {
                if (CurrentItem == null) {
                    _currentIndex--;
                    CurrentItem?.OnSelectedItem();
                    _handler?.OnChangedCurrentItem(_currentIndex, CurrentItem);
                }
            }

            return success;
        }

        /// <summary>
        /// 項目の全クリア
        /// </summary>
        public void Clear() {
            CurrentItem?.OnDeselectedItem();

            for (var i = 0; i < _itemInfos.Count; i++) {
                _itemInfoPool.Release(_itemInfos[i]);
            }

            _itemInfos.Clear();
            _currentIndex = -1;
            _handler?.OnChangedCurrentItem(_currentIndex, CurrentItem);
        }

        /// <summary>
        /// 選択
        /// </summary>
        public void Select(int index) {
            if (index < 0 || index >= _itemInfos.Count) {
                DebugLog.Warning($"Invalid selectable index. [{index}]");
                return;
            }

            if (index == _currentIndex) {
                return;
            }

            // 選択不可能アイテムだった場合は無視
            var itemInfo = _itemInfos[index];
            if (itemInfo.Disabled) {
                return;
            }

            // 既存の項目があれば選択解除
            CurrentItem?.OnDeselectedItem();

            // 新しい項目の選択
            _currentIndex = index;
            itemInfo.Item.OnSelectedItem();
            _handler?.OnChangedCurrentItem(_currentIndex, CurrentItem);
        }

        /// <summary>
        /// 選択肢を先に進める
        /// </summary>
        public void Next(bool repeat = false) {
            if (_itemInfos.Count <= 0) {
                return;
            }

            if (CurrentItem == null) {
                return;
            }
            
            var index = _currentIndex;
            if (repeat) {
                do {
                    index = (index + 1) % _itemInfos.Count;
                    if (!_itemInfos[index].Disabled) {
                        break;
                    }
                } while (index != _currentIndex);

                Select(index);
            }
            else {
                do {
                    index = IntMath.Min(_itemInfos.Count - 1, index + 1);
                    if (!_itemInfos[index].Disabled) {
                        break;
                    }
                } while (index < _itemInfos.Count - 1);

                Select(index);
            }
        }

        /// <summary>
        /// 選択肢を前に戻す
        /// </summary>
        public void Previous(bool repeat = false) {
            if (_itemInfos.Count <= 0) {
                return;
            }

            if (CurrentItem == null) {
                return;
            }
            
            var index = _currentIndex;
            if (repeat) {
                do {
                    index = (index - 1 + _itemInfos.Count) % _itemInfos.Count;
                    if (!_itemInfos[index].Disabled) {
                        break;
                    }
                } while (index != _currentIndex);

                Select(index);
            }
            else {
                do {
                    index = IntMath.Max(0, index - 1);
                    if (!_itemInfos[index].Disabled) {
                        break;
                    }
                } while (index > 0);

                Select(index);
            }
        }

        /// <summary>
        /// 選択無効状態の設定
        /// </summary>
        public void SetDisabled(int index, bool disabled) {
            if (index < 0 || index >= _itemInfos.Count) {
                DebugLog.Warning($"Invalid selectable index. [{index}]");
                return;
            }

            if (_itemInfos[index].Disabled == disabled) {
                return;
            }

            _itemInfos[index].Disabled = disabled;

            // 無効にした時
            if (disabled) {
                // 選択対象だった場合、選択対象をずらす
                if (index == _currentIndex) {
                    // 選択をずらす(後優先)
                    var found = false;
                    for (var i = index + 1; i < _itemInfos.Count; i++) {
                        var itemInfo = _itemInfos[i];
                        if (!itemInfo.Disabled) {
                            Select(i);
                            found = true;
                            break;
                        }
                    }

                    // 選択対象を前にずらす
                    if (!found) {
                        for (var i = index - 1; i >= 0; i--) {
                            var itemInfo = _itemInfos[i];
                            if (!itemInfo.Disabled) {
                                Select(i);
                                found = true;
                                break;
                            }
                        }
                    }

                    // それでも見つからないなら未選択
                    if (!found) {
                        _itemInfos[index].Item.OnDeselectedItem();
                        _currentIndex = -1;
                        _handler?.OnChangedCurrentItem(_currentIndex, CurrentItem);
                    }
                }
            }
            // 無効を解除した時
            else {
                // 選択対象がなかった状態ならば選択対象にする
                if (CurrentItem == null) {
                    Select(index);
                }
            }
        }
    }
}