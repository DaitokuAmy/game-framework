using System;
using System.Buffers;
using UnityEngine;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// DOTween風Sequence（Append/Join/Insertのタイムライン実行）
    /// </summary>
    public sealed class Sequence : Tween {
        /// <summary>
        /// タイムライン上の配置情報
        /// </summary>
        private struct Item {
            public Tween Tween;
            public float Start;
            public float End;
        }

        private TweenPlayer _owner = null!;

        private Item[] _items = null!;
        private int _itemsCount;
        private int _itemsCapacity;

        private float _time;
        private float _duration;

        private float _cursor;
        private float _lastAppendStart;
        private float _groupEnd;

        /// <inheritdoc/>
        public override float Duration => _duration;

        /// <inheritdoc/>
        protected override void OnBegin() {
            // 子のBeginは開始時刻に到達した瞬間に呼び出す
        }

        /// <inheritdoc/>
        protected override void OnTick(float deltaTime) {
            var prev = _time;
            _time += deltaTime;

            for (var i = 0; i < _itemsCount; i++) {
                ref var item = ref _items[i];

                if (item.Tween.IsComplete || item.Tween.IsKilled) {
                    continue;
                }

                if (_time < item.Start) {
                    continue;
                }

                if (prev <= item.Start) {
                    item.Tween.BeginInternal();
                }

                var tickStart = Mathf.Max(prev, item.Start);
                var tickEnd = Mathf.Min(_time, item.End);
                var dt = tickEnd - tickStart;

                if (dt > 0.0f) {
                    item.Tween.TickInternal(dt);
                }
            }

            if (_time >= _duration) {
                CompleteInternal();
            }
        }

        /// <inheritdoc/>
        protected override void OnForceComplete() {
            var remaining = _duration - _time;
            if (remaining > 0.0f) {
                OnTick(remaining);
            }
        }

        /// <inheritdoc/>
        protected override void OnKill() {
            for (var i = 0; i < _itemsCount; i++) {
                _items[i].Tween.Kill();
            }
        }

        /// <inheritdoc/>
        protected override void OnReset() {
            for (var i = 0; i < _itemsCount; i++) {
                _items[i].Tween?.ClearSequenceOwnerInternal(this);
            }

            _owner = null!;
            _time = 0.0f;
            _duration = 0.0f;

            _cursor = 0.0f;
            _lastAppendStart = 0.0f;
            _groupEnd = 0.0f;

            if (_items != null) {
                ArrayPool<Item>.Shared.Return(_items, clearArray: true);
                _items = null!;
            }

            _itemsCount = 0;
            _itemsCapacity = 0;
        }

        /// <summary>
        /// PlayerがPoolから借りた直後の初期化
        /// </summary>
        public void Setup(TweenPlayer owner) {
            _owner = owner;
            _itemsCount = 0;
            _time = 0.0f;
            _duration = 0.0f;

            _cursor = 0.0f;
            _lastAppendStart = 0.0f;
            _groupEnd = 0.0f;
        }

        /// <summary>
        /// 直列に追加（末尾に積む）
        /// </summary>
        public Sequence Append(Tween tween) {
            ValidateCanModify(tween);
            EnsureCapacity(_itemsCount + 1);

            var start = _cursor;
            var end = start + tween.Duration;

            _items[_itemsCount++] = new Item { Tween = tween, Start = start, End = end };
            tween.SetSequenceOwnerInternal(this);

            _lastAppendStart = start;
            _groupEnd = Mathf.Max(_groupEnd, end);

            _cursor = _groupEnd;
            _duration = Mathf.Max(_duration, _cursor);

            return this;
        }

        /// <summary>
        /// 直前のAppend開始時刻に並列追加
        /// </summary>
        public Sequence Join(Tween tween) {
            ValidateCanModify(tween);
            EnsureCapacity(_itemsCount + 1);

            var start = _lastAppendStart;
            var end = start + tween.Duration;

            _items[_itemsCount++] = new Item { Tween = tween, Start = start, End = end };
            tween.SetSequenceOwnerInternal(this);

            _groupEnd = Mathf.Max(_groupEnd, end);

            _cursor = _groupEnd;
            _duration = Mathf.Max(_duration, _cursor);

            return this;
        }

        /// <summary>
        /// 指定時刻に挿入（任意時刻開始）
        /// </summary>
        public Sequence Insert(float at, Tween tween) {
            ValidateCanModify(tween);
            EnsureCapacity(_itemsCount + 1);

            var start = Mathf.Max(0.0f, at);
            var end = start + tween.Duration;

            _items[_itemsCount++] = new Item { Tween = tween, Start = start, End = end };
            tween.SetSequenceOwnerInternal(this);

            _duration = Mathf.Max(_duration, end);

            return this;
        }

        /// <summary>
        /// インターバル（Delay）をAppend
        /// </summary>
        public Sequence AppendInterval(float seconds) {
            ValidateCanModify();
            var delay = _owner.CreateTweener<DelayTweener>();
            delay.Setup(seconds);
            return Append(delay);
        }

        /// <summary>
        /// 子Tween数を取得（Playerがツリー回収に使用）
        /// </summary>
        public int GetChildCount() {
            return _itemsCount;
        }

        /// <summary>
        /// 子Tweenを取得（Playerがツリー回収に使用）
        /// </summary>
        public Tween GetChild(int index) {
            if ((uint)index >= (uint)_itemsCount) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _items[index].Tween;
        }

        /// <summary>
        /// 内部配列容量を確保
        /// </summary>
        public void EnsureCapacity(int required) {
            if (_itemsCapacity >= required) {
                return;
            }

            var newCapacity = Mathf.Max(8, _itemsCapacity * 2);
            while (newCapacity < required) {
                newCapacity *= 2;
            }

            var newArr = ArrayPool<Item>.Shared.Rent(newCapacity);

            if (_itemsCount > 0 && _items != null) {
                System.Array.Copy(_items, 0, newArr, 0, _itemsCount);
                ArrayPool<Item>.Shared.Return(_items, clearArray: true);
            }

            _items = newArr;
            _itemsCapacity = newCapacity;
        }

        /// <summary>
        /// 再生開始後に Sequence を変更しないことを検証
        /// </summary>
        private void ValidateCanModify() {
            if (!IsIdleInternal) {
                throw new InvalidOperationException("Cannot modify a sequence after playback has started.");
            }
        }

        /// <summary>
        /// 追加対象の Tween が Sequence に組み込める状態かを検証
        /// </summary>
        private void ValidateCanModify(Tween tween) {
            if (tween == null) {
                throw new ArgumentNullException(nameof(tween));
            }

            ValidateCanModify();

            if (ReferenceEquals(tween, this)) {
                throw new InvalidOperationException("A sequence cannot contain itself.");
            }

            if (!tween.IsIdleInternal) {
                throw new InvalidOperationException("Tween must be idle before adding to a sequence.");
            }

            if (tween.HasSequenceOwnerInternal) {
                throw new InvalidOperationException("Tween is already owned by a sequence.");
            }
        }
    }
}
