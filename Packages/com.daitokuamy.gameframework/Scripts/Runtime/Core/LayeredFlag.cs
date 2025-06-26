using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// レイヤー管理可能フラグ
    /// </summary>
    public struct LayeredFlag : IEquatable<LayeredFlag> {
        /// <summary>空のフラグ</summary>
        public static readonly LayeredFlag Empty = new();
        
        private List<uint> _bitFlags;

        /// <summary>ビットフラグ</summary>
        public IReadOnlyList<uint> BitFlags {
            get {
                if (_bitFlags == null) {
                    _bitFlags = new List<uint>();
                }

                return _bitFlags;
            }
        }
        /// <summary>フラグがONか</summary>
        public bool IsOn {
            get {
                var result = 0U;
                for (var i = 0; i < BitFlags.Count; i++) {
                    result |= BitFlags[i];
                }

                return result != 0;
            }
        }
        /// <summary>インデクサ</summary>
        public bool this[int index] {
            get => Check(index);
            set => Set(index, value);
        }
        /// <summary>インデクサ</summary>
        public bool this[Enum enumValue] {
            get => Check(enumValue);
            set => Set(enumValue, value);
        }

        /// <summary>
        /// コンストラクタ
        /// <param name="onIndices">ON状態のIndex配列</param>
        /// </summary>
        public LayeredFlag(params int[] onIndices)
            : this() {
            _bitFlags = new();
            On(onIndices);
        }

        /// <summary>
        /// コンストラクタ
        /// <param name="onEnumValues">ON状態のIndex配列(Enum)</param>
        /// </summary>
        public LayeredFlag(params Enum[] onEnumValues)
            : this() {
            _bitFlags = new();
            On(onEnumValues);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="flag">初期値フラグ</param>
        public LayeredFlag(LayeredFlag flag)
            : this() {
            _bitFlags = new List<uint>(flag._bitFlags);
        }

        /// <summary>
        /// 等価比較演算
        /// </summary>
        public static bool operator ==(LayeredFlag lhs, LayeredFlag rhs) {
            var maxFlagCount = Mathf.Max(lhs.BitFlags.Count, rhs.BitFlags.Count);

            for (var i = 0; i < maxFlagCount; i++) {
                var maskA = i < lhs.BitFlags.Count ? lhs.BitFlags[i] : 0;
                var maskB = i < rhs.BitFlags.Count ? rhs.BitFlags[i] : 0;

                if (maskA != maskB) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 等価比較演算
        /// </summary>
        public static bool operator ==(LayeredFlag lhs, bool rhs) {
            return rhs ? lhs.IsOn : !lhs.IsOn;
        }

        /// <summary>
        /// 不等価比較演算
        /// </summary>
        public static bool operator !=(LayeredFlag lhs, LayeredFlag rhs) {
            return !(lhs == rhs);
        }

        /// <summary>
        /// 不等価比較演算
        /// </summary>
        public static bool operator !=(LayeredFlag lhs, bool rhs) {
            return !(lhs == rhs);
        }

        /// <summary>
        /// true判定
        /// </summary>
        public static bool operator true(LayeredFlag flag) {
            return flag.IsOn;
        }

        /// <summary>
        /// false判定
        /// </summary>
        public static bool operator false(LayeredFlag flag) {
            return !flag.IsOn;
        }

        /// <summary>
        /// boolキャスト
        /// </summary>
        public static implicit operator bool(LayeredFlag flag) {
            return flag.IsOn;
        }

        /// <summary>
        /// AND演算
        /// </summary>
        public static LayeredFlag operator &(LayeredFlag lhs, LayeredFlag rhs) {
            var result = new LayeredFlag();
            result._bitFlags = new List<uint>();
            var flagCount = Mathf.Max(lhs.BitFlags.Count, rhs.BitFlags.Count);

            for (var i = 0; i < flagCount; i++) {
                var maskA = i < lhs.BitFlags.Count ? lhs.BitFlags[i] : 0;
                var maskB = i < rhs.BitFlags.Count ? rhs.BitFlags[i] : 0;
                result._bitFlags.Add(maskA & maskB);
            }

            return result;
        }

        /// <summary>
        /// OR演算
        /// </summary>
        public static LayeredFlag operator |(LayeredFlag lhs, LayeredFlag rhs) {
            var result = new LayeredFlag();
            result._bitFlags = new List<uint>();
            var flagCount = Mathf.Max(lhs.BitFlags.Count, rhs.BitFlags.Count);

            for (var i = 0; i < flagCount; i++) {
                var maskA = i < lhs.BitFlags.Count ? lhs.BitFlags[i] : 0;
                var maskB = i < rhs.BitFlags.Count ? rhs.BitFlags[i] : 0;
                result._bitFlags.Add(maskA | maskB);
            }

            return result;
        }

        /// <summary>
        /// EOR演算
        /// </summary>
        public static LayeredFlag operator ^(LayeredFlag lhs, LayeredFlag rhs) {
            var result = new LayeredFlag();
            result._bitFlags = new List<uint>();
            var flagCount = Mathf.Max(lhs.BitFlags.Count, rhs.BitFlags.Count);

            for (var i = 0; i < flagCount; i++) {
                var maskA = i < lhs.BitFlags.Count ? lhs.BitFlags[i] : 0;
                var maskB = i < rhs.BitFlags.Count ? rhs.BitFlags[i] : 0;
                result._bitFlags.Add(maskA ^ maskB);
            }

            return result;
        }

        /// <summary>
        /// 否定演算
        /// </summary>
        public static bool operator !(LayeredFlag flag) {
            return !flag.IsOn;
        }

        /// <summary>
        /// 論理否定演算
        /// </summary>
        public static LayeredFlag operator ~(LayeredFlag flag) {
            var result = flag;

            if (result._bitFlags == null) {
                result._bitFlags = new();
            }

            for (var i = 0; i < flag.BitFlags.Count; i++) {
                result._bitFlags[i] = ~flag.BitFlags[i];
            }

            return result;
        }

        /// <summary>
        /// 文字列化
        /// </summary>
        public override string ToString() {
            if (_bitFlags == null) {
                return "0";
            }
            
            var builder = new StringBuilder();

            for (var i = 0; i < _bitFlags.Count; i++) {
                builder.AppendLine($"[{i}] > {Convert.ToString(_bitFlags[i], 2)}");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Hash計算
        /// </summary>
        public override int GetHashCode() {
            if (_bitFlags == null) {
                return 0;
            }
            
            var hash = 0U;
            for (var i = 0; i < _bitFlags.Count; i++) {
                hash ^= _bitFlags[i];
            }

            return (int)hash;
        }

        /// <summary>
        /// 比較
        /// </summary>
        public override bool Equals(object obj) {
            return obj is LayeredFlag other && Equals(other);
        }

        /// <summary>
        /// 等価判定
        /// </summary>
        public bool Equals(LayeredFlag other) {
            return this == other;
        }

        /// <summary>
        /// Flagの設定
        /// </summary>
        /// <param name="index">設定したいIndex</param>
        /// <param name="on">フラグの状態</param>
        public void Set(int index, bool on) {
            RebuildBitFlags(index);

            var flagIndex = index / 32;
            var mask = 1U << (index % 32);

            if (on) {
                _bitFlags[flagIndex] |= mask;
            }
            else {
                _bitFlags[flagIndex] &= ~mask;
            }
        }

        /// <summary>
        /// Flagの設定
        /// </summary>
        /// <param name="enumValue">設定したいIndexを表すEnum</param>
        /// <param name="on">フラグの状態</param>
        public void Set(Enum enumValue, bool on) {
            Set(Convert.ToInt32(enumValue), on);
        }

        /// <summary>
        /// Flagの設定
        /// </summary>
        /// <param name="on">フラグの状態</param>
        /// <param name="indices">設定したいIndexリスト</param>
        public void Set(bool on, params int[] indices) {
            for (var i = 0; i < indices.Length; i++) {
                Set(indices[i], on);
            }
        }

        /// <summary>
        /// Flagの設定
        /// </summary>
        /// <param name="on">フラグの状態</param>
        /// <param name="enumValues">設定したいIndexを表すEnumリスト</param>
        public void Set(bool on, params Enum[] enumValues) {
            for (var i = 0; i < enumValues.Length; i++) {
                Set(Convert.ToInt32(enumValues[i]), on);
            }
        }

        /// <summary>
        /// FlagのON設定
        /// </summary>
        /// <param name="indices">設定したいIndexリスト</param>
        public void On(params int[] indices) {
            Set(true, indices);
        }

        /// <summary>
        /// FlagのON設定
        /// </summary>
        /// <param name="enumValues">設定したいIndexを表すEnumリスト</param>
        public void On(params Enum[] enumValues) {
            Set(true, enumValues);
        }

        /// <summary>
        /// FlagのOFF設定
        /// </summary>
        /// <param name="indices">設定したいIndexリスト</param>
        public void Off(params int[] indices) {
            Set(false, indices);
        }

        /// <summary>
        /// FlagのOFF設定
        /// </summary>
        /// <param name="enumValues">設定したいIndexを表すEnumリスト</param>
        public void Off(params Enum[] enumValues) {
            Set(false, enumValues);
        }

        /// <summary>
        /// フラグのクリア
        /// </summary>
        public void Clear() {
            if (_bitFlags == null) {
                return;
            }
            
            _bitFlags.Clear();
        }

        /// <summary>
        /// ビット単位でのフラグチェック
        /// </summary>
        /// <param name="indices">確認したいIndexリスト</param>
        public bool Check(params int[] indices) {
            var maxIndex = indices.Max();
            RebuildBitFlags(maxIndex);
            
            for (var i = 0; i < indices.Length; i++) {
                var index = indices[i];
                if (index < 0) {
                    continue;
                }
                
                var flagIndex = index / 32;
                var mask = 1U << (index % 32);

                if ((_bitFlags[flagIndex] & mask) != 0) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ビット単位でのフラグチェック
        /// </summary>
        /// <param name="enumValues">確認したいIndexを表すEnumリスト</param>
        public bool Check(params Enum[] enumValues) {
            return Check(enumValues.Select(x => Convert.ToInt32(x)).ToArray());
        }

        /// <summary>
        /// BitFlagのリビルド
        /// </summary>
        /// <param name="index">フラグに登録するIndex</param>
        private void RebuildBitFlags(int index) {
            if (index < 0) {
                return;
            }

            if (_bitFlags == null) {
                _bitFlags = new();
            }

            var flagCount = index / 32 + 1;

            while (flagCount > _bitFlags.Count) {
                _bitFlags.Add(0);
            }
        }
    }
}